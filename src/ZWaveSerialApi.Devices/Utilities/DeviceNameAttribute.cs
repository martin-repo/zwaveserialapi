// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceNameAttribute.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    internal class DeviceNameAttribute : Attribute
    {
        public DeviceNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}