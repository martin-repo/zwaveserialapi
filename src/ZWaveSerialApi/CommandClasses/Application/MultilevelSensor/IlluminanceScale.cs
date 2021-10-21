// -------------------------------------------------------------------------------------------------
// <copyright file="IlluminanceScale.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSensor
{
    using ZWaveSerialApi.Utilities;

    public enum IlluminanceScale
    {
        [Unit("%")]
        Percentage = 0x00,

        [Unit("Lux")]
        Lux = 0x01
    }
}