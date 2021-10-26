// -------------------------------------------------------------------------------------------------
// <copyright file="SendDataAbortTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.SendData
{
    internal class SendDataAbortTx : FunctionTx
    {
        private SendDataAbortTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => false;

        public static SendDataAbortTx Create()
        {
            var functionArgsBytes = new byte[1];

            functionArgsBytes[0] = (byte)FunctionType.SendDataAbort;

            return new SendDataAbortTx(functionArgsBytes);
        }
    }
}