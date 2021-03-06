// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiSetupTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.SerialApi
{
    internal class SerialApiSetupTx : FunctionTx
    {
        private SerialApiSetupTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static SerialApiSetupTx Create(bool enableStatusReport)
        {
            var serialCommandBytes = new byte[3];

            serialCommandBytes[0] = (byte)FunctionType.SerialApiSetup;
            serialCommandBytes[1] = (byte)SerialApiSetupCommand.StatusReport;
            serialCommandBytes[2] = (byte)(enableStatusReport ? 1 : 0);

            return new SerialApiSetupTx(serialCommandBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new SerialApiSetupRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.SerialApiSetup;
        }
    }
}