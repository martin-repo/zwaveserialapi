// -------------------------------------------------------------------------------------------------
// <copyright file="PassiveInfraredTimeout.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Configuration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.Configuration;
    using ZWaveSerialApi.Utilities;

    public class PassiveInfraredTimeout
    {
        private readonly ConfigurationCommandClass _configuration;
        private readonly short _maxValue;
        private readonly short _minValue;
        private readonly byte _nodeId;
        private readonly byte _parameterNumber;

        internal PassiveInfraredTimeout(
            byte nodeId,
            byte parameterNumber,
            short minValue,
            short maxValue,
            ConfigurationCommandClass configuration)
        {
            _nodeId = nodeId;
            _parameterNumber = parameterNumber;
            _minValue = minValue;
            _maxValue = maxValue;
            _configuration = configuration;
        }

        public async Task<TimeSpan> GetAsync(CancellationToken cancellationToken = default)
        {
            var report = await _configuration.GetAsync(_nodeId, _parameterNumber, cancellationToken).ConfigureAwait(false);
            var intervalSeconds = EndianHelper.ToUInt16(report.Value);
            return TimeSpan.FromSeconds(intervalSeconds);
        }

        public async Task SetAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var intervalSeconds = (ushort)timeout.TotalSeconds;
            if (intervalSeconds < _minValue || intervalSeconds > _maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), $"Timeout must in the range {_minValue}-{_maxValue} total seconds.");
            }

            var intervalBytes = EndianHelper.GetBytes(intervalSeconds);
            await _configuration.SetAsync(_nodeId, _parameterNumber, false, intervalBytes, cancellationToken).ConfigureAwait(false);
        }
    }
}