// -------------------------------------------------------------------------------------------------
// <copyright file="AeotecMultiSensor6Parameters.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Aeotec.Configuration
{
    using ZWaveSerialApi.CommandClasses.Application.Configuration;
    using ZWaveSerialApi.Devices.Brands.Configuration;

    public class AeotecMultiSensor6Parameters
    {
        internal AeotecMultiSensor6Parameters(AeotecMultiSensor6 device, ConfigurationCommandClass configuration)
        {
            MotionTimeout = new MotionTimeout(device, 0x03, 10, 3600, configuration);
        }

        public MotionTimeout MotionTimeout { get; }
    }
}