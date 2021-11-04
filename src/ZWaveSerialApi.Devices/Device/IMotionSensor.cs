// -------------------------------------------------------------------------------------------------
// <copyright file="IMotionSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMotionSensor
    {
        event EventHandler? MotionDetected;

        event EventHandler? MotionIdle;

        Task<TimeSpan> GetMotionTimeoutAsync(CancellationToken cancellationToken = default);
    }
}