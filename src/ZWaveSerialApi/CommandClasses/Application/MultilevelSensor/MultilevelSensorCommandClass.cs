// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSensorCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSensor
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class MultilevelSensorCommandClass : CommandClass
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<byte, TaskCompletionSource<MultilevelSensorReport>> _reportCallbackSources = new();

        public MultilevelSensorCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
        }

        public event EventHandler<MultilevelSensorEventArgs>? Report;

        public async Task<MultilevelSensorReport> GetAsync<T>(
            byte destinationNodeId,
            MultilevelSensorType sensorType,
            T scale,
            CancellationToken cancellationToken)
            where T : Enum
        {
            var expectedScaleEnumType = AttributeHelper.GetScaleEnumType(sensorType);
            if (expectedScaleEnumType != scale.GetType())
            {
                throw new InvalidOperationException($"Attempt to use {sensorType} with scale {scale.GetType().Name}");
            }

            var scaleValue = Convert.ToByte(scale);

            var commandClassBytes = new byte[4];
            commandClassBytes[0] = (byte)CommandClassType.MultilevelSensor;
            commandClassBytes[1] = (byte)MultilevelSensorCommand.Get;
            commandClassBytes[2] = (byte)sensorType;
            commandClassBytes[3] = ConstructMetadataByte(0, scaleValue, 0);

            return await WaitForResponseAsync(destinationNodeId, commandClassBytes, _reportCallbackSources, cancellationToken);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (MultilevelSensorCommand)commandClassBytes[1];
            if (command != MultilevelSensorCommand.Report)
            {
                _logger.Error("Unsupported multilevel sensor command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                return;
            }

            var sensorType = (MultilevelSensorType)commandClassBytes[2];
            if (!Enum.IsDefined(typeof(MultilevelSensorType), sensorType))
            {
                _logger.Error("Unsupported multilevel sensor type {Type}", BitConverter.ToString(commandClassBytes, 2, 1));
                return;
            }

            var scaleEnumType = AttributeHelper.GetScaleEnumType(sensorType);

            var (precision, scaleValue, size) = DeconstructMetadataByte(commandClassBytes[3]);

            var scale = (Enum)Enum.ToObject(scaleEnumType, scaleValue);
            if (!Enum.IsDefined(scaleEnumType, scale))
            {
                _logger.Error("Unsupported multilevel sensor {ScaleName} {ScaleValue}", scaleEnumType.Name, scaleValue);
                return;
            }

            var (unit, label) = AttributeHelper.GetUnit(scale);

            if (!TryGetRawValue(size, commandClassBytes, out var rawValue))
            {
                return;
            }

            var value = rawValue / Math.Pow(10, precision);

            if (_reportCallbackSources.TryRemove(sourceNodeId, out var callbackSource))
            {
                var get = new MultilevelSensorReport(sensorType, value, unit, label, scale);
                callbackSource.TrySetResult(get);
            }

            Report?.Invoke(
                this,
                new MultilevelSensorEventArgs(
                    sourceNodeId,
                    sensorType,
                    value,
                    unit,
                    label,
                    scale));
        }

        private byte ConstructMetadataByte(byte precision, byte scaleValue, byte size)
        {
            //  7 6 5 4 3 2 1 0
            // |-----| precision
            //       |---| scale
            //           |-----| size
            var metadataByte = (byte)0;
            metadataByte |= (byte)(precision << 5);
            metadataByte |= (byte)((scaleValue & BitHelper.Bit1Mask) << 3);
            metadataByte |= (byte)(size & BitHelper.Bit2Mask);

            return metadataByte;
        }

        private (byte Precision, byte ScaleValue, byte Size) DeconstructMetadataByte(byte metadataByte)
        {
            //  7 6 5 4 3 2 1 0
            // |-----| precision
            //       |---| scale
            //           |-----| size
            var precision = (byte)(metadataByte >> 5);
            var scaleValue = (byte)((metadataByte >> 3) & BitHelper.Bit1Mask);
            var size = (byte)(metadataByte & BitHelper.Bit2Mask);

            return (precision, scaleValue, size);
        }

        private bool TryGetRawValue(int size, byte[] commandClassBytes, out double rawValue)
        {
            switch (size)
            {
                case 1:
                    rawValue = (sbyte)commandClassBytes[4];
                    return true;
                case 2:
                    rawValue = EndianHelper.ToInt16(commandClassBytes[4..6]);
                    return true;
                case 4:
                    rawValue = EndianHelper.ToInt32(commandClassBytes[4..8]);
                    return true;
                default:
                    _logger.Error("Unsupported multilevel sensor size {Size}", size);
                    rawValue = 0;
                    return false;
            }
        }
    }
}