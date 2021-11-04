// -------------------------------------------------------------------------------------------------
// <copyright file="FibaroMotionSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Fibaro
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.Configuration;
    using ZWaveSerialApi.CommandClasses.Application.Notification;
    using ZWaveSerialApi.Devices.Brands.Fibaro.Configuration;
    using ZWaveSerialApi.Devices.Device;
    using ZWaveSerialApi.Devices.Utilities;

    [DeviceName("Fibaro Motion Sensor")]
    [DeviceType(0x010F, 0x0801, 0x1002)]
    public class FibaroMotionSensor : WakeUpDevice, IMotionSensor
    {
        internal FibaroMotionSensor(IZWaveSerialClient client, DeviceState deviceState)
            : base(client, deviceState)
        {
            Parameters = new FibaroMotionSensorParameters(this, Client.GetCommandClass<ConfigurationCommandClass>());

            var notification = Client.GetCommandClass<NotificationCommandClass>();
            notification.HomeSecurityStateChanged += OnNotificationHomeSecurityStateChanged;
        }

        public event EventHandler? MotionDetected;

        public event EventHandler? MotionIdle;

        public FibaroMotionSensorParameters Parameters { get; }

        public async Task<TimeSpan> GetMotionTimeoutAsync(CancellationToken cancellationToken = default)
        {
            AssertAwake();
            return await Parameters.MotionTimeout.GetAsync(cancellationToken);
        }

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
                            MotionIdle?.Invoke(sender, EventArgs.Empty);
                            break;
                    }

                    break;
                case HomeSecurityState.MotionDetection:
                    MotionDetected?.Invoke(sender, EventArgs.Empty);
                    break;
            }
        }
    }
}