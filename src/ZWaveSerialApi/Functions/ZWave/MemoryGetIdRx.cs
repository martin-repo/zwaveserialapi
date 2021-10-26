// -------------------------------------------------------------------------------------------------
// <copyright file="MemoryGetIdRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave
{
    internal class MemoryGetIdRx : FunctionRx
    {
        public MemoryGetIdRx(byte[] returnValueBytes)
            : base(FunctionType.MemoryGetId, returnValueBytes)
        {
            HomeId = returnValueBytes[1..5];
            NodeId = returnValueBytes[5];
        }

        public byte[] HomeId { get; }

        public byte NodeId { get; }
    }
}