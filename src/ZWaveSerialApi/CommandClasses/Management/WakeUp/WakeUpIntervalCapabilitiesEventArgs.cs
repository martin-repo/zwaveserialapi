// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpIntervalCapabilitiesEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.WakeUp
{
    using System;

    public class WakeUpIntervalCapabilitiesEventArgs : EventArgs
    {
        public WakeUpIntervalCapabilitiesEventArgs(
            byte sourceNodeId,
            TimeSpan minimumInterval,
            TimeSpan maximumInterval,
            TimeSpan defaultInterval,
            TimeSpan intervalStep)
        {
            SourceNodeId = sourceNodeId;
            MinimumInterval = minimumInterval;
            MaximumInterval = maximumInterval;
            DefaultInterval = defaultInterval;
            IntervalStep = intervalStep;
        }

        public TimeSpan DefaultInterval { get; }

        public TimeSpan IntervalStep { get; }

        public TimeSpan MaximumInterval { get; }

        public TimeSpan MinimumInterval { get; }

        public byte SourceNodeId { get; }
    }
}