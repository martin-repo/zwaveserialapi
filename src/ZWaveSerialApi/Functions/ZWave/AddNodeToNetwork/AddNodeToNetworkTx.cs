// -------------------------------------------------------------------------------------------------
// <copyright file="AddNodeToNetworkTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.AddNodeToNetwork
{
    internal class AddNodeToNetworkTx : FunctionTx
    {
        public const AddNodeMode DefaultAddMode = AddNodeMode.Any | AddNodeMode.OptionNormalPower | AddNodeMode.OptionNetworkWide;

        private readonly byte _callbackFuncId;

        private AddNodeToNetworkTx(byte[] functionArgsBytes, byte callbackFuncId)
            : base(functionArgsBytes)
        {
            _callbackFuncId = callbackFuncId;
        }

        public override bool HasReturnValue => true;

        public static AddNodeToNetworkTx Create(AddNodeMode mode, byte callbackFuncId)
        {
            var functionArgsBytes = new byte[3];

            functionArgsBytes[0] = (byte)FunctionType.AddNodeToNetwork;
            functionArgsBytes[1] = (byte)mode;
            functionArgsBytes[2] = callbackFuncId;

            return new AddNodeToNetworkTx(functionArgsBytes, callbackFuncId);
        }

        public override FunctionRx CreateReturnValue(byte[] returnValueBytes)
        {
            return new AddNodeToNetworkRx(returnValueBytes);
        }

        public override bool IsValidReturnValue(byte[] returnValueBytes)
        {
            var functionType = (FunctionType)returnValueBytes[0];
            return functionType == FunctionType.AddNodeToNetwork && returnValueBytes[1] == _callbackFuncId;
        }
    }
}