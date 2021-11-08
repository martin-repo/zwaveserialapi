// -------------------------------------------------------------------------------------------------
// <copyright file="AddDeviceResult.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using ZWaveSerialApi.Devices.Device;

    public record AddDeviceResult(bool Success, IDevice? Device);
}