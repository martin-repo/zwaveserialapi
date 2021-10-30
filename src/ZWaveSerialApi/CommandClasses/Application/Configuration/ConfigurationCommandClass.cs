// -------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class ConfigurationCommandClass : CommandClass
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<byte, TaskCompletionSource<ConfigurationReport>> _reportCallbackSources = new();

        public ConfigurationCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(client)
        {
            _logger = logger.ForContext<ConfigurationCommandClass>().ForContext(Constants.ClassName, GetType().Name);
        }

        public async Task<ConfigurationReport> GetAsync(byte destinationNodeId, byte parameterNumber, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[3];
            commandClassBytes[0] = (byte)CommandClassType.Configuration;
            commandClassBytes[1] = (byte)ConfigurationCommand.Get;
            commandClassBytes[2] = parameterNumber;

            return await WaitForResponseAsync(destinationNodeId, commandClassBytes, _reportCallbackSources, cancellationToken);
        }

        public async Task SetAsync(
            byte destinationNodeId,
            byte parameterNumber,
            bool @default,
            byte[] value,
            CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[4 + value.Length];
            commandClassBytes[0] = (byte)CommandClassType.Configuration;
            commandClassBytes[1] = (byte)ConfigurationCommand.Set;
            commandClassBytes[2] = parameterNumber;
            commandClassBytes[3] = ConstructMetadataByte(@default, (byte)value.Length);

            value.CopyTo(commandClassBytes, 4);

            await Client.SendDataAsync(destinationNodeId, commandClassBytes.ToArray(), cancellationToken);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (ConfigurationCommand)commandClassBytes[1];
            if (command != ConfigurationCommand.Report)
            {
                _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                return;
            }

            var parameterNumber = commandClassBytes[2];

            var size = DeconstructMetadataByte(commandClassBytes[3]);

            var value = commandClassBytes[4..(4 + size)];

            if (_reportCallbackSources.TryRemove(sourceNodeId, out var callbackSource))
            {
                var report = new ConfigurationReport(parameterNumber, value);
                callbackSource.TrySetResult(report);
            }
        }

        private byte ConstructMetadataByte(bool @default, byte size)
        {
            //  7 6 5 4 3 2 1 0
            // |-| default
            //   |-------| reserved
            //           |-----| size
            var metadataByte = (byte)0;
            if (@default)
            {
                metadataByte |= 1 << 7;
            }

            metadataByte |= (byte)(size & BitHelper.Bit2Mask);

            return metadataByte;
        }

        private byte DeconstructMetadataByte(byte metadataByte)
        {
            //  7 6 5 4 3 2 1 0
            // |---------| reserved
            //           |-----| size
            var size = (byte)(metadataByte & BitHelper.Bit2Mask);

            return size;
        }
    }
}