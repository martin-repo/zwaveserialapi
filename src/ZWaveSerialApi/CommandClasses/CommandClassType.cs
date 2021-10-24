// -------------------------------------------------------------------------------------------------
// <copyright file="CommandClassType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses
{
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

        // Transport encapsulation
        Crc16Encap = 0x56
    }
}