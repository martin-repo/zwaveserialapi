// -------------------------------------------------------------------------------------------------
// <copyright file="BatteryEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.Battery
{
    using System;

    public class BatteryEventArgs : EventArgs
    {
        public BatteryEventArgs(byte sourceNodeId, bool isLow, byte percentage)
        {
            SourceNodeId = sourceNodeId;
            IsLow = isLow;
            Percentage = percentage;
        }

        public bool IsLow { get; }

        public byte Percentage { get; }

        public byte SourceNodeId { get; }
    }
}