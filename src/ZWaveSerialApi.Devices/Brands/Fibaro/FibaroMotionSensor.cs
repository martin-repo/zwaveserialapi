// -------------------------------------------------------------------------------------------------
// <copyright file="FibaroMotionSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Fibaro
{
    using System;

    using ZWaveSerialApi.CommandClasses.Application.Notification;
    using ZWaveSerialApi.Devices.Device;
    using ZWaveSerialApi.Devices.Utilities;

    [DeviceName("Fibaro Motion Sensor")]
    [DeviceType(0x010F, 0x0801, 0x1002)]
    public class FibaroMotionSensor : Device
    {
        internal FibaroMotionSensor(IZWaveSerialClient client, DeviceState deviceState)
            : base(client, deviceState)
        {
            var notification = Client.GetCommandClass<NotificationCommandClass>();
            notification.HomeSecurityStateChanged += OnNotificationHomeSecurityStateChanged;
        }

        public event EventHandler? HomeSecurityMotionDetected;

        public event EventHandler? HomeSecurityMotionIdle;

        private void OnNotificationHomeSecurityStateChanged(object? sender, HomeSecurityEventArgs eventArgs)
        {
            if (eventArgs.SourceNodeId != NodeId)
            {
                return;
            }

            switch (eventArgs.State)
            {
                case HomeSecurityState.Idle:
                    var eventGoingIdle = (HomeSecurityState)eventArgs.Parameters[0];
                    switch (eventGoingIdle)
                    {
                        case HomeSecurityState.MotionDetection:
                            HomeSecurityMotionIdle?.Invoke(sender, EventArgs.Empty);
                            break;
                    }

                    break;
                case HomeSecurityState.MotionDetection:
                    HomeSecurityMotionDetected?.Invoke(sender, EventArgs.Empty);
                    break;
            }
        }
    }
}