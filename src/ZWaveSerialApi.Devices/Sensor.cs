// -------------------------------------------------------------------------------------------------
// <copyright file="Sensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Management.WakeUp;

    public abstract class Sensor : Device
    {
        protected Sensor(ZWaveSerialClient client, byte nodeId)
            : base(client, nodeId)
        {
            var wakeUp = Client.GetCommandClass<WakeUpCommandClass>();
            wakeUp.Notification += OnWakeUpNotification;
        }

        public event EventHandler<WakeUpNotificationEventArgs>? WakeUpNotification;

        public Task<TimeSpan> GetWakeUpIntervalAsync(CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<TimeSpan>();

            var wakeUp = Client.GetCommandClass<WakeUpCommandClass>();

            void IntervalReportHandler(object? sender, WakeUpIntervalEventArgs eventArgs)
            {
                if (eventArgs.SourceNodeId != NodeId)
                {
                    return;
                }

                completionSource.SetResult(eventArgs.Interval);

                wakeUp.IntervalReport -= IntervalReportHandler;
            }

            wakeUp.IntervalReport += IntervalReportHandler;
            _ = wakeUp.IntervalGetAsync(NodeId, cancellationToken);

            return completionSource.Task;
        }

        public Task<WakeUpIntervalCapabilities> GetWakeUpIntervalCapabilitiesAsync(CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<WakeUpIntervalCapabilities>();

            var wakeUp = Client.GetCommandClass<WakeUpCommandClass>();

            void IntervalCapabilitiesReportHandler(object? sender, WakeUpIntervalCapabilitiesEventArgs eventArgs)
            {
                if (eventArgs.SourceNodeId != NodeId)
                {
                    return;
                }

                var capabilities = new WakeUpIntervalCapabilities(
                    eventArgs.MinimumInterval,
                    eventArgs.MaximumInterval,
                    eventArgs.DefaultInterval,
                    eventArgs.IntervalStep);
                completionSource.SetResult(capabilities);

                wakeUp.IntervalCapabilitiesReport -= IntervalCapabilitiesReportHandler;
            }

            wakeUp.IntervalCapabilitiesReport += IntervalCapabilitiesReportHandler;
            _ = wakeUp.IntervalCapabilitiesGetAsync(NodeId, cancellationToken);

            return completionSource.Task;
        }

        public Task<bool> SetWakeUpIntervalAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<bool>();

            var wakeUp = Client.GetCommandClass<WakeUpCommandClass>();

            void IntervalReportHandler(object? sender, WakeUpIntervalEventArgs eventArgs)
            {
                if (eventArgs.SourceNodeId != NodeId)
                {
                    return;
                }

                completionSource.SetResult(eventArgs.Interval == interval);

                wakeUp.IntervalReport -= IntervalReportHandler;
            }

            wakeUp.IntervalReport += IntervalReportHandler;
            _ = wakeUp.IntervalSetAsync(NodeId, interval, cancellationToken)
                      .ContinueWith(
                          antecedent =>
                          {
                              if (!antecedent.Result)
                              {
                                  completionSource.TrySetResult(false);
                              }
                          },
                          TaskContinuationOptions.NotOnCanceled);

            return completionSource.Task;
        }

        public async Task<bool> WakeUpNoMoreInformationAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            var wakeUp = Client.GetCommandClass<WakeUpCommandClass>();
            return await wakeUp.NoMoreInformationAsync(NodeId, cancellationToken);
        }

        private void OnWakeUpNotification(object? sender, WakeUpNotificationEventArgs eventArgs)
        {
            if (eventArgs.SourceNodeId == NodeId)
            {
                WakeUpNotification?.Invoke(sender, eventArgs);
            }
        }
    }
}