// -------------------------------------------------------------------------------------------------
// <copyright file="RequestNodeInfoRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.RequestNodeInfo
{
    internal class RequestNodeInfoRx : FunctionRx
    {
        public RequestNodeInfoRx(byte[] returnValueBytes)
            : base(FunctionType.RequestNodeInfo, returnValueBytes)
        {
            Success = returnValueBytes[1] != 0;
        }

        public bool Success { get; }
    }
}