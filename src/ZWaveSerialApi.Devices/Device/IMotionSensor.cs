// -------------------------------------------------------------------------------------------------
// <copyright file="IMotionSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;

    public interface IMotionSensor : IDevice
    {
        event EventHandler? MotionDetected;

        event EventHandler? MotionIdle;
    }
}