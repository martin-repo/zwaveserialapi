// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;
    using System.Text;

    using ZWaveSerialApi.Utilities;

    public record DeviceType(ushort ManufacturerId, ushort ProductTypeId, ushort ProductId)
    {
        public override string ToString()
        {
            var manufacturerIdString = BitConverter.ToString(EndianHelper.GetBytes(ManufacturerId)).Replace("-", string.Empty);
            var productTypeIdString = BitConverter.ToString(EndianHelper.GetBytes(ProductTypeId)).Replace("-", string.Empty);
            var productIdString = BitConverter.ToString(EndianHelper.GetBytes(ProductId)).Replace("-", string.Empty);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{nameof(ManufacturerId)}=0x{manufacturerIdString}");
            stringBuilder.Append($",{nameof(ProductTypeId)}=0x{productTypeIdString}");
            stringBuilder.Append($",{nameof(ProductId)}=0x{productIdString}");
            return stringBuilder.ToString();
        }
    }
}