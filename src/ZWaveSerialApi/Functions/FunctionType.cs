// -------------------------------------------------------------------------------------------------
// <copyright file="FunctionType.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    internal enum FunctionType
    {
        AddNodeToNetwork = 0x4A,
        ApplicationCommandHandlerBridge = 0xA8,
        ApplicationUpdate = 0x49,
        GetNodeProtocolInfo = 0x41,
        GetSucNodeId = 0x56,
        MemoryGetId = 0x20,
        RequestNodeInfo = 0x60,
        SendData = 0x13,
        SendDataAbort = 0x16,
        SerialApiGetInitData = 0x02,
        SerialApiSetup = 0x0B,
        TypeLibrary = 0xBD
    }
}