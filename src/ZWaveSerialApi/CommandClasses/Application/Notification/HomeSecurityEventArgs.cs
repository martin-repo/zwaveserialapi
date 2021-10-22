// -------------------------------------------------------------------------------------------------
// <copyright file="HomeSecurityEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Notification
{
    using System;

    public class HomeSecurityEventArgs : EventArgs
    {
        public HomeSecurityEventArgs(int sourceNodeId, HomeSecurityState state, byte[] parameters)
        {
            SourceNodeId = sourceNodeId;
            State = state;
            Parameters = parameters;
        }

        public byte[] Parameters { get; }

        public int SourceNodeId { get; }

        public HomeSecurityState State { get; }
    }
}