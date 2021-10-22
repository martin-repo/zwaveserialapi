// -------------------------------------------------------------------------------------------------
// <copyright file="AsyncManualResetEvent.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Utilities
{
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncManualResetEvent
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private TaskCompletionSource _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsSet
        {
            get
            {
                _semaphore.Wait();
                try
                {
                    return _completionSource.Task.IsCompleted;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public void Reset()
        {
            _semaphore.Wait();
            try
            {
                if (!_completionSource.Task.IsCompleted)
                {
                    return;
                }

                _completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Set()
        {
            _semaphore.Wait();
            try
            {
                if (_completionSource.Task.IsCompleted)
                {
                    return;
                }

                _completionSource.SetResult();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            Task task;

            _semaphore.Wait(cancellationToken);
            try
            {
                if (_completionSource.Task.IsCompleted || !cancellationToken.CanBeCanceled)
                {
                    return _completionSource.Task;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled(cancellationToken);
                }

                task = _completionSource.Task;
            }
            finally
            {
                _semaphore.Release();
            }

            var waitCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = cancellationToken.Register(() => waitCompletionSource.SetCanceled(cancellationToken));
            return cancellationToken.IsCancellationRequested ? Task.FromCanceled(cancellationToken) : Task.WhenAny(task, waitCompletionSource.Task);
        }
    }
}