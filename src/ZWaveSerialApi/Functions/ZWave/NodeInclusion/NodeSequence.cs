// -------------------------------------------------------------------------------------------------
// <copyright file="NodeSequence.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Functions.ZWave.NodeInclusion.AddNodeToNetwork;
    using ZWaveSerialApi.Functions.ZWave.NodeInclusion.RemoveNodeFromNetwork;
    using ZWaveSerialApi.Protocol;

    internal abstract class NodeSequence
    {
        private readonly byte _callbackFuncId;
        private readonly ZWaveSerialClient _client;
        private readonly Action? _controllerReadyCallback;
        private readonly IFunctionTx _initiateLearnFunctionTx;
        private readonly ILogger _logger;
        private readonly ZWaveSerialPort _port;

        private readonly BlockingCollection<Frame> _receivedFrames = new(new ConcurrentQueue<Frame>());

        private readonly FunctionType _type;

        protected NodeSequence(
            ILogger logger,
            ZWaveSerialClient client,
            ZWaveSerialPort port,
            byte callbackFuncId,
            Action? controllerReadyCallback,
            FunctionType functionType,
            IFunctionTx initiateLearnFunctionTx)
        {
            _logger = logger.ForContext<NodeSequence>().ForContext(Constants.ClassName, GetType().Name);
            _client = client;
            _port = port;
            _callbackFuncId = callbackFuncId;
            _controllerReadyCallback = controllerReadyCallback;
            _type = functionType;
            _initiateLearnFunctionTx = initiateLearnFunctionTx;
        }

        public async Task<(bool Success, INodeFunctionRx NodeFunctionRx)> ExecuteAsync(
            CancellationToken abortRequestedToken,
            CancellationToken cancellationToken)
        {
            var abortTokenSource = CancellationTokenSource.CreateLinkedTokenSource(abortRequestedToken, cancellationToken);
            var abortToken = abortTokenSource.Token;

            await StopPreviousSessionAsync(abortToken).ConfigureAwait(false);

            _logger.Debug("Starting node sequence");
            var nodeTimeouts = await InitializeAsync(abortToken).ConfigureAwait(false);

            _port.FrameReceived += OnFrameReceived;
            try
            {
                var started = DateTime.UtcNow;
                var protocolReadyTimeout = started.Add(nodeTimeouts.LearnReadyTimeout);
                await WaitForProtocolAsync(protocolReadyTimeout, abortRequestedToken, cancellationToken).ConfigureAwait(false);

                if (_controllerReadyCallback != null)
                {
                    _logger.Debug("Notifying user that controller is ready");
                    _ = Task.Run(_controllerReadyCallback, abortToken);
                }

                var nodeWaitTimeout = started.Add(nodeTimeouts.NodeFoundTimeout);
                await WaitForNodeAsync(nodeWaitTimeout, abortRequestedToken, cancellationToken).ConfigureAwait(false);

                // NOTE! User cannot abort beyond this point
                abortTokenSource.Dispose();

                var nodeFunctionRx = await WaitForNodeTypeAsync(nodeWaitTimeout, cancellationToken).ConfigureAwait(false);

                var nodeOperationSuccess = false;
                switch (nodeFunctionRx.Status)
                {
                    case NodeStatus.Slave:
                        var slaveTimeout = DateTime.UtcNow.Add(nodeTimeouts.SlaveTimeout);
                        _logger.Debug("Waiting for slave completion");
                        nodeOperationSuccess = await WaitForCompletionAsync(
                                                       nodeFunctionRx.SourceNodeId,
                                                       slaveTimeout,
                                                       _ => Task.CompletedTask,
                                                       cancellationToken)
                                                   .ConfigureAwait(false);
                        break;
                    case NodeStatus.Controller:
                        var controllerTimeout = DateTime.UtcNow.Add(nodeTimeouts.ControllerTimeout);
                        _logger.Debug("Waiting for controller completion");
                        nodeOperationSuccess = await WaitForCompletionAsync(
                                                       nodeFunctionRx.SourceNodeId,
                                                       controllerTimeout,
                                                       ControllerSpecificHandlerAsync,
                                                       cancellationToken)
                                                   .ConfigureAwait(false);
                        break;
                }

                return (nodeOperationSuccess, nodeFunctionRx);
            }
            finally
            {
                _port.FrameReceived -= OnFrameReceived;
            }
        }

        protected virtual Task ControllerSpecificHandlerAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected IFunctionTx CreateFunctionTx(NodeMode mode, byte callbackFuncId)
        {
            return _type == FunctionType.AddNodeToNetwork
                       ? AddNodeToNetworkTx.Create(mode, callbackFuncId)
                       : RemoveNodeFromNetworkTx.Create(mode, callbackFuncId);
        }

        protected abstract Task<NodeTimeouts> InitializeAsync(CancellationToken cancellationToken);

        private void OnFrameReceived(object? sender, FrameEventArgs eventArgs)
        {
            _receivedFrames.Add(eventArgs.Frame);
        }

        private async Task<(bool TimedOut, INodeFunctionRx? NodeFunction)> ReceiveNodeFunctionAsync(
            NodeStatus[] statuses,
            DateTime timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var timeoutMilliseconds = (int)(timeout - DateTime.UtcNow).TotalMilliseconds;
            if (timeoutMilliseconds < 0)
            {
                return (true, null);
            }

            return await Task.Run<(bool TimedOut, INodeFunctionRx? NodeFunction)>(
                                 () =>
                                 {
                                     while (_receivedFrames.TryTake(out var frame, timeoutMilliseconds, cancellationToken))
                                     {
                                         if (frame.Type != FrameType.Request
                                             || !_initiateLearnFunctionTx.IsValidReturnValue(frame.SerialCommandBytes))
                                         {
                                             continue;
                                         }

                                         var nodeFunctionRx = (INodeFunctionRx)_initiateLearnFunctionTx.CreateReturnValue(frame.SerialCommandBytes);
                                         if (statuses.Any(status => status == nodeFunctionRx.Status))
                                         {
                                             return (false, nodeFunctionRx);
                                         }
                                     }

                                     return (true, null);
                                 },
                                 cancellationToken)
                             .ConfigureAwait(false);
        }

        private async Task StopPreviousSessionAsync(CancellationToken cancellationToken)
        {
            var addFunctionTx = AddNodeToNetworkTx.Create(NodeMode.Stop, 0);
            await _client.InvokeVoidFunctionAsync(addFunctionTx, cancellationToken).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        private async Task<bool> WaitForCompletionAsync(
            byte nodeId,
            DateTime timeout,
            Func<CancellationToken, Task> successFunc,
            CancellationToken cancellationToken)
        {
            var successStatus = _type == FunctionType.AddNodeToNetwork ? NodeStatus.ProtocolDone : NodeStatus.Done;
            var (timedOut, nodeFunction) = await ReceiveNodeFunctionAsync(new[] { successStatus, NodeStatus.Failed }, timeout, cancellationToken)
                                               .ConfigureAwait(false);
            switch (timedOut)
            {
                case false:
                    _logger.Debug("Invoking completion callback");
                    await successFunc(cancellationToken).ConfigureAwait(false);
                    break;
                case true when _type == FunctionType.AddNodeToNetwork:
                    _logger.Debug("Requesting done");
                    await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, _callbackFuncId), cancellationToken).ConfigureAwait(false);
                    _ = await ReceiveNodeFunctionAsync(new[] { NodeStatus.Done }, timeout, cancellationToken);
                    break;
            }

            _logger.Debug("Sending stop");
            await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, 0), cancellationToken).ConfigureAwait(false);

            if (!timedOut)
            {
                _logger.Debug("Completed");
                return nodeFunction!.Status == successStatus;
            }

            _logger.Debug("Timeout");
            throw new NodeTimeoutException(
                $"Timeout waiting for {successStatus} or {nameof(NodeStatus.Failed)}.",
                nodeId,
                NodeTimeout.AddNodeTimeout);
        }

        private async Task WaitForNodeAsync(DateTime timeout, CancellationToken abortRequestedToken, CancellationToken cancellationToken)
        {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(abortRequestedToken, cancellationToken);
            try
            {
                _logger.Debug("Awaiting node");
                var (timedOut, _) = await ReceiveNodeFunctionAsync(new[] { NodeStatus.NodeFound }, timeout, tokenSource.Token).ConfigureAwait(false);
                if (!timedOut)
                {
                    _logger.Debug("Node found");
                    return;
                }

                _logger.Debug("Timeout - sending stop");
                await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, 0), cancellationToken).ConfigureAwait(false);
                throw new NodeTimeoutException($"Timeout waiting for {nameof(NodeStatus.NodeFound)}.", 0, NodeTimeout.NodeTimeout);
            }
            catch (OperationCanceledException operationCanceledException) when (operationCanceledException.CancellationToken == abortRequestedToken)
            {
                _logger.Debug("User aborted - requesting done");
                await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, _callbackFuncId), cancellationToken).ConfigureAwait(false);
                var (timedOut, nodeFunction) = await ReceiveNodeFunctionAsync(
                                                   new[] { NodeStatus.NodeFound, NodeStatus.Done },
                                                   timeout,
                                                   cancellationToken);
                if (!timedOut && nodeFunction!.Status == NodeStatus.NodeFound)
                {
                    // Received NodeFound while attempting to Stop - valid use case
                    _logger.Debug("Node found after user aborted - continuing flow");
                    return;
                }

                _logger.Debug("User aborted - sending stop");
                await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, 0), cancellationToken).ConfigureAwait(false);

                throw;
            }
        }

        private async Task<INodeFunctionRx> WaitForNodeTypeAsync(DateTime timeout, CancellationToken cancellationToken)
        {
            _logger.Debug("Awaiting node type");
            var (timedOut, nodeFunction) = await ReceiveNodeFunctionAsync(
                                                   new[] { NodeStatus.Controller, NodeStatus.Slave },
                                                   timeout,
                                                   cancellationToken)
                                               .ConfigureAwait(false);
            if (!timedOut)
            {
                _logger.Debug("Node type received");
                return nodeFunction!;
            }

            _logger.Debug("Timeout - requesting done");
            await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, _callbackFuncId), cancellationToken).ConfigureAwait(false);
            _ = await ReceiveNodeFunctionAsync(new[] { NodeStatus.Done }, timeout, cancellationToken);

            _logger.Debug("Timeout - sending stop");
            await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, 0), cancellationToken).ConfigureAwait(false);
            throw new NodeTimeoutException(
                $"Timeout waiting for {nameof(NodeStatus.Controller)} or {nameof(NodeStatus.Slave)}.",
                0,
                NodeTimeout.NodeTimeout);
        }

        private async Task WaitForProtocolAsync(DateTime timeout, CancellationToken abortRequestedToken, CancellationToken cancellationToken)
        {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(abortRequestedToken, cancellationToken);
            try
            {
                _logger.Debug("Requesting learn mode");
                await _client.InvokeVoidFunctionAsync(_initiateLearnFunctionTx, cancellationToken).ConfigureAwait(false);

                _logger.Debug("Awaiting learn mode");
                var (timedOut, nodeFunction) = await ReceiveNodeFunctionAsync(
                                                       new[] { NodeStatus.LearnReady, NodeStatus.NotPrimary },
                                                       timeout,
                                                       tokenSource.Token)
                                                   .ConfigureAwait(false);
                if (!timedOut)
                {
                    if (nodeFunction!.Status == NodeStatus.NotPrimary)
                    {
                        throw new InvalidOperationException("Attempt to add node via non-primary controller.");
                    }

                    _logger.Debug("Controller is in learn mode");
                    return;
                }

                _logger.Debug("Timeout - sending stop");
                await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, 0), cancellationToken).ConfigureAwait(false);
                throw new NodeTimeoutException($"Timeout waiting for {nameof(NodeStatus.LearnReady)}.", 0, NodeTimeout.ProtocolReadyTimeout);
            }
            catch (OperationCanceledException operationCanceledException) when (operationCanceledException.CancellationToken == abortRequestedToken)
            {
                _logger.Debug("User aborted - requesting done");
                await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, _callbackFuncId), cancellationToken).ConfigureAwait(false);
                _ = await ReceiveNodeFunctionAsync(new[] { NodeStatus.Done }, timeout, cancellationToken);

                _logger.Debug("User aborted - sending stop");
                await _client.InvokeVoidFunctionAsync(CreateFunctionTx(NodeMode.Stop, 0), cancellationToken).ConfigureAwait(false);

                throw;
            }
        }
    }
}