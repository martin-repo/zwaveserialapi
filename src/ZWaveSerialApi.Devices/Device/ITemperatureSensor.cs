// -------------------------------------------------------------------------------------------------
// <copyright file="ITemperatureSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

    public interface ITemperatureSensor : IDevice
    {
        event EventHandler<MultilevelSensorReportEventArgs>? TemperatureReport;

        Task<MultilevelSensorReport> GetTemperatureAsync(TemperatureScale scale, CancellationToken cancellationToken = default);
    }
}