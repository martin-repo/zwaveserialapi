// -------------------------------------------------------------------------------------------------
// <copyright file="IDewPointSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

    public interface IDewPointSensor
    {
        event EventHandler<MultilevelSensorReportEventArgs>? DewPointReport;

        Task<MultilevelSensorReport> GetDewPointAsync(TemperatureScale scale, CancellationToken cancellationToken = default);
    }
}