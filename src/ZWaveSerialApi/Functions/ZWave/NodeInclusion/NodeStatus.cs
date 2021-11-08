// -------------------------------------------------------------------------------------------------
// <copyright file="NodeStatus.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion
{
    using System;

    // ADD_NODE_STATUS_* @ ZW_controller_api.h
    // REMOVE_NODE_STATUS_* @ ZW_controller_api.h
    [Flags]
    internal enum NodeStatus
    {
        LearnReady = 1,
        NodeFound = 2,
        Slave = 3,
        Controller = 4,
        ProtocolDone = 5,
        Done = 6,
        Failed = 7,
        NotPrimary = 0x23
    }
}