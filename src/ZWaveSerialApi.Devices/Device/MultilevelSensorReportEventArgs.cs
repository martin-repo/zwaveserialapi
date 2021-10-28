// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSensorReportEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

    public class MultilevelSensorReportEventArgs : EventArgs
    {
        public MultilevelSensorReportEventArgs(MultilevelSensorReport report)
        {
            Report = report;
        }

        public MultilevelSensorReport Report { get; }
    }
}