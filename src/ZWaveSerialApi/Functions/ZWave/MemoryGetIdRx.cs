// -------------------------------------------------------------------------------------------------
// <copyright file="MemoryGetIdRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave
{
    using ZWaveSerialApi.Utilities;

    internal class MemoryGetIdRx : FunctionRx
    {
        public MemoryGetIdRx(byte[] returnValueBytes)
            : base(FunctionType.MemoryGetId, returnValueBytes)
        {
            HomeId = (uint)EndianHelper.ToInt32(returnValueBytes[1..5]);
            NodeId = returnValueBytes[5];
        }

        public uint HomeId { get; }

        public byte NodeId { get; }
    }
}