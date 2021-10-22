// -------------------------------------------------------------------------------------------------
// <copyright file="GetSucNodeIdRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    internal class GetSucNodeIdRx : FunctionRx
    {
        public GetSucNodeIdRx(byte[] returnValueBytes)
            : base(FunctionType.SendData, returnValueBytes)
        {
            NodeId = returnValueBytes[1];
        }

        public byte NodeId { get; }
    }
}