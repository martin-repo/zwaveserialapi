// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSwitchCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    public class MultilevelSwitchCommandClass : CommandClass
    {
        private readonly IZWaveSerialClient _client;
        private readonly ILogger _logger;

        public MultilevelSwitchCommandClass(ILogger logger, IZWaveSerialClient client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
            _client = client;
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            _logger.Error("Unsupported multilevel switch command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
        }

        public async Task<bool> SetAsync(
            byte destinationNodeId,
            byte level,
            DurationType duration,
            CancellationToken cancellationToken)
        {
            if (level > 0x63 && level < 0xFF)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be 0 to 99 or 255.");
            }

            var commandClassBytes = new byte[4];
            commandClassBytes[0] = (byte)CommandClassType.MultilevelSwitch;
            commandClassBytes[1] = (byte)MultilevelSwitchCommand.Set;
            commandClassBytes[2] = level;
            commandClassBytes[3] = (byte)duration;

            return await _client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken);
        }
    }
}