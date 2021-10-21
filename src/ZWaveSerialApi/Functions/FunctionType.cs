// -------------------------------------------------------------------------------------------------
// <copyright file="FunctionType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    public enum FunctionType
    {
        ApplicationCommandHandlerBridge = 0xA8,
        ApplicationUpdate = 0x49,
        SendData = 0x13,
        SerialApiSetup = 0x0B,
        SetPromiscuousMode = 0xD0
    }
}