// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;

    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs(IDevice device)
        {
            Device = device;
        }

        public IDevice Device { get; }
    }
}