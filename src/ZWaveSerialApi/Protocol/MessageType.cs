// -------------------------------------------------------------------------------------------------
// <copyright file="MessageType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Protocol
{
    internal enum MessageType
    {
        StartOfFrame = 0x01,
        Ack = 0x06,
        Nack = 0x15,
        Cancel = 0x18
    }
}