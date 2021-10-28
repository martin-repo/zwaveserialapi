// -------------------------------------------------------------------------------------------------
// <copyright file="TransmitOption.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.SendData
{
    using System;

    /// <remarks>
    /// ZW_transport_api.h
    /// </remarks>
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