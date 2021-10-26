// -------------------------------------------------------------------------------------------------
// <copyright file="DeviceClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.RequestNodeInfo
{
    public record DeviceClass(byte Basic, byte Generic, byte Specific);
}