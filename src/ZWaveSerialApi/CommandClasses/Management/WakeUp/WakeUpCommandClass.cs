// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.WakeUp
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Functions.ZWave.SendData;
    using ZWaveSerialApi.Utilities;

    public class WakeUpCommandClass : CommandClass
    {
        private readonly ConcurrentDictionary<byte, TaskCompletionSource<WakeUpIntervalCapabilitiesReport>>
            _intervalCapabilitiesReportCallbackSources = new();

        private readonly ConcurrentDictionary<byte, TaskCompletionSource<TimeSpan>> _intervalReportCallbackSources = new();
        private readonly ILogger _logger;

        public WakeUpCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(client)
        {
            _logger = logger.ForContext<WakeUpCommandClass>().ForContext(Constants.ClassName, GetType().Name);
        }

        public event EventHandler<WakeUpNotificationEventArgs>? Notification;

        public async Task<WakeUpIntervalCapabilitiesReport> IntervalCapabilitiesGetAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.IntervalCapabilitiesGet;

            return await WaitForResponseAsync(destinationNodeId, commandClassBytes, _intervalCapabilitiesReportCallbackSources, cancellationToken);
        }

        public async Task<TimeSpan> IntervalGetAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.IntervalGet;

            return await WaitForResponseAsync(destinationNodeId, commandClassBytes, _intervalReportCallbackSources, cancellationToken);
        }

        public async Task IntervalSetAsync(byte destinationNodeId, TimeSpan interval, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[6];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.IntervalSet;

            EndianHelper.GetBytes((int)interval.TotalSeconds, 3).CopyTo(commandClassBytes, 2);

            commandClassBytes[5] = Client.ControllerNodeId;

            await Client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken);
        }

        public async Task NoMoreInformationAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.NoMoreInformation;

            // Only use TransmitOption.Ack since node is known after having sent wake up notification.
            await Client.SendDataAsync(destinationNodeId, commandClassBytes, TransmitOption.Ack, cancellationToken);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (WakeUpCommand)commandClassBytes[1];
            switch (command)
            {
                case WakeUpCommand.IntervalReport:
                    ProcessIntervalReport(sourceNodeId, commandClassBytes);
                    break;
                case WakeUpCommand.Notification:
                    Notification?.Invoke(this, new WakeUpNotificationEventArgs(sourceNodeId));
                    break;
                case WakeUpCommand.IntervalCapabilitiesReport:
                    ProcessIntervalCapabilitiesReport(sourceNodeId, commandClassBytes);
                    break;
                default:
                    _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                    return;
            }
        }

        private void ProcessIntervalCapabilitiesReport(byte sourceNodeId, byte[] commandClassBytes)
        {
            if (!_intervalCapabilitiesReportCallbackSources.TryRemove(sourceNodeId, out var callbackSource))
            {
                return;
            }

            var minimumInterval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[2..5]));
            var maximumInterval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[5..8]));
            var defaultInterval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[8..11]));
            var intervalStep = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[11..14]));

            var intervalCapabilitiesReport = new WakeUpIntervalCapabilitiesReport(minimumInterval, maximumInterval, defaultInterval, intervalStep);
            callbackSource.TrySetResult(intervalCapabilitiesReport);
        }

        private void ProcessIntervalReport(byte sourceNodeId, byte[] commandClassBytes)
        {
            var destinationNodeId = commandClassBytes[5];

            if (destinationNodeId != Client.ControllerNodeId)
            {
                _logger.Error("Received interval report from {SourceNodeId} intended for {DestinationNodeId}", sourceNodeId, destinationNodeId);
                return;
            }

            if (!_intervalReportCallbackSources.TryRemove(sourceNodeId, out var callbackSource))
            {
                return;
            }

            var interval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[2..5]));

            callbackSource.TrySetResult(interval);
        }
    }
}