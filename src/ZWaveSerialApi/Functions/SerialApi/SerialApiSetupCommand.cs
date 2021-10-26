// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiSetupCommand.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.SerialApi
{
    internal enum SerialApiSetupCommand
    {
        StatusReport = 0x02,
        PowerlevelSet = 0x04,
        PowerlevelGet = 0x08,
        GetMaxPayloadSize = 0x10
    }
}