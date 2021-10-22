// -------------------------------------------------------------------------------------------------
// <copyright file="HumidityScale.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSensor
{
    using ZWaveSerialApi.Utilities;

    public enum HumidityScale
    {
        [Unit("%", "Relative humidity")]
        Percentage = 0x00
    }
}