﻿// -------------------------------------------------------------------------------------------------
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
    using ZWaveSerialApi.Devices.Settings;

    public class AeotecAerqSensor : Device
    {
        private readonly MultilevelSensorCommandClass _multilevelSensor;

        internal AeotecAerqSensor(ZWaveSerialClient client, byte nodeId, NetworkDevice networkDevice)
            : base(client, nodeId, networkDevice)
        {
            _multilevelSensor = Client.GetCommandClass<MultilevelSensorCommandClass>();

            _multilevelSensor.Report += OnMultiLevelSensorReport;
        }

        public event EventHandler<MultilevelSensorReportEventArgs>? DewPointReport;

        public event EventHandler<MultilevelSensorReportEventArgs>? HumidityReport;

        public event EventHandler<MultilevelSensorReportEventArgs>? TemperatureReport;

        public async Task<MultilevelSensorReport> GetDewPointAsync(TemperatureScale scale, CancellationToken cancellationToken = default)
        {
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.DewPoint, scale, cancellationToken);
        }

        public async Task<MultilevelSensorReport> GetHumidityAsync(HumidityScale scale, CancellationToken cancellationToken = default)
        {
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.Humidity, scale, cancellationToken);
        }

        public async Task<MultilevelSensorReport> GetTemperatureAsync(TemperatureScale scale, CancellationToken cancellationToken = default)
        {
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.AirTemperature, scale, cancellationToken);
        }

        private void OnMultiLevelSensorReport(object? sender, MultilevelSensorEventArgs eventArgs)
        {
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