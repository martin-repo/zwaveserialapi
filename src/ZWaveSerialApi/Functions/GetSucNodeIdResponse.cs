// -------------------------------------------------------------------------------------------------
// <copyright file="GetSucNodeIdResponse.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    public record GetSucNodeIdResponse(bool Success, byte NodeId);
}