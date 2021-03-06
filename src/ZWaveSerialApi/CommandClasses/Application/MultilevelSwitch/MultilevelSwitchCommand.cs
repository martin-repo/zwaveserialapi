// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSwitchCommand.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch
{
    internal enum MultilevelSwitchCommand
    {
        Set = 0x01,
        Get = 0x02,
        Report = 0x03
    }
}