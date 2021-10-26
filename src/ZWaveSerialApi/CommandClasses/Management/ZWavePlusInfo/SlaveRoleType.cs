// -------------------------------------------------------------------------------------------------
// <copyright file="SlaveRoleType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.ZWavePlusInfo
{
    public enum SlaveRoleType
    {
        Portable = 0x04,
        AlwaysOn = 0x05,
        SleepingReporting = 0x06,
        SleepingListening = 0x07
    }
}