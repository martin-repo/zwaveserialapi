// -------------------------------------------------------------------------------------------------
// <copyright file="TransmitComplete.cs" company="Martin Karlsson">
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
    internal enum TransmitComplete
    {
        Ok = 0x00,
        NoAck = 0x01,
        Fail = 0x02,
        RoutingNotIdle = 0x03,
        NoRoute = 0x04,
        Verified = 0x05
    }
}