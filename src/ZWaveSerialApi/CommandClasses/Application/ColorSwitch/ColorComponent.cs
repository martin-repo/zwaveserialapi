// -------------------------------------------------------------------------------------------------
// <copyright file="ColorComponent.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.ColorSwitch
{
    public record ColorComponent(ColorComponentType Type, byte Value);
}