// -------------------------------------------------------------------------------------------------
// <copyright file="AttributeHelper.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    using System;
    using System.Reflection;

    using ZWaveSerialApi.Devices.Device;

    internal class AttributeHelper
    {
        public static string? GetDeviceName(Type type)
        {
            return type.GetCustomAttribute<DeviceNameAttribute>()?.Name;
        }

        public static DeviceType? GetDeviceType(Type type)
        {
            return type.GetCustomAttribute<DeviceTypeAttribute>()?.DeviceType;
        }
    }
}