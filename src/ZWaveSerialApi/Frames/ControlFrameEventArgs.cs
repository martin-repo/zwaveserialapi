// -------------------------------------------------------------------------------------------------
// <copyright file="ControlFrameEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    using System;

    internal class ControlFrameEventArgs : EventArgs
    {
        public ControlFrameEventArgs(FramePreamble framePreamble)
        {
            FramePreamble = framePreamble;
        }

        public FramePreamble FramePreamble { get; }
    }
}