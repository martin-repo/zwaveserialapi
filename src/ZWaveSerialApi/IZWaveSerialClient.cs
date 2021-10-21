// -------------------------------------------------------------------------------------------------
// <copyright file="IZWaveSerialClient.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi
{
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses;
    using ZWaveSerialApi.Functions;

    public interface IZWaveSerialClient
    {
        T GetCommandClass<T>()
            where T : CommandClass;

        CommandClass GetCommandClass(CommandClassType type);

        Task<bool> SendDataAsync(byte destinationNodeId, byte[] commandClassBytes, CancellationToken cancellationToken);

        Task<SerialApiSetupResponse> SerialApiSetupAsync(bool enableStatusReport, CancellationToken cancellationToken);

        Task<bool> SetPromiscuousModeAsync(bool enabled, CancellationToken cancellationToken);
    }
}