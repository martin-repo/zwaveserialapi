// -------------------------------------------------------------------------------------------------
// <copyright file="TransmitOption.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    using System;

    [Flags]
    public enum TransmitOption
    {
        Ack = 0x01,
        LowPower = 0x02,
        AutoRoute = 0x04,
        NoRoute = 0x10,
        Explore = 0x20
    }
}