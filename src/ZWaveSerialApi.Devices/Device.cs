// -------------------------------------------------------------------------------------------------
// <copyright file="Device.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    public abstract class Device
    {
        protected Device(ZWaveSerialClient client, byte nodeId)
        {
            Client = client;
            NodeId = nodeId;
        }

        public ZWaveSerialClient Client { get; set; }

        public byte NodeId { get; set; }
    }
}