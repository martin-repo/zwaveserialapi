// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands
{
    using ZWaveSerialApi.Devices.Utilities;

    public enum DeviceType
    {
        [ManufacturerName("Aeotec LED Bulb 6 MultiColor")]
        [ManufacturerSpecific(0x0371, 0x0003, 0x0002)]
        AeotecLedBulb6MultiColor,

        [ManufacturerName("Aeotec MultiSensor 6")]
        [ManufacturerSpecific(0x0086, 0x0002, 0x0064)]
        AeotecMultiSensor6
    }
}