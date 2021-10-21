// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpCommand.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.WakeUp
{
    public enum WakeUpCommand
    {
        IntervalSet = 0x04,
        IntervalGet = 0x05,
        IntervalReport = 0x06,
        Notification = 0x07,
        NoMoreInformation = 0x08,
        IntervalCapabilitiesGet = 0x09,
        IntervalCapabilitiesReport = 0x0A
    }
}