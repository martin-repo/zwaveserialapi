// -------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationReport.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Configuration
{
    public class ConfigurationReport
    {
        public ConfigurationReport(byte parameterNumber, byte[] value)
        {
            ParameterNumber = parameterNumber;
            Value = value;
        }

        public byte ParameterNumber { get; }

        public byte[] Value { get; }
    }
}