// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSensorEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSensor
{
    using System;

    public class MultilevelSensorEventArgs : EventArgs
    {
        public MultilevelSensorEventArgs(
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