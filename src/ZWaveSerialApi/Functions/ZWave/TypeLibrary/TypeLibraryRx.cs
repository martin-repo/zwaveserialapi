// -------------------------------------------------------------------------------------------------
// <copyright file="TypeLibraryRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.TypeLibrary
{
    internal class TypeLibraryRx : FunctionRx
    {
        public TypeLibraryRx(byte[] returnValueBytes)
            : base(FunctionType.TypeLibrary, returnValueBytes)
        {
            LibraryType = returnValueBytes[1];
        }

        public byte LibraryType { get; }
    }
}