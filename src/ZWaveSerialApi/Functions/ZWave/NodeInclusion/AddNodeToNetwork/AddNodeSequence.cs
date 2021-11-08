// -------------------------------------------------------------------------------------------------
// <copyright file="AddNodeSequence.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion.AddNodeToNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Protocol;

    internal class AddNodeSequence : NodeSequence
    {
        private readonly ZWaveSerialClient _client;
        private readonly byte _listeningNodeCount;

        private bool _hasSucNode;

        public AddNodeSequence(
            ILogger logger,
            byte listeningNodeCount,
            ZWaveSerialClient client,
            ZWaveSerialPort port,
            byte callbackFuncId,
            Action? controllerReadyCallback)
            : base(
                logger,
                client,
                port,
                callbackFuncId,
                controllerReadyCallback,
                FunctionType.AddNodeToNetwork,
                CreateInitiateLearnFunctionTx(callbackFuncId))
        {
            _listeningNodeCount = listeningNodeCount;
            _client = client;
        }

        protected override Task ControllerSpecificHandlerAsync(CancellationToken cancellationToken)
        {
            const bool ShouldReplicate = false;
            if (ShouldReplicate)
            {
                throw new NotSupportedException("Controller replication is not supported.");
            }

            if (!_hasSucNode)
            {
                // TODO: var result = await _client.SetSucNodeIdAsync(...).ConfigureAwait(false);
                // TODO: result should be logged, but should not affect node sequence flow
            }

            return Task.CompletedTask;
        }

        protected override async Task<NodeTimeouts> InitializeAsync(CancellationToken cancellationToken)
        {
            var memoryGetIdResponsea = await _client.MemoryGetIdAsync(cancellationToken).ConfigureAwait(false);
            var sucNodeId = await _client.GetSucNodeIdAsync(cancellationToken).ConfigureAwait(false);
            _hasSucNode = memoryGetIdResponsea.NodeId == sucNodeId;

            if (!_hasSucNode)
            {
                throw new NotSupportedException("Networks without a static update controller (SUC) is not supported.");
            }

            var initData = await _client.SerialApiGetInitDataAsync(cancellationToken).ConfigureAwait(false);

            var slaveTimeout = TimeSpan.FromSeconds(76) + TimeSpan.FromMilliseconds(217 * _listeningNodeCount);

            var controllerTimeout = TimeSpan.FromSeconds(76)
                                    + TimeSpan.FromMilliseconds(217 * _listeningNodeCount)
                                    + TimeSpan.FromMilliseconds(732 * initData.DeviceNodeIds.Length);

            return new NodeTimeouts(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60), slaveTimeout, controllerTimeout);
        }

        private static IFunctionTx CreateInitiateLearnFunctionTx(byte callbackFuncId)
        {
            return AddNodeToNetworkTx.Create(NodeMode.Any | NodeMode.OptionNormalPower | NodeMode.OptionNetworkWide, callbackFuncId);
        }
    }
}