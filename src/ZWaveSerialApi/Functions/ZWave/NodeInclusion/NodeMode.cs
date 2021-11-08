// -------------------------------------------------------------------------------------------------
// <copyright file="AddNodeMode.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion
{
    using System;

    // ADD_NODE_* @ ZW_controller_api.h
    // REMOVE_NODE_* @ ZW_controller_api.h
    [Flags]
    internal enum NodeMode
    {
        // Commands, only one of these may be used
        Any = 1,
        Controller = 2,
        Slave = 3,
        Existing = 4,
        Stop = 5,
        StopFailed = 6,
        HomeId = 8,
        SmartStart = 9,

        // Flags, multiple of these may be used
        OptionLr = 0x20,
        OptionNetworkWide = 0x40,
        OptionNormalPower = 0x80
    }
}