// -------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationCommand.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Configuration
{
    // CONFIGURATION_* @ ZW_classcmd.h
    internal enum ConfigurationCommand
    {
        Get = 0x05,
        Report = 0x06,
        Set = 0x04
    }
}