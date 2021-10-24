// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSensorReport.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSensor
{
    using System;

    public class MultilevelSensorReport
    {
        public MultilevelSensorReport(
            MultilevelSensorType sensorType,
            double value,
            string unit,
            string label,
            Enum scale)
        {
            SensorType = sensorType;
            Value = value;
            Unit = unit;
            Label = label;
            Scale = scale;
        }

        public string Label { get; }

        public Enum Scale { get; }

        public MultilevelSensorType SensorType { get; }

        public string Unit { get; }

        public double Value { get; }
    }
}