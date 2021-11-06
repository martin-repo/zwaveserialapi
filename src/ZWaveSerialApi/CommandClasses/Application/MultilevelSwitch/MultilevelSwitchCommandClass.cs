// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSwitchCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    public class MultilevelSwitchCommandClass : CommandClass
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<byte, TaskCompletionSource<MultilevelSwitchReport>> _reportCallbackSources = new();

        public MultilevelSwitchCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(CommandClassType.MultilevelSwitch, client)
        {
            _logger = logger.ForContext<MultilevelSwitchCommandClass>().ForContext(Constants.ClassName, GetType().Name);
        }

        public async Task<MultilevelSwitchReport> GetAsync(byte destinationNodeId, CancellationToken cancellationToken = default)
        {
            const MultilevelSwitchCommand Command = MultilevelSwitchCommand.Get;

            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)Type;
            commandClassBytes[1] = (byte)Command;

            _logger.OutboundCommand(destinationNodeId, commandClassBytes, Type, Command);
            return await WaitForResponseAsync(destinationNodeId, commandClassBytes, _reportCallbackSources, cancellationToken).ConfigureAwait(false);
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

            const MultilevelSwitchCommand Command = MultilevelSwitchCommand.Set;

            var commandClassBytes = new byte[4];
            commandClassBytes[0] = (byte)Type;
            commandClassBytes[1] = (byte)Command;
            commandClassBytes[2] = level;
            commandClassBytes[3] = (byte)duration;

            _logger.OutboundCommand(destinationNodeId, commandClassBytes, Type, Command);
            await Client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken).ConfigureAwait(false);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (MultilevelSwitchCommand)commandClassBytes[1];
            switch (command)
            {
                case MultilevelSwitchCommand.Report:
                    _logger.InboundCommand(sourceNodeId, commandClassBytes, Type, command);
                    ProcessReport(sourceNodeId, commandClassBytes);
                    break;
                default:
                    _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                    return;
            }
        }

        private void ProcessReport(byte sourceNodeId, byte[] commandClassBytes)
        {
            if (!_reportCallbackSources.TryRemove(sourceNodeId, out var callbackSource))
            {
                return;
            }

            var value = commandClassBytes[2];
            callbackSource.TrySetResult(new MultilevelSwitchReport(value));
        }
    }
}