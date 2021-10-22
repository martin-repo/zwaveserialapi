// -------------------------------------------------------------------------------------------------
// <copyright file="GetSucNodeIdTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    /// <summary>
    /// Get static update controller node id
    /// </summary>
    internal class GetSucNodeIdTx : FunctionTx
    {
        private GetSucNodeIdTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static GetSucNodeIdTx Create()
        {
            var functionArgsBytes = new byte[1];

            functionArgsBytes[0] = (byte)FunctionType.GetSucNodeId;

            return new GetSucNodeIdTx(functionArgsBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new GetSucNodeIdRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.GetSucNodeId;
        }
    }
}