// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpNotificationEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.WakeUp
{
    using System;

    public class WakeUpNotificationEventArgs : EventArgs
    {
        public WakeUpNotificationEventArgs(byte sourceNodeId)
        {
            SourceNodeId = sourceNodeId;
        }

        public byte SourceNodeId { get; }
    }
}