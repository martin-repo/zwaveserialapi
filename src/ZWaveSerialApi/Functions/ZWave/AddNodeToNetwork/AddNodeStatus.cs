// -------------------------------------------------------------------------------------------------
// <copyright file="AddNodeStatus.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.AddNodeToNetwork
{
    using System;

    // ADD_NODE_STATUS_* @ ZW_controller_api.h
    [Flags]
    internal enum AddNodeStatus
    {
        LearnReady = 1,
        NodeFound = 2,
        AddingSlave = 3,
        AddingController = 4,
        ProtocolDone = 5,
        Done = 6,
        Failed = 7,
        NotPrimary = 0x23
    }
}