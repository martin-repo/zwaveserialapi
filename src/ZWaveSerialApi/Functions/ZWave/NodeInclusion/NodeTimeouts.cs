// -------------------------------------------------------------------------------------------------
// <copyright file="NodeTimeouts.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion
{
    using System;

    internal record NodeTimeouts(
        TimeSpan LearnReadyTimeout,
        TimeSpan NodeFoundTimeout,
        TimeSpan SlaveTimeout,
        TimeSpan ControllerTimeout);
}