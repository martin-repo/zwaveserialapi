// -------------------------------------------------------------------------------------------------
// <copyright file="Sensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;

    using ZWaveSerialApi.CommandClasses.Management.WakeUp;

    public abstract class Sensor : Device
    {
        protected Sensor(ZWaveSerialClient client, byte nodeId)
            : base(client, nodeId)
        {
            var wakeUp = Client.GetCommandClass<WakeUpCommandClass>();
            wakeUp.IntervalReport += (sender, eventArgs) =>
            {
                if (eventArgs.SourceNodeId == NodeId)
                {
                    IntervalReport?.Invoke(sender, eventArgs);
                }
            };
            wakeUp.Notification += (sender, eventArgs) =>
            {
                if (eventArgs.SourceNodeId == NodeId)
                {
                    WakeUpNotification?.Invoke(sender, eventArgs);
                }
            };
        }

        public event EventHandler<WakeUpIntervalEventArgs>? IntervalReport;
        public event EventHandler<WakeUpNotificationEventArgs>? WakeUpNotification;


    }
}