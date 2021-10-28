// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceState.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    public class DeviceState
    {
        public DeviceType DeviceType { get; init; } = new(0, 0, 0);

        public bool IsAlwaysOn { get; init; }

        public bool IsListening { get; init; }

        public string Location { get; set; } = string.Empty;

        public byte NodeId { get; init; }
    }
}