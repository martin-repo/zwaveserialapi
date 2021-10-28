// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerSpecificReport.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific
{
    public class ManufacturerSpecificReport
    {
        public ManufacturerSpecificReport(ushort manufacturerId, ushort productTypeId, ushort productId)
        {
            ManufacturerId = manufacturerId;
            ProductTypeId = productTypeId;
            ProductId = productId;
        }

        public ushort ManufacturerId { get; }

        public ushort ProductId { get; }

        public ushort ProductTypeId { get; }
    }
}