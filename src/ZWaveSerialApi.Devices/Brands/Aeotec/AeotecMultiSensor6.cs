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

    using ZWaveSerialApi.CommandClasses.Application.Configuration;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.CommandClasses.Application.Notification;
    using ZWaveSerialApi.Devices.Brands.Aeotec.Configuration;
    using ZWaveSerialApi.Devices.Device;
    using ZWaveSerialApi.Devices.Utilities;

    [DeviceName("Aeotec MultiSensor 6")]
    [DeviceType(0x0086, 0x0002, 0x0064)]
    public class AeotecMultiSensor6 : WakeUpDevice, IMotionSensor, ITemperatureSensor, IHumiditySensor
    {
        private readonly MultilevelSensorCommandClass _multilevelSensor;

        internal AeotecMultiSensor6(IZWaveSerialClient client, DeviceState deviceState)
            : base(client, deviceState)
        {
            Parameters = new AeotecMultiSensor6Parameters(this, Client.GetCommandClass<ConfigurationCommandClass>());

            _multilevelSensor = Client.GetCommandClass<MultilevelSensorCommandClass>();
            _multilevelSensor.Report += OnMultiLevelSensorReport;

            var notification = Client.GetCommandClass<NotificationCommandClass>();
            notification.HomeSecurityStateChanged += OnNotificationHomeSecurityStateChanged;
        }

        public event EventHandler<MultilevelSensorReportEventArgs>? HumidityReport;

        public event EventHandler? MotionDetected;

        public event EventHandler? MotionIdle;

        public event EventHandler<MultilevelSensorReportEventArgs>? TemperatureReport;

        public AeotecMultiSensor6Parameters Parameters { get; }

        public async Task<MultilevelSensorReport> GetHumidityAsync(HumidityScale scale, CancellationToken cancellationToken = default)
        {
            AssertAwake();
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.Humidity, scale, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MultilevelSensorReport> GetTemperatureAsync(TemperatureScale scale, CancellationToken cancellationToken = default)
        {
            AssertAwake();
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.AirTemperature, scale, cancellationToken).ConfigureAwait(false);
        }

        private void OnMultiLevelSensorReport(object? sender, MultilevelSensorEventArgs eventArgs)
        {
            if (eventArgs.SourceNodeId != NodeId)
            {
                return;
            }

            var report = new MultilevelSensorReport(eventArgs.SensorType, eventArgs.Value, eventArgs.Unit, eventArgs.Label, eventArgs.Scale);

            switch (eventArgs.SensorType)
            {
                case MultilevelSensorType.AirTemperature:
                    TemperatureReport?.Invoke(this, new MultilevelSensorReportEventArgs(report));
                    break;
                case MultilevelSensorType.Humidity:
                    HumidityReport?.Invoke(this, new MultilevelSensorReportEventArgs(report));
                    break;
            }
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
                    MotionIdle?.Invoke(sender, EventArgs.Empty);
                    break;
                case HomeSecurityState.MotionDetection:
                    MotionDetected?.Invoke(sender, EventArgs.Empty);
                    break;
            }
        }
    }
}