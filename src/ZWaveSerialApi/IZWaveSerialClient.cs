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

    public interface IZWaveSerialClient
    {
        TimeSpan CallbackTimeout { get; }

        byte ControllerNodeId { get; }

        CommandClass GetCommandClass(CommandClassType type);

        Task SendDataAsync(byte destinationNodeId, byte[] commandClassBytes, CancellationToken cancellationToken = default);
    }
}