// -------------------------------------------------------------------------------------------------
// <copyright file="FrameEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Protocol
{
    using System;

    internal class MessageTypeEventArgs : EventArgs
    {
        public MessageTypeEventArgs(MessageType messageType)
        {
            MessageType = messageType;
        }

        public MessageType MessageType { get; }
    }
}