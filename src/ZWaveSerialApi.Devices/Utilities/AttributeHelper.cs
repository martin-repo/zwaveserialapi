// -------------------------------------------------------------------------------------------------
// <copyright file="AttributeHelper.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    using System;

    public class AttributeHelper
    {
        public static string GetManufacturerName(Enum @enum)
        {
            var manufacturerNameAttribute = @enum.GetAttribute<ManufacturerNameAttribute>();
            return manufacturerNameAttribute.Name;
        }

        public static ManufacturerSpecific GetManufacturerSpecific(Enum @enum)
        {
            var manufacturerSpecificAttribute = @enum.GetAttribute<ManufacturerSpecificAttribute>();
            return manufacturerSpecificAttribute.ManufacturerSpecific;
        }
    }
}