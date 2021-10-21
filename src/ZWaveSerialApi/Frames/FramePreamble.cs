// -------------------------------------------------------------------------------------------------
// <copyright file="FramePreamble.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    public enum FramePreamble
    {
        StartOfFrame = 0x01,
        Ack = 0x06,
        Nack = 0x15,
        Cancel = 0x18
    }
}