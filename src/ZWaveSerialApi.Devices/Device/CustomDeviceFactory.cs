// -------------------------------------------------------------------------------------------------
// <copyright file="CustomDeviceFactory.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System.Collections.Generic;

    internal class CustomDeviceFactory
    {
        private readonly IZWaveSerialClient _client;
        private readonly Dictionary<DeviceType, DeviceConstructionDelegate> _customDeviceDelegates = new();

        public CustomDeviceFactory(IZWaveSerialClient client)
        {
            _client = client;
        }

        public bool CanCreate(DeviceType deviceType)
        {
            return _customDeviceDelegates.ContainsKey(deviceType);
        }

        public IDevice Create(DeviceState deviceState)
        {
            return _customDeviceDelegates[deviceState.DeviceType](_client, deviceState);
        }

        public void RegisterDeviceType(DeviceType deviceType, DeviceConstructionDelegate deviceConstructionDelegate)
        {
            _customDeviceDelegates.Add(deviceType, deviceConstructionDelegate);
        }
    }
}