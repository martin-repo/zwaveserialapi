// -------------------------------------------------------------------------------------------------
// <copyright file="AeotecAerqSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Aeotec
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.Devices.Device;
    using ZWaveSerialApi.Devices.Utilities;

    [DeviceName("Aeotec aërQ Temperature & Humidity Sensor")]
    [DeviceType(0x0371, 0x0002, 0x0009)]
    public class AeotecAerqSensor : WakeUpDevice, ITemperatureSensor, IHumiditySensor, IDewPointSensor
    {
        private readonly MultilevelSensorCommandClass _multilevelSensor;

        internal AeotecAerqSensor(IZWaveSerialClient client, DeviceState deviceState)
            : base(client, deviceState)
        {
            _multilevelSensor = Client.GetCommandClass<MultilevelSensorCommandClass>();
            _multilevelSensor.Report += OnMultiLevelSensorReport;
        }

        public event EventHandler<MultilevelSensorReportEventArgs>? DewPointReport;

        public event EventHandler<MultilevelSensorReportEventArgs>? HumidityReport;

        public event EventHandler<MultilevelSensorReportEventArgs>? TemperatureReport;

        public async Task<MultilevelSensorReport> GetDewPointAsync(TemperatureScale scale, CancellationToken cancellationToken = default)
        {
            AssertAwake();
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.DewPoint, scale, cancellationToken).ConfigureAwait(false);
        }

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
                case MultilevelSensorType.DewPoint:
                    DewPointReport?.Invoke(this, new MultilevelSensorReportEventArgs(report));
                    break;
                case MultilevelSensorType.Humidity:
                    HumidityReport?.Invoke(this, new MultilevelSensorReportEventArgs(report));
                    break;
            }
        }
    }
}