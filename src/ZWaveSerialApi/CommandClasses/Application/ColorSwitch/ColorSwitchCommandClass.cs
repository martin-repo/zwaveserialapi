﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ColorSwitchCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.ColorSwitch
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class ColorSwitchCommandClass : CommandClass
    {
        private readonly IZWaveSerialClient _client;
        private readonly ILogger _logger;

        public ColorSwitchCommandClass(ILogger logger, IZWaveSerialClient client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
            _client = client;
        }

        public override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            _logger.Error("Unsupported color switch command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
        }

        public async Task<bool> SetAsync(
            byte destinationNodeId,
            ICollection<ColorComponent> colorComponents,
            DurationType duration,
            CancellationToken cancellationToken)
        {
            var commandClassBytes = new List<byte>();
            commandClassBytes.Add((byte)CommandClassType.ColorSwitch);
            commandClassBytes.Add((byte)ColorSwitchCommand.Set);
            commandClassBytes.Add(ConstructMetadataByte(colorComponents.Count));

            foreach (var (type, value) in colorComponents)
            {
                commandClassBytes.Add((byte)type);
                commandClassBytes.Add(value);
            }

            commandClassBytes.Add((byte)duration);

            return await _client.SendDataAsync(destinationNodeId, commandClassBytes.ToArray(), cancellationToken);
        }

        private byte ConstructMetadataByte(int colorComponentCount)
        {
            //  7 6 5 4 3 2 1 0
            // |-----| reserved
            //       |---------| color component count
            var metadataByte = (byte)0;
            metadataByte |= (byte)(colorComponentCount & BitHelper.Bit4Mask);

            return metadataByte;
        }
    }
}