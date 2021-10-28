// -------------------------------------------------------------------------------------------------
// <copyright file="Device.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using ZWaveSerialApi.Devices.Utilities;

    public abstract class Device : IDevice
    {
        private readonly DeviceState _deviceState;

        internal Device(IZWaveSerialClient client, DeviceState deviceState)
        {
            Client = client;
            _deviceState = deviceState;
            Name = AttributeHelper.GetDeviceName(GetType());
        }

        public bool IsAlwaysOn => _deviceState.IsAlwaysOn;

        public bool IsListening => _deviceState.IsListening;

        public string Location
        {
            get => _deviceState.Location;
            set => _deviceState.Location = value;
        }

        public string Name { get; }

        public byte NodeId => _deviceState.NodeId;

        internal IZWaveSerialClient Client { get; }
    }
}