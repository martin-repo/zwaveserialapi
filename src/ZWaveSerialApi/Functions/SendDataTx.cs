// -------------------------------------------------------------------------------------------------
// <copyright file="SendDataTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    internal class SendDataTx : FunctionTx
    {
        private static int callbackId;

        private SendDataTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static SendDataTx Create(
            int destinationNodeId,
            byte[] commandClassBytes,
            TransmitOption transmitOptions,
            bool requestCallback)
        {
            var functionArgsBytes = new byte[commandClassBytes.Length + 5];

            functionArgsBytes[0] = (byte)FunctionType.SendData;
            functionArgsBytes[1] = (byte)destinationNodeId;
            functionArgsBytes[2] = (byte)commandClassBytes.Length;
            commandClassBytes.CopyTo(functionArgsBytes, 3);

            var index = 3 + commandClassBytes.Length;
            functionArgsBytes[index++] = (byte)transmitOptions;
            functionArgsBytes[index] = requestCallback ? GetNextCallbackId() : (byte)0;

            return new SendDataTx(functionArgsBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new SendDataRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.SendData;
        }

        private static byte GetNextCallbackId()
        {
            if (++callbackId > 255)
            {
                callbackId = 1;
            }

            return (byte)callbackId;
        }
    }
}