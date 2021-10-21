// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiSetupResponse.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    using System.Collections.ObjectModel;

    public record SerialApiSetupResponse(bool Success, ReadOnlyCollection<byte> Response);
}