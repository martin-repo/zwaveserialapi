// -------------------------------------------------------------------------------------------------
// <copyright file="BasicCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Basic
{
    using System;

    using Serilog;

    public class BasicCommandClass : CommandClass
    {
        private readonly ILogger _logger;

        public BasicCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
        }

        public event EventHandler<BasicEventArgs>? Report;

        public event EventHandler<BasicEventArgs>? Set;

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (BasicCommand)commandClassBytes[1];
            switch (command)
            {
                case BasicCommand.Set:
                    Set?.Invoke(this, new BasicEventArgs(sourceNodeId, commandClassBytes[2]));
                    break;
                case BasicCommand.Report:
                    Report?.Invoke(this, new BasicEventArgs(sourceNodeId, commandClassBytes[2]));
                    break;
                default:
                    _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                    break;
            }
        }
    }
}