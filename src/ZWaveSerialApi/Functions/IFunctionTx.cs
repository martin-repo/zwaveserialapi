// -------------------------------------------------------------------------------------------------
// <copyright file="IFunctionTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    internal interface IFunctionTx
    {
        byte[] FunctionArgsBytes { get; }

        bool HasReturnValue { get; }

        IFunctionRx CreateReturnValue(byte[] returnValueBytes);

        bool IsValidReturnValue(byte[] returnValueBytes);
    }
}