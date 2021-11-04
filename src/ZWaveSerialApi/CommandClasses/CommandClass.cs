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
        protected CommandClass(CommandClassType type, IZWaveSerialClient client)
        {
            Type = type;
            Client = client;
        }

        public CommandClassType Type { get; }

        protected IZWaveSerialClient Client { get; }

        protected async Task<T> WaitForResponseAsync<T>(
            byte destinationNodeId,
            byte[] commandClassBytes,
            ConcurrentDictionary<byte, TaskCompletionSource<T>> callbackSources,
            CancellationToken cancellationToken)
        {
            var callbackSource = callbackSources.GetOrAdd(
                destinationNodeId,
                _ => new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously));

            try
            {
                await Client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken).ConfigureAwait(false);

                if (await Task.WhenAny(callbackSource.Task, Task.Delay(Client.CallbackTimeout, cancellationToken)).ConfigureAwait(false)
                    == callbackSource.Task)
                {
                    return callbackSource.Task.Result;
                }

                throw new TimeoutException("Timeout waiting for response.");
            }
            finally
            {
                callbackSources.TryRemove(destinationNodeId, out _);
            }
        }

        internal abstract void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes);
    }
}