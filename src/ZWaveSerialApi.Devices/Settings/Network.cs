// -------------------------------------------------------------------------------------------------
// <copyright file="Network.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Settings
{
    using System.Collections.Generic;

    public class Network
    {
        public Dictionary<byte, NetworkDevice> Devices { get; set; } = new();
    }
}