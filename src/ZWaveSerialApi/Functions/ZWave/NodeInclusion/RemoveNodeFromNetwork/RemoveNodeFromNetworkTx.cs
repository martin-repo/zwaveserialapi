// -------------------------------------------------------------------------------------------------
// <copyright file="RemoveNodeFromNetworkTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion.RemoveNodeFromNetwork
{
    internal class RemoveNodeFromNetworkTx : FunctionTx
    {
        private readonly byte _callbackFuncId;

        private RemoveNodeFromNetworkTx(byte[] functionArgsBytes, byte callbackFuncId)
            : base(functionArgsBytes)
        {
            _callbackFuncId = callbackFuncId;
        }

        public override bool HasReturnValue => true;

        public static RemoveNodeFromNetworkTx Create(NodeMode mode, byte callbackFuncId)
        {
            var functionArgsBytes = new byte[3];

            functionArgsBytes[0] = (byte)FunctionType.RemoveNodeFromNetwork;
            functionArgsBytes[1] = (byte)mode;
            functionArgsBytes[2] = callbackFuncId;

            return new RemoveNodeFromNetworkTx(functionArgsBytes, callbackFuncId);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new RemoveNodeFromNetworkRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.RemoveNodeFromNetwork && returnValueBytes[1] == _callbackFuncId;
        }
    }
}