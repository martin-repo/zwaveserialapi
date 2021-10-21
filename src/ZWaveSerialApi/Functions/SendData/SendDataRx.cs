// -------------------------------------------------------------------------------------------------
// <copyright file="SendDataRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    public class SendDataRx : FunctionRx
    {
        public SendDataRx(byte[] returnValueBytes)
            : base(FunctionType.SendData, returnValueBytes)
        {
            Success = returnValueBytes[1] != 0;
        }

        public bool Success { get; }
    }
}