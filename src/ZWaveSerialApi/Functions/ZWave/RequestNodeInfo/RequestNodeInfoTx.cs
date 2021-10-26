// -------------------------------------------------------------------------------------------------
// <copyright file="RequestNodeInfoTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.RequestNodeInfo
{
    internal class RequestNodeInfoTx : FunctionTx
    {
        private RequestNodeInfoTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static RequestNodeInfoTx Create(byte destinationNodeId)
        {
            var functionArgsBytes = new byte[2];

            functionArgsBytes[0] = (byte)FunctionType.RequestNodeInfo;
            functionArgsBytes[1] = destinationNodeId;

            return new RequestNodeInfoTx(functionArgsBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new RequestNodeInfoRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.RequestNodeInfo;
        }
    }
}