// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpIntervalCapabilities.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;

    public class WakeUpIntervalCapabilities
    {
        public WakeUpIntervalCapabilities(
            TimeSpan minimumInterval,
            TimeSpan maximumInterval,
            TimeSpan defaultInterval,
            TimeSpan intervalStep)
        {
            MinimumInterval = minimumInterval;
            MaximumInterval = maximumInterval;
            DefaultInterval = defaultInterval;
            IntervalStep = intervalStep;
        }

        public TimeSpan DefaultInterval { get; }

        public TimeSpan IntervalStep { get; }

        public TimeSpan MaximumInterval { get; }

        public TimeSpan MinimumInterval { get; }
    }
}