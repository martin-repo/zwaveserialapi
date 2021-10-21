// -------------------------------------------------------------------------------------------------
// <copyright file="HomeSecurityType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Notification
{
    public enum HomeSecurityState
    {
        Idle = 0x00,
        CoverTampering = 0x03,
        MotionDetection = 0x08
    }
}