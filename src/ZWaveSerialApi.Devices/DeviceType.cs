// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    public enum DeviceType
    {
        // [ManufacturerSpecific(ManufacturerId, ProductType, ProductId)]
        // [ManufacturerSpecific(0x0371, 0x0003, 0x0002)]
        AeotecLedBulb6MultiColor,
        // [ManufacturerSpecific(0x0086, 0x0002, 0x0064)]
        AeotecMultiSensor6
    }
}