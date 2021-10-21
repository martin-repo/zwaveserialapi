// -------------------------------------------------------------------------------------------------
// <copyright file="TemperatureScale.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSensor
{
    using ZWaveSerialApi.Utilities;

    public enum TemperatureScale
    {
        [Unit("C", "Celcius")]
        Celcius = 0x00,

        [Unit("F", "Fahrenheit")]
        Fahrenheit = 0x01
    }
}