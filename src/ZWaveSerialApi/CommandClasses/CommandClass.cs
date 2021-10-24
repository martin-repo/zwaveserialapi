// -------------------------------------------------------------------------------------------------
// <copyright file="CommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class CommandClass
    {
        protected CommandClass(IZWaveSerialClient client)
        {
            Client = client;
        }

        protected IZWaveSerialClient Client { get; }

        protected async Task<T> WaitForResponseAsync<T>(
            byte destinationNodeId,
            byte[] commandClassBytes,
            ConcurrentDictionary<byte, TaskCompletionSource<T>> callbackSources,
            CancellationToken cancellationToken)
        {
            var callbackSource = callbackSources.GetOrAdd(destinationNodeId, _ => new TaskCompletionSource<T>());

            await Client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken);
            if (await Task.WhenAny(callbackSource.Task, Task.Delay(Client.Timeout, cancellationToken)) == callbackSource.Task)
            {
                return callbackSource.Task.Result;
            }

            callbackSources.TryRemove(destinationNodeId, out _);
            throw new TimeoutException("Timeout waiting for response.");
        }

        internal abstract void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes);
    }
}