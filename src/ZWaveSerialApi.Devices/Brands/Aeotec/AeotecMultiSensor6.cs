// -------------------------------------------------------------------------------------------------
// <copyright file="AeotecMultiSensor6.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Aeotec
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.CommandClasses.Application.Notification;

    public class AeotecMultiSensor6 : Device
    {
        private readonly MultilevelSensorCommandClass _multilevelSensor;

        public AeotecMultiSensor6(ZWaveSerialClient client, byte nodeId)
            : base(client, nodeId)
        {
            _multilevelSensor = Client.GetCommandClass<MultilevelSensorCommandClass>();

            var notification = Client.GetCommandClass<NotificationCommandClass>();
            notification.HomeSecurityStateChanged += OnNotificationHomeSecurityStateChanged;
        }

        public event EventHandler? HomeSecurityIdle;

        public event EventHandler? HomeSecurityMotionDetected;

        public async Task<MultilevelSensorReport> GetHumidityAsync(HumidityScale scale, CancellationToken cancellationToken)
        {
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.Humidity, scale, cancellationToken);
        }

        public async Task<MultilevelSensorReport> GetTemperatureAsync(TemperatureScale scale, CancellationToken cancellationToken)
        {
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.AirTemperature, scale, cancellationToken);
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
                    HomeSecurityIdle?.Invoke(sender, EventArgs.Empty);
                    break;
                case HomeSecurityState.MotionDetection:
                    HomeSecurityMotionDetected?.Invoke(sender, EventArgs.Empty);
                    break;
            }
        }
    }
}