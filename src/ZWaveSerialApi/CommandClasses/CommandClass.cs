// -------------------------------------------------------------------------------------------------
// <copyright file="CommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses
{
    public abstract class CommandClass
    {
        public abstract void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes);
    }
}