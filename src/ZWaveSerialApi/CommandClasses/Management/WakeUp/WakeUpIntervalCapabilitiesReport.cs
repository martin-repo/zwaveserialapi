// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpIntervalCapabilitiesReport.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.WakeUp
{
    using System;

    public class WakeUpIntervalCapabilitiesReport
    {
        public WakeUpIntervalCapabilitiesReport(
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