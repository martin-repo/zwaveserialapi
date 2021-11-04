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
        private readonly ILogger _logger;

        public MultilevelSwitchCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(CommandClassType.MultilevelSwitch, client)
        {
            _logger = logger.ForContext<MultilevelSwitchCommandClass>().ForContext(Constants.ClassName, GetType().Name);
        }

        public async Task SetAsync(
            byte destinationNodeId,
            byte level,
            DurationType duration,
            CancellationToken cancellationToken)
        {
            if (level is > 0x63 and < 0xFF)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be 0 to 99 or 255.");
            }

            var command = MultilevelSwitchCommand.Set;

            var commandClassBytes = new byte[4];
            commandClassBytes[0] = (byte)Type;
            commandClassBytes[1] = (byte)command;
            commandClassBytes[2] = level;
            commandClassBytes[3] = (byte)duration;

            _logger.OutboundCommand(destinationNodeId, commandClassBytes, Type, command);
            await Client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken).ConfigureAwait(false);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
        }
    }
}