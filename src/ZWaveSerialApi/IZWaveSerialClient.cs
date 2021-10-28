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
    using ZWaveSerialApi.Functions.ZWave.SendData;

    public interface IZWaveSerialClient
    {
        TimeSpan CallbackTimeout { get; }

        byte ControllerNodeId { get; }

        CommandClass GetCommandClass(CommandClassType type);

        T GetCommandClass<T>()
            where T : CommandClass;

        Task SendDataAsync(byte destinationNodeId, byte[] commandClassBytes, CancellationToken cancellationToken = default);

        Task SendDataAsync(
            byte destinationNodeId,
            byte[] commandClassBytes,
            TransmitOption transmitOptions,
            CancellationToken cancellationToken = default);
    }
}