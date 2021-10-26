// -------------------------------------------------------------------------------------------------
// <copyright file="UpdateState.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.RequestNodeInfo
{
    internal enum UpdateState
    {
        NodeInfoRequestFailed = 0x81,
        NodeInfoReceived = 0x84
    }
}