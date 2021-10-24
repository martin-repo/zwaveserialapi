// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiGetInitDataResponse.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.SerialApi
{
    public record SerialApiGetInitDataResponse(bool IsStaticUpdateController, byte[] DeviceNodeIds);
}