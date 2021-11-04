// -------------------------------------------------------------------------------------------------
// <copyright file="MotionTimeout.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Configuration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.Configuration;
    using ZWaveSerialApi.Devices.Device;
    using ZWaveSerialApi.Utilities;

    /// <summary>
    /// How long time, in seconds, a sensor should wait after last motion until sending an idle message.
    /// </summary>
    public class MotionTimeout
    {
        private readonly ConfigurationCommandClass _configuration;
        private readonly Device _device;
        private readonly short _maxValue;
        private readonly short _minValue;
        private readonly byte _parameterNumber;

        internal MotionTimeout(
            Device device,
            byte parameterNumber,
            short minValue,
            short maxValue,
            ConfigurationCommandClass configuration)
        {
            _device = device;
            _parameterNumber = parameterNumber;
            _minValue = minValue;
            _maxValue = maxValue;
            _configuration = configuration;
        }

        public async Task<TimeSpan> GetAsync(CancellationToken cancellationToken = default)
        {
            (_device as WakeUpDevice)?.AssertAwake();

            var report = await _configuration.GetAsync(_device.NodeId, _parameterNumber, cancellationToken).ConfigureAwait(false);
            var intervalSeconds = EndianHelper.ToUInt16(report.Value);
            return TimeSpan.FromSeconds(intervalSeconds);
        }

        public async Task SetAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            (_device as WakeUpDevice)?.AssertAwake();

            var intervalSeconds = (ushort)timeout.TotalSeconds;
            if (intervalSeconds < _minValue || intervalSeconds > _maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), $"Timeout must in the range {_minValue}-{_maxValue} total seconds.");
            }

            var intervalBytes = EndianHelper.GetBytes(intervalSeconds);
            await _configuration.SetAsync(_device.NodeId, _parameterNumber, false, intervalBytes, cancellationToken).ConfigureAwait(false);
        }
    }
}