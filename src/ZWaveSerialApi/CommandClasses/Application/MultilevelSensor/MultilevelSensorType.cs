// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSensorType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSensor
{
    using ZWaveSerialApi.Utilities;

    public enum MultilevelSensorType
    {
        [Scale(typeof(TemperatureScale))]
        AirTemperature = 0x01,

        [Scale(typeof(TemperatureScale))]
        DewPoint = 0x0B,

        [Scale(typeof(HumidityScale))]
        Humidity = 0x05,

        [Scale(typeof(IlluminanceScale))]
        Illuminance = 0x03,

        [Scale(typeof(UltravioletScale))]
        Ultraviolet = 0x1B
    }
}