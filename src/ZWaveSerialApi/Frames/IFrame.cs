// -------------------------------------------------------------------------------------------------
// <copyright file="IFrame.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    public interface IFrame
    {
        byte[] Bytes { get; }

        FramePreamble Preamble { get; }
    }
}