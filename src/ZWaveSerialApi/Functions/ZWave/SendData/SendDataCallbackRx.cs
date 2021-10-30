// -------------------------------------------------------------------------------------------------
// <copyright file="SendDataCallbackRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.SendData
{
    internal class SendDataCallbackRx : FunctionRx
    {
        private SendDataCallbackRx(byte[] returnValueBytes)
            : base(FunctionType.SendData, returnValueBytes)
        {
            CallbackFuncId = returnValueBytes[1];
            Status = (TransmitComplete)returnValueBytes[2];
        }

        public byte CallbackFuncId { get; }

        public TransmitComplete Status { get; }

        public static SendDataCallbackRx? TryCreate(byte[] returnValueBytes)
        {
            if (returnValueBytes.Length != 3 || (FunctionType)returnValueBytes[0] != FunctionType.SendData)
            {
                return null;
            }

            return new SendDataCallbackRx(returnValueBytes);
        }
    }
}