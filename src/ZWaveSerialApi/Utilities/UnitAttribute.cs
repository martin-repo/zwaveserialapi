// -------------------------------------------------------------------------------------------------
// <copyright file="UnitAttribute.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Utilities
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class UnitAttribute : Attribute
    {
        public UnitAttribute(string unit)
        {
            Unit = unit;
            Label = string.Empty;
        }

        public UnitAttribute(string unit, string label)
        {
            Unit = unit;
            Label = label;
        }

        public string Label { get; }

        public string Unit { get; }
    }
}