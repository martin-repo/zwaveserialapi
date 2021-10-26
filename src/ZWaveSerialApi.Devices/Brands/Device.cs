// -------------------------------------------------------------------------------------------------
// <copyright file="Device.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands
{
    using ZWaveSerialApi.Devices.Settings;

    public abstract class Device
    {
        private readonly NetworkDevice _networkDevice;

        protected Device(ZWaveSerialClient client, byte nodeId, NetworkDevice networkDevice)
        {
            Client = client;
            NodeId = nodeId;
            _networkDevice = networkDevice;
        }

        public bool IsListening => _networkDevice.IsListening;

        public string Location
        {
            get => _networkDevice.Location;
            set => _networkDevice.Location = value;
        }

        public string Name => _networkDevice.Name;

        public byte NodeId { get; }

        internal ZWaveSerialClient Client { get; }
    }
}