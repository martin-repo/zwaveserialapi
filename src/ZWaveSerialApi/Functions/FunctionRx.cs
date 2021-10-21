// -------------------------------------------------------------------------------------------------
// <copyright file="FunctionRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    using System;

    public abstract class FunctionRx : IFunctionRx
    {
        protected FunctionRx(FunctionType functionType, byte[] returnValueBytes)
        {
            var serialCommandType = (FunctionType)returnValueBytes[0];
            if (serialCommandType != functionType)
            {
                throw new ArgumentException($"Expected {nameof(FunctionType)} {functionType}.");
            }
        }
    }
}