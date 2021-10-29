// -------------------------------------------------------------------------------------------------
// <copyright file="AddNodeSequence.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.AddNodeToNetwork
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Protocol;

    internal class AddNodeSequence
    {
        private readonly AddNodeToNetworkTx _addNodeToNetworkTx;
        private readonly byte _callbackFuncId;
        private readonly ZWaveSerialClient _client;
        private readonly byte _listeningNodeCount;
        private readonly ILogger _logger;
        private readonly ZWaveSerialPort _port;
        private readonly object _receiveTargetLock = new();

        private TaskCompletionSource<AddNodeToNetworkRx> _receiveDataCompletionSource = new();
        private AddNodeStatus[] _receiveStatuses = Array.Empty<AddNodeStatus>();

        public AddNodeSequence(
            ILogger logger,
            byte listeningNodeCount,
            ZWaveSerialClient client,
            ZWaveSerialPort port,
            byte callbackFuncId)
        {
            _logger = logger.ForContext<AddNodeSequence>().ForContext(Constants.ClassName, GetType().Name);
            _listeningNodeCount = listeningNodeCount;
            _client = client;
            _port = port;
            _callbackFuncId = callbackFuncId;
            _addNodeToNetworkTx = AddNodeToNetworkTx.Create(AddNodeToNetworkTx.DefaultAddMode, callbackFuncId);
        }

        public async Task<AddedNode> ExecuteAsync(CancellationToken cancellationToken)
        {
            await StopPreviousSessionAsync(cancellationToken);

            var memoryGetIdResponsea = await _client.MemoryGetIdAsync(cancellationToken);
            var sucNodeId = await _client.GetSucNodeIdAsync(cancellationToken);
            var hasSucNode = memoryGetIdResponsea.NodeId == sucNodeId;

            var initData = await _client.SerialApiGetInitDataAsync(cancellationToken);

            var nodeAddSuccess = false;

            _port.FrameReceived += OnFrameReceived;
            try
            {
                var nodeDiscoveryDeadline = DateTime.UtcNow.AddSeconds(60);

                var startNodeAddTimeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                await StartNodeAddAsync(startNodeAddTimeoutTask, cancellationToken);

                var nodeWaitTimeoutTask = Task.Delay(nodeDiscoveryDeadline - DateTime.UtcNow, cancellationToken);
                await WaitForNodeAsync(nodeWaitTimeoutTask, cancellationToken);

                var addNodeToNetworkRx = await WaitForNodeTypeAsync();
                switch (addNodeToNetworkRx.Status)
                {
                    case AddNodeStatus.AddingSlave:
                        var slaveTimeout = TimeSpan.FromSeconds(76) + TimeSpan.FromMilliseconds(217 * _listeningNodeCount);
                        var slaveTimeoutTask = Task.Delay(slaveTimeout, CancellationToken.None);
                        nodeAddSuccess = await WaitForSlaveCompletionAsync(slaveTimeoutTask);
                        break;
                    case AddNodeStatus.AddingController:
                        var controllerTimeout = TimeSpan.FromSeconds(76)
                                                + TimeSpan.FromMilliseconds(217 * _listeningNodeCount)
                                                + TimeSpan.FromMilliseconds(732 * initData.DeviceNodeIds.Length);
                        var controllerTimeoutTask = Task.Delay(controllerTimeout, CancellationToken.None);
                        nodeAddSuccess = await WaitForControllerCompletionAsync(hasSucNode, controllerTimeoutTask);
                        break;
                }

                return new AddedNode(
                    addNodeToNetworkRx.SourceNodeId,
                    addNodeToNetworkRx.DeviceClass,
                    addNodeToNetworkRx.CommandClasses,
                    nodeAddSuccess);
            }
            finally
            {
                _port.FrameReceived -= OnFrameReceived;
            }
        }

        private void OnFrameReceived(object? sender, FrameEventArgs eventArgs)
        {
            var frame = eventArgs.Frame;
            if (frame.Type != FrameType.Request || !_addNodeToNetworkTx.IsValidReturnValue(frame.SerialCommandBytes))
            {
                return;
            }

            var addNodeToNetworkRx = (AddNodeToNetworkRx)_addNodeToNetworkTx.CreateReturnValue(frame.SerialCommandBytes);

            lock (_receiveTargetLock)
            {
                if (_receiveStatuses.Any(status => status == addNodeToNetworkRx.Status))
                {
                    _receiveDataCompletionSource.TrySetResult(addNodeToNetworkRx);
                }
            }
        }

        private void SetReceiveTarget(params AddNodeStatus[] statuses)
        {
            lock (_receiveTargetLock)
            {
                _receiveDataCompletionSource = new TaskCompletionSource<AddNodeToNetworkRx>();
                _receiveStatuses = statuses;
            }
        }

        private async Task StartNodeAddAsync(Task timeoutTask, CancellationToken cancellationToken)
        {
            SetReceiveTarget(AddNodeStatus.LearnReady);
            try
            {
                await _client.InvokeVoidFunctionAsync(_addNodeToNetworkTx, cancellationToken);
                if (await Task.WhenAny(_receiveDataCompletionSource.Task, timeoutTask) != _receiveDataCompletionSource.Task)
                {
                    await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, 0), cancellationToken);
                    throw new TimeoutException($"Timeout waiting for {nameof(AddNodeStatus.LearnReady)}.");
                }
            }
            catch (OperationCanceledException)
            {
                SetReceiveTarget(AddNodeStatus.Done);
                await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, _callbackFuncId), CancellationToken.None);
                await _receiveDataCompletionSource.Task;
                throw;
            }
        }

        private async Task StopPreviousSessionAsync(CancellationToken cancellationToken)
        {
            await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, 0), cancellationToken);
        }

        private async Task<bool> WaitForControllerCompletionAsync(bool sucInNetwork, Task timeoutTask)
        {
            var timedOut = false;
            var success = true;

            SetReceiveTarget(AddNodeStatus.ProtocolDone, AddNodeStatus.Failed);
            if (await Task.WhenAny(_receiveDataCompletionSource.Task, timeoutTask) != _receiveDataCompletionSource.Task)
            {
                timedOut = true;
            }
            else if (_receiveDataCompletionSource.Task.Result.Status != AddNodeStatus.ProtocolDone)
            {
                success = false;
            }

            if (!timedOut && success && !sucInNetwork)
            {
                // TODO
                // var result = await _client.SetSucNodeIdAsync(...); => FAILED or SUCCEEDED, ignored
            }

            SetReceiveTarget(AddNodeStatus.Done);
            await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, _callbackFuncId), CancellationToken.None);
            await _receiveDataCompletionSource.Task;
            await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, 0), CancellationToken.None);

            if (timedOut)
            {
                throw new TimeoutException($"Timeout waiting for {nameof(AddNodeStatus.ProtocolDone)} or {nameof(AddNodeStatus.Failed)}.");
            }

            return success;
        }

        private async Task WaitForNodeAsync(Task timeoutTask, CancellationToken cancellationToken)
        {
            SetReceiveTarget(AddNodeStatus.NodeFound);
            try
            {
                if (await Task.WhenAny(_receiveDataCompletionSource.Task, timeoutTask) != _receiveDataCompletionSource.Task)
                {
                    await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, 0), cancellationToken);
                    throw new TimeoutException($"Timeout waiting for {nameof(AddNodeStatus.NodeFound)}.");
                }
            }
            catch (OperationCanceledException)
            {
                SetReceiveTarget(AddNodeStatus.NodeFound, AddNodeStatus.Done);
                await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, _callbackFuncId), CancellationToken.None);
                var receivedData = await _receiveDataCompletionSource.Task;
                if (receivedData.Status != AddNodeStatus.NodeFound)
                {
                    throw;
                }
            }
        }

        private async Task<AddNodeToNetworkRx> WaitForNodeTypeAsync()
        {
            SetReceiveTarget(AddNodeStatus.AddingController, AddNodeStatus.AddingSlave);
            return await _receiveDataCompletionSource.Task;
        }

        private async Task<bool> WaitForSlaveCompletionAsync(Task timeoutTask)
        {
            var timedOut = false;
            var success = true;

            SetReceiveTarget(AddNodeStatus.ProtocolDone, AddNodeStatus.Failed);
            if (await Task.WhenAny(_receiveDataCompletionSource.Task, timeoutTask) != _receiveDataCompletionSource.Task)
            {
                timedOut = true;
            }
            else if (_receiveDataCompletionSource.Task.Result.Status != AddNodeStatus.ProtocolDone)
            {
                success = false;
            }

            SetReceiveTarget(AddNodeStatus.Done);
            await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, _callbackFuncId), CancellationToken.None);
            await _receiveDataCompletionSource.Task;
            await _client.InvokeVoidFunctionAsync(AddNodeToNetworkTx.Create(AddNodeMode.Stop, 0), CancellationToken.None);

            if (timedOut)
            {
                throw new TimeoutException($"Timeout waiting for {nameof(AddNodeStatus.ProtocolDone)} or {nameof(AddNodeStatus.Failed)}.");
            }

            return success;
        }
    }
}