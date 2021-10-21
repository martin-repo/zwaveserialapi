// -------------------------------------------------------------------------------------------------
// <copyright file="MotionEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Notification
{
    using System;

    public class HomeSecurityEventArgs : EventArgs
    {
        public HomeSecurityEventArgs(int sourceNodeId, HomeSecurityState state, byte[]parameters)
        {
            SourceNodeId = sourceNodeId;
            State = state;
            Parameters = parameters;
        }

        public HomeSecurityState State { get; }
        public byte[] Parameters { get; }

        public int SourceNodeId { get; }
    }
}