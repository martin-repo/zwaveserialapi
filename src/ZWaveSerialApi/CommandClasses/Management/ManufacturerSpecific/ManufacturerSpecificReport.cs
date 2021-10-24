// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerSpecificReport.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific
{
    public class ManufacturerSpecificReport
    {
        public ManufacturerSpecificReport(short manufacturerId, short productTypeId, short productId)
        {
            ManufacturerId = manufacturerId;
            ProductTypeId = productTypeId;
            ProductId = productId;
        }

        public short ManufacturerId { get; }

        public short ProductId { get; }

        public short ProductTypeId { get; }
    }
}