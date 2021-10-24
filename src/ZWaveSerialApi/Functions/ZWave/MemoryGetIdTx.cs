// -------------------------------------------------------------------------------------------------
// <copyright file="GetSucNodeIdTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave
{
    internal class MemoryGetIdTx : FunctionTx
    {
        private MemoryGetIdTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static MemoryGetIdTx Create()
        {
            var functionArgsBytes = new byte[1];

            functionArgsBytes[0] = (byte)FunctionType.MemoryGetId;

            return new MemoryGetIdTx(functionArgsBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new MemoryGetIdRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.MemoryGetId;
        }
    }
}