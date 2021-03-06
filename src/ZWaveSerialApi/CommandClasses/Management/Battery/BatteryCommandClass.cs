// -------------------------------------------------------------------------------------------------
// <copyright file="BatteryCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.Battery
{
    using System;

    using Serilog;

    public class BatteryCommandClass : CommandClass
    {
        private readonly ILogger _logger;

        public BatteryCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(CommandClassType.Battery, client)
        {
            _logger = logger.ForContext<BatteryCommandClass>().ForContext(Constants.ClassName, GetType().Name);
        }

        public event EventHandler<BatteryEventArgs>? Report;

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (BatteryCommand)commandClassBytes[1];
            switch (command)
            {
                case BatteryCommand.Report:
                    _logger.InboundCommand(sourceNodeId, commandClassBytes, Type, command);
                    var isLow = commandClassBytes[2] == 0xFF;
                    var value = isLow ? (byte)0 : commandClassBytes[2];
                    Report?.Invoke(this, new BatteryEventArgs(sourceNodeId, isLow, value));
                    break;
                default:
                    _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                    break;
            }
        }
    }
}