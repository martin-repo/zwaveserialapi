// -------------------------------------------------------------------------------------------------
// <copyright file="RemoveNodeSequence.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion.RemoveNodeFromNetwork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Protocol;

    internal class RemoveNodeSequence : NodeSequence
    {
        public RemoveNodeSequence(
            ILogger logger,
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
                FunctionType.RemoveNodeFromNetwork,
                CreateInitiateLearnFunctionTx(callbackFuncId))
        {
        }

        protected override Task<NodeTimeouts> InitializeAsync(CancellationToken cancellationToken)
        {
            var nodeTimeouts = new NodeTimeouts(
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromSeconds(60),
                TimeSpan.FromSeconds(14),
                TimeSpan.FromSeconds(14));
            return Task.FromResult(nodeTimeouts);
        }

        private static IFunctionTx CreateInitiateLearnFunctionTx(byte callbackFuncId)
        {
            return RemoveNodeFromNetworkTx.Create(NodeMode.Any | NodeMode.OptionNormalPower, callbackFuncId);
        }
    }
}