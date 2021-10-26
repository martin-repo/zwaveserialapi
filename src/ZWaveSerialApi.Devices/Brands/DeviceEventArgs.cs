// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands
{
    using System;

    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs(Device device)
        {
            Device = device;
        }

        public Device Device { get; }
    }
}