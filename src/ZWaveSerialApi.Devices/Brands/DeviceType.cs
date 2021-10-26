// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands
{
    using ZWaveSerialApi.Devices.Utilities;

    // NOTE! When adding an entry to this enum, be sure to also update ZWaveSerialApi.Devices.ZWaveDevices.CreateDevice(...)
    public enum DeviceType : ulong
    {
        [ManufacturerName("Aeotec aërQ Temperature & Humidity Sensor")]
        [ManufacturerSpecific(0x0371, 0x0002, 0x0009)]
        AeotecAerqSensor = 0x037100020009,

        [ManufacturerName("Aeotec LED Bulb 6 MultiColor")]
        [ManufacturerSpecific(0x0371, 0x0003, 0x0002)]
        AeotecLedBulb6MultiColor = 0x037100030002,

        [ManufacturerName("Aeotec MultiSensor 6")]
        [ManufacturerSpecific(0x0086, 0x0002, 0x0064)]
        AeotecMultiSensor6 = 0x008600020064,

        [ManufacturerName("Fibaro Motion Sensor")]
        [ManufacturerSpecific(0x010F, 0x0801, 0x1002)]
        FibaroMotionSensor = 0x010F08011002
    }
}