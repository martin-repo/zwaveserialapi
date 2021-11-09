// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceTypeAttribute.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    using System;

    using ZWaveSerialApi.Devices.Device;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class DeviceTypeAttribute : Attribute
    {
        public DeviceTypeAttribute(
            ushort manufacturerId,
            ushort productTypeId,
            ushort productId,
            ZWaveRegion region = ZWaveRegion.Unknown)
        {
            DeviceType = new DeviceType(manufacturerId, productTypeId, productId);
        }

        public DeviceType DeviceType { get; }
    }
}