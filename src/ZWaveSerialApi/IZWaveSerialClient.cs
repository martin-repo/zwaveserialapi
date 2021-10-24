// -------------------------------------------------------------------------------------------------
// <copyright file="IZWaveSerialClient.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses;
    using ZWaveSerialApi.Functions;

    public interface IZWaveSerialClient
    {
        byte NodeId { get; }
        TimeSpan Timeout { get; }

        T GetCommandClass<T>()
            where T : CommandClass;

        CommandClass GetCommandClass(CommandClassType type);

        Task SendDataAsync(byte destinationNodeId, byte[] commandClassBytes, CancellationToken cancellationToken);

        Task<byte[]> SerialApiSetupAsync(bool enableStatusReport, CancellationToken cancellationToken);
    }
}