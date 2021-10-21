// -------------------------------------------------------------------------------------------------
// <copyright file="FunctionCallResult.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    public record FunctionCallResult(bool TransmitSuccess, IFunctionRx? ReturnValue);
}