// -------------------------------------------------------------------------------------------------
// <copyright file="AddNodeMode.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.AddNodeToNetwork
{
    using System;

    // ADD_NODE_* @ ZW_controller_api.h
    [Flags]
    internal enum AddNodeMode
    {
        // Commands, only one of these may be used
        Any = 1,
        Stop = 5,
        StopFailed = 6,

        // Flags, multiple of these may be used
        OptionNetworkWide = 0x40,
        OptionNormalPower = 0x80
    }
}