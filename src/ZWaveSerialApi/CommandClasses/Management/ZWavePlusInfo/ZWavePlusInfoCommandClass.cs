// -------------------------------------------------------------------------------------------------
// <copyright file="ZWavePlusInfoCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.ZWavePlusInfo
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    public class ZWavePlusInfoCommandClass : CommandClass
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<byte, TaskCompletionSource<ZWavePlusInfoReport>> _reportCallbackSources = new();

        public ZWavePlusInfoCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
        }

        public async Task<ZWavePlusInfoReport> GetAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.ZWavePlusInfo;
            commandClassBytes[1] = (byte)ZWavePlusInfoCommand.Get;

            return await WaitForResponseAsync(destinationNodeId, commandClassBytes, _reportCallbackSources, cancellationToken);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (ZWavePlusInfoCommand)commandClassBytes[1];
            if (command != ZWavePlusInfoCommand.Report)
            {
                _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
            }

            if (!_reportCallbackSources.TryRemove(sourceNodeId, out var callbackSource))
            {
                return;
            }

            var zwavePlusVersion = commandClassBytes[2];
            var roleType = (SlaveRoleType)commandClassBytes[3];
            var nodeType = (NodeType)commandClassBytes[4];
            var installerIcon = commandClassBytes[5..7];
            var userIcon = commandClassBytes[7..9];

            var report = new ZWavePlusInfoReport(zwavePlusVersion, roleType, nodeType, installerIcon, userIcon);
            callbackSource.TrySetResult(report);
        }
    }
}