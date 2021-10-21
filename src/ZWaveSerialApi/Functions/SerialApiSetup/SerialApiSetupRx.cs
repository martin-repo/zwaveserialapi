// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiSetupRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    public class SerialApiSetupRx : FunctionRx
    {
        public SerialApiSetupRx(byte[] returnValueBytes)
            : base(FunctionType.SerialApiSetup, returnValueBytes)
        {
            Response = returnValueBytes[2..];
        }

        public byte[] Response { get; }
    }
}