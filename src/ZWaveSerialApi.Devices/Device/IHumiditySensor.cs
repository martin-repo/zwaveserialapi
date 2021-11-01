// -------------------------------------------------------------------------------------------------
// <copyright file="IHumiditySensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

    public interface IHumiditySensor
    {
        event EventHandler<MultilevelSensorReportEventArgs>? HumidityReport;

        Task<MultilevelSensorReport> GetHumidityAsync(HumidityScale scale, CancellationToken cancellationToken = default);
    }
}