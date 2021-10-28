// -------------------------------------------------------------------------------------------------
// <copyright file="NetworkSettings.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System.Collections.Generic;

    using ZWaveSerialApi.Devices.Device;

    public class NetworkSettings
    {
        public List<DeviceState> DeviceStates { get; set; } = new();
    }
}