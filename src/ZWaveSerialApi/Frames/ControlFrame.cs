// -------------------------------------------------------------------------------------------------
// <copyright file="ControlFrame.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    public class ControlFrame : Frame
    {
        public ControlFrame(byte frameTypeByte)
            : base(new[] { frameTypeByte })
        {
        }
    }
}