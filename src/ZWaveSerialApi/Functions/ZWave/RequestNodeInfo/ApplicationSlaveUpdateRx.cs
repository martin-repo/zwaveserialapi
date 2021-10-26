// -------------------------------------------------------------------------------------------------
// <copyright file="ApplicationSlaveUpdateRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.RequestNodeInfo
{
    using System;

    internal class ApplicationSlaveUpdateRx : FunctionRx
    {
        public ApplicationSlaveUpdateRx(byte[] returnValueBytes)
            : base(FunctionType.ApplicationUpdate, returnValueBytes)
        {
            State = (UpdateState)returnValueBytes[1];
            if (State != UpdateState.NodeInfoReceived)
            {
                DeviceClass = new DeviceClass(0, 0, 0);
                CommandClasses = Array.Empty<byte>();
                return;
            }

            SourceNodeId = returnValueBytes[2];
            var length = returnValueBytes[3];

            var basic = returnValueBytes[4];
            var generic = returnValueBytes[5];
            var specific = returnValueBytes[6];
            DeviceClass = new DeviceClass(basic, generic, specific);

            CommandClasses = returnValueBytes[7..(7 + length - 3)];
        }

        public byte[] CommandClasses { get; }

        public DeviceClass DeviceClass { get; }

        public byte SourceNodeId { get; }

        public UpdateState State { get; }
    }
}