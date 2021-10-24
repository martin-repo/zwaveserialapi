// -------------------------------------------------------------------------------------------------
// <copyright file="TypeLibraryTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.TypeLibrary
{
    internal class TypeLibraryTx : FunctionTx
    {
        private TypeLibraryTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static TypeLibraryTx Create()
        {
            var functionArgsBytes = new byte[1];

            functionArgsBytes[0] = (byte)FunctionType.TypeLibrary;

            return new TypeLibraryTx(functionArgsBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new TypeLibraryRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.TypeLibrary;
        }
    }
}