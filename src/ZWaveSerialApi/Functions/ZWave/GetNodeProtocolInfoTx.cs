// -------------------------------------------------------------------------------------------------
// <copyright file="GetNodeProtocolInfoTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave
{
    internal class GetNodeProtocolInfoTx : FunctionTx
    {
        private GetNodeProtocolInfoTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => true;

        public static GetNodeProtocolInfoTx Create(byte destinationNodeId)
        {
            var functionArgsBytes = new byte[2];

            functionArgsBytes[0] = (byte)FunctionType.GetNodeProtocolInfo;
            functionArgsBytes[1] = destinationNodeId;

            return new GetNodeProtocolInfoTx(functionArgsBytes);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new GetNodeProtocolInfoRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.GetNodeProtocolInfo;
        }
    }
}