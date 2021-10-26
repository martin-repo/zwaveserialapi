// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerSpecificAttribute.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    internal class ManufacturerSpecificAttribute : Attribute
    {
        public ManufacturerSpecificAttribute(short manufacturerId, short productTypeId, short productId)
        {
            ManufacturerSpecific = new ManufacturerSpecific(manufacturerId, productTypeId, productId);
        }

        public ManufacturerSpecific ManufacturerSpecific { get; }
    }
}