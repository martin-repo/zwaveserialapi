// -------------------------------------------------------------------------------------------------
// <copyright file="Frame.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    public abstract class Frame
    {
        protected Frame(byte[] frameBytes)
        {
            Bytes = frameBytes;
            Preamble = (FramePreamble)Bytes[0];
        }

        public byte[] Bytes { get; }

        public FramePreamble Preamble { get; }
    }
}