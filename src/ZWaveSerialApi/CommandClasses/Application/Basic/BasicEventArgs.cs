// -------------------------------------------------------------------------------------------------
// <copyright file="BasicEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Basic
{
    using System;

    public class BasicEventArgs : EventArgs
    {
        public BasicEventArgs(byte sourceNodeId, byte value)
        {
            SourceNodeId = sourceNodeId;
            Value = value;
        }

        public byte SourceNodeId { get; }

        public byte Value { get; }
    }
}