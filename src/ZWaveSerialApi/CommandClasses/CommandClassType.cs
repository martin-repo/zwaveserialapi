// -------------------------------------------------------------------------------------------------
// <copyright file="CommandClassType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses
{
    // NOTE! When adding an entry to this enum, be sure to also update ZWaveSerialApi.ZWaveSerialClient.CreateCommandClasses()
    public enum CommandClassType
    {
        // Application
        Basic = 0x20,
        ColorSwitch = 0x33,
        MultilevelSensor = 0x31,
        MultilevelSwitch = 0x26,
        Notification = 0x71,

        // Management
        Battery = 0x80,
        ManufacturerSpecific = 0x72,
        WakeUp = 0x84,
        ZWavePlusInfo = 0x5E,

        // Transport encapsulation
        Crc16Encap = 0x56
    }
}