// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceConstructionDelegate.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    public delegate IDevice DeviceConstructionDelegate(IZWaveSerialClient client, DeviceState deviceState);
}