// -------------------------------------------------------------------------------------------------
// <copyright file="FibaroMotionSensorParameters.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Fibaro.Configuration
{
    using ZWaveSerialApi.CommandClasses.Application.Configuration;
    using ZWaveSerialApi.Devices.Brands.Configuration;

    public class FibaroMotionSensorParameters
    {
        internal FibaroMotionSensorParameters(FibaroMotionSensor device, ConfigurationCommandClass configuration)
        {
            MotionTimeout = new MotionTimeout(device, 0x06, 1, 32767, configuration);
        }

        public MotionTimeout MotionTimeout { get; }
    }
}