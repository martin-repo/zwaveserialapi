// -------------------------------------------------------------------------------------------------
// <copyright file="LibraryType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.TypeLibrary
{
    public enum LibraryType
    {
        StaticController = 0x01,
        Controller = 0x02,
        EnhancedSlave = 0x03,
        Slave = 0x04,
        Installer = 0x05,
        RoutingSlave = 0x06,
        BridgeController = 0x07
    }
}