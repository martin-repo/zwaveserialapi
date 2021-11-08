// -------------------------------------------------------------------------------------------------
// <copyright file="RemoveNodeFromNetworkRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion.RemoveNodeFromNetwork
{
    using System;

    internal class RemoveNodeFromNetworkRx : FunctionRx, INodeFunctionRx
    {
        public RemoveNodeFromNetworkRx(byte[] returnValueBytes)
            : base(FunctionType.RemoveNodeFromNetwork, returnValueBytes)
        {
            CompletedFuncId = returnValueBytes[1];
            Status = (NodeStatus)returnValueBytes[2];
            SourceNodeId = returnValueBytes[3];

            var length = returnValueBytes[4];
            if (length == 0)
            {
                DeviceClass = new DeviceClass(0, 0, 0);
                CommandClasses = Array.Empty<byte>();
                return;
            }

            var basic = returnValueBytes[5];
            var generic = returnValueBytes[6];
            var specific = returnValueBytes[7];
            DeviceClass = new DeviceClass(basic, generic, specific);

            CommandClasses = returnValueBytes[7..(7 + length - 3)];
        }

        public byte[] CommandClasses { get; }

        public byte CompletedFuncId { get; }

        public DeviceClass DeviceClass { get; }

        public byte SourceNodeId { get; }

        public NodeStatus Status { get; }
    }
}