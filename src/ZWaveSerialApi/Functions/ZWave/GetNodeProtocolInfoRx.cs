// -------------------------------------------------------------------------------------------------
// <copyright file="GetNodeProtocolInfoRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave
{
    using ZWaveSerialApi.Utilities;

    internal class GetNodeProtocolInfoRx : FunctionRx
    {
        public GetNodeProtocolInfoRx(byte[] returnValueBytes)
            : base(FunctionType.GetNodeProtocolInfo, returnValueBytes)
        {
            Listening = BitHelper.IsSet(returnValueBytes[1], 7);
            var securityAndSensorMetadata = returnValueBytes[2];
            var reserved = returnValueBytes[3];
            var basic = returnValueBytes[4];
            var generic = returnValueBytes[5];
            var specific = returnValueBytes[6];
        }

        public bool Listening { get; }
    }
}