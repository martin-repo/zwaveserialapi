// -------------------------------------------------------------------------------------------------
// <copyright file="FunctionTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    using System;

    internal abstract class FunctionTx : IFunctionTx
    {
        protected FunctionTx(byte[] functionArgsBytes)
        {
            FunctionArgsBytes = functionArgsBytes;
        }

        public byte[] FunctionArgsBytes { get; }

        public abstract bool HasReturnValue { get; }

        public virtual IFunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            throw new InvalidOperationException();
        }

        public virtual bool IsValidReturnValue(byte[] returnValueBytes)
        {
            return false;
        }
    }
}