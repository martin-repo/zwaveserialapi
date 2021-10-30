// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceFactory.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ZWaveSerialApi.Devices.Utilities;

    internal class DeviceFactory
    {
        private const string BrandsNamespace = "ZWaveSerialApi.Devices.Brands";

        private readonly IZWaveSerialClient _client;
        private readonly Dictionary<DeviceType, DeviceConstructionDelegate> _deviceDelegates = new();

        public DeviceFactory(IZWaveSerialClient client)
        {
            _client = client;

            AddDeviceDelegates();
        }

        public bool CanCreate(DeviceType deviceType)
        {
            return _deviceDelegates.ContainsKey(deviceType);
        }

        public IDevice Create(DeviceState deviceState)
        {
            return _deviceDelegates[deviceState.DeviceType](_client, deviceState);
        }

        private void AddDeviceDelegates()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var brandTypes = assembly.DefinedTypes.Where(
                type => type.IsClass && !type.IsNested && type.Namespace!.StartsWith(BrandsNamespace, StringComparison.Ordinal));

            foreach (var brandType in brandTypes)
            {
                var deviceType = AttributeHelper.GetDeviceType(brandType);
                if (deviceType == null)
                {
                    continue;
                }

                var constructor = brandType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();

                _deviceDelegates.Add(deviceType, (client, deviceState) => (IDevice)constructor.Invoke(new object[] { client, deviceState }));
            }
        }
    }
}