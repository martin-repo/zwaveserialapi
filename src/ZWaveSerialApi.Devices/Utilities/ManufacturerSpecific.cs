// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerId.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Utilities
{
    public record ManufacturerSpecific(short ManufacturerId, short ProductTypeId, short ProductId);
}