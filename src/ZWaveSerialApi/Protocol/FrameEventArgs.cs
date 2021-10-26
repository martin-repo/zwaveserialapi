// -------------------------------------------------------------------------------------------------
// <copyright file="FrameEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Protocol
{
    using System;

    internal class FrameEventArgs : EventArgs
    {
        public FrameEventArgs(Frame frame)
        {
            Frame = frame;
        }

        public Frame Frame { get; }
    }
}