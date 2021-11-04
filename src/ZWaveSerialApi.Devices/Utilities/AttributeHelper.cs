// -------------------------------------------------------------------------------------------------
// <copyright file="AttributeHelper.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ZWaveSerialApi.Devices.Device;

    internal class AttributeHelper
    {
        public static string? GetDeviceName(Type type)
        {
            return type.GetCustomAttribute<DeviceNameAttribute>()?.Name;
        }

        public static IEnumerable<DeviceType> GetDeviceTypes(Type type)
        {
            return type.GetCustomAttributes<DeviceTypeAttribute>().Select(attribute => attribute.DeviceType);
        }
    }
}