// -------------------------------------------------------------------------------------------------
// <copyright file="SendDataTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.SendData
{
    internal class SendDataTx : FunctionTx
    {
        public const TransmitOption DefaultTransmitOptions = TransmitOption.Ack | TransmitOption.AutoRoute | TransmitOption.Explore;

        private readonly byte _callbackFuncId;

        private SendDataTx(byte[] functionArgsBytes, byte callbackFuncId)
            : base(functionArgsBytes)
        {
            _callbackFuncId = callbackFuncId;
        }

        public override bool HasReturnValue => true;

        public static SendDataTx Create(
            byte destinationNodeId,
            byte[] commandClassBytes,
            TransmitOption transmitOptions,
            byte callbackFuncId)
        {
            var functionArgsBytes = new byte[commandClassBytes.Length + 5];

            functionArgsBytes[0] = (byte)FunctionType.SendData;
            functionArgsBytes[1] = destinationNodeId;
            functionArgsBytes[2] = (byte)commandClassBytes.Length;
            commandClassBytes.CopyTo(functionArgsBytes, 3);

            functionArgsBytes[^2] = (byte)transmitOptions;
            functionArgsBytes[^1] = callbackFuncId;

            return new SendDataTx(functionArgsBytes, callbackFuncId);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new SendDataRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.SendData && returnValueBytes[1] == _callbackFuncId;
        }
    }
}