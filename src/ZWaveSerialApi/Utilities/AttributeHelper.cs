// -------------------------------------------------------------------------------------------------
// <copyright file="AttributeHelper.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Utilities
{
    using System;

    public class AttributeHelper
    {
        public static Type GetScaleEnumType(Enum @enum)
        {
            var scaleAttribute = @enum.GetAttribute<ScaleAttribute>();
            return scaleAttribute.ScaleEnumType;
        }

        public static (string Unit, string Label) GetUnit(Enum @enum)
        {
            var unitAttribute = @enum.GetAttribute<UnitAttribute>();
            return (unitAttribute.Unit, unitAttribute.Label);
        }
    }
}