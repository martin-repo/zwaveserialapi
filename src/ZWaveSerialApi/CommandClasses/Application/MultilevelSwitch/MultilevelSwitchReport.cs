// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSwitchReport.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch
{
    public class MultilevelSwitchReport
    {
        public MultilevelSwitchReport(byte value)
        {
            Value = value;
        }

        public byte Value { get; }
    }
}