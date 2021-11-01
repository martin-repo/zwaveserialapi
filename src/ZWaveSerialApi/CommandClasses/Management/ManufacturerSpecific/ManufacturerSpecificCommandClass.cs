﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerSpecificCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class ManufacturerSpecificCommandClass : CommandClass
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<byte, TaskCompletionSource<ManufacturerSpecificReport>> _reportCallbackSources = new();

        public ManufacturerSpecificCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(client)
        {
            _logger = logger.ForContext<ManufacturerSpecificCommandClass>().ForContext(Constants.ClassName, GetType().Name);
        }

        public async Task<ManufacturerSpecificReport> GetAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.ManufacturerSpecific;
            commandClassBytes[1] = (byte)ManufacturerSpecificCommand.Get;

            return await WaitForResponseAsync(destinationNodeId, commandClassBytes, _reportCallbackSources, cancellationToken).ConfigureAwait(false);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (ManufacturerSpecificCommand)commandClassBytes[1];
            if (command != ManufacturerSpecificCommand.Report)
            {
                _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
            }

            if (!_reportCallbackSources.TryRemove(sourceNodeId, out var callbackSource))
            {
                return;
            }

            var manufacturerId = EndianHelper.ToUInt16(commandClassBytes[2..4]);
            var productTypeId = EndianHelper.ToUInt16(commandClassBytes[4..6]);
            var productId = EndianHelper.ToUInt16(commandClassBytes[6..8]);

            var report = new ManufacturerSpecificReport(manufacturerId, productTypeId, productId);
            callbackSource.TrySetResult(report);
        }
    }
}