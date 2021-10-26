// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerNameAttribute.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    internal class ManufacturerNameAttribute : Attribute
    {
        public ManufacturerNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}