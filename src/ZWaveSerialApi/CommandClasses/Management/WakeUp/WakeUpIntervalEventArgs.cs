// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpIntervalEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.WakeUp
{
    using System;

    public class WakeUpIntervalEventArgs : EventArgs
    {
        public WakeUpIntervalEventArgs(byte sourceNodeId, TimeSpan interval)
        {
            SourceNodeId = sourceNodeId;
            Interval = interval;
        }

        public TimeSpan Interval { get; }

        public byte SourceNodeId { get; }
    }
}