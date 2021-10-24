// -------------------------------------------------------------------------------------------------
// <copyright file="GetSucNodeIdResponse.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave
{
    public record MemoryGetIdResponse(uint HomeId, byte NodeId);
}