// -------------------------------------------------------------------------------------------------
// <copyright file="IDevice.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    public interface IDevice
    {
        bool IsAlwaysOn { get; }

        bool IsListening { get; }

        string Location { get; set; }

        string Name { get; }

        byte NodeId { get; }
    }
}