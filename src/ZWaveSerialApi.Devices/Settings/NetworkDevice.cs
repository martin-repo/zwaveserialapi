// -------------------------------------------------------------------------------------------------
// <copyright file="NetworkDevice.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Settings
{
    using ZWaveSerialApi.Devices.Brands;

    public class NetworkDevice
    {
        public bool IsAlwaysOn { get; set; }

        public bool IsListening { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DeviceType Type { get; set; }
    }
}