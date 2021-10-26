// -------------------------------------------------------------------------------------------------
// <copyright file="ScaleAttribute.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Utilities
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{ScaleEnumType.Name,nq}")]
    [AttributeUsage(AttributeTargets.Field)]
    internal class ScaleAttribute : Attribute
    {
        public ScaleAttribute(Type scaleEnumType)
        {
            ScaleEnumType = scaleEnumType;
        }

        public Type ScaleEnumType { get; }
    }
}