// -------------------------------------------------------------------------------------------------
// <copyright file="Device.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;

    public abstract class Device
    {
        protected Device(ZWaveSerialClient client, byte nodeId)
        {
            Client = client;
            NodeId = nodeId;
        }

        internal ZWaveSerialClient Client { get;  }

        public byte NodeId { get; }
    }
}