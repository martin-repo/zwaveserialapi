// -------------------------------------------------------------------------------------------------
// <copyright file="SensorValue.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

    public class SensorValue
    {
        public SensorValue(
            byte sourceNodeId,
            MultilevelSensorType sensorType,
            double value,
            string unit,
            string label,
            Enum scale)
        {
            SourceNodeId = sourceNodeId;
            SensorType = sensorType;
            Value = value;
            Unit = unit;
            Label = label;
            Scale = scale;
        }

        public string Label { get; }

        public Enum Scale { get; }

        public MultilevelSensorType SensorType { get; }

        public byte SourceNodeId { get; }

        public string Unit { get; }

        public double Value { get; }
    }
}