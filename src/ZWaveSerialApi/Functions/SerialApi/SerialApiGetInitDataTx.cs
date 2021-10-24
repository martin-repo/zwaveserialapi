// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiGetInitDataTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.SerialApi
{
    internal class SerialApiGetInitDataTx : FunctionTx
    {
        private SerialApiGetInitDataTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static SerialApiGetInitDataTx Create()
        {
            var serialCommandBytes = new byte[1];

            serialCommandBytes[0] = (byte)FunctionType.SerialApiGetInitData;

            return new SerialApiGetInitDataTx(serialCommandBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new SerialApiGetInitDataRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.SerialApiGetInitData;
        }
    }
}