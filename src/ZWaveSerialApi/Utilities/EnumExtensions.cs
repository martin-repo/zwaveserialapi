// -------------------------------------------------------------------------------------------------
// <copyright file="EnumExtensions.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Utilities
{
    using System;
    using System.Linq;

    internal static class EnumExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            var enumType = enumValue.GetType();
            var enumValueName = Enum.GetName(enumType, enumValue)!;
            return enumType.GetField(enumValueName)!.GetCustomAttributes(false).OfType<TAttribute>().Single();
        }
    }
}