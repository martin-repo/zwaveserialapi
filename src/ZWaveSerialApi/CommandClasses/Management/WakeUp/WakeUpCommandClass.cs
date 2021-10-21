// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.WakeUp
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class WakeUpCommandClass : CommandClass
    {
        private readonly IZWaveSerialClient _client;
        private readonly ILogger _logger;

        public WakeUpCommandClass(ILogger logger, IZWaveSerialClient client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
            _client = client;
        }

        public event EventHandler<WakeUpIntervalCapabilitiesEventArgs>? IntervalCapabilitiesReport;

        public event EventHandler<WakeUpIntervalEventArgs>? IntervalReport;

        public event EventHandler<WakeUpNotificationEventArgs>? Notification;

        public async Task<bool> IntervalCapabilitiesGetAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.IntervalCapabilitiesGet;

            return await _client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken);
        }

        public async Task<bool> IntervalGetAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.IntervalGet;

            return await _client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken);
        }

        public async Task<bool> IntervalSetAsync(byte destinationNodeId, TimeSpan interval, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[6];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.IntervalSet;

            EndianHelper.GetBytes((int)interval.TotalSeconds, 3).CopyTo(commandClassBytes, 2);

            commandClassBytes[5] = destinationNodeId;

            return await _client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken);
        }

        public async Task<bool> NoMoreInformationAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var commandClassBytes = new byte[2];
            commandClassBytes[0] = (byte)CommandClassType.WakeUp;
            commandClassBytes[1] = (byte)WakeUpCommand.NoMoreInformation;

            return await _client.SendDataAsync(destinationNodeId, commandClassBytes, cancellationToken);
        }

        public override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
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
                    _logger.Error("Unsupported wake up command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                    return;
            }
        }




        private void ProcessIntervalCapabilitiesReport(byte sourceNodeId, byte[] commandClassBytes)
        {
            var minimumInterval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[2..5]));
            var maximumInterval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[5..8]));
            var defaultInterval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[8..11]));
            var intervalStep = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[11..14]));

            IntervalCapabilitiesReport?.Invoke(
                this,
                new WakeUpIntervalCapabilitiesEventArgs(sourceNodeId, minimumInterval, maximumInterval, defaultInterval, intervalStep));
        }

        private void ProcessIntervalReport(byte sourceNodeId, byte[] commandClassBytes)
        {
            var nodeId = commandClassBytes[5];

            //if (nodeId != controller node id)
            //{
            //    _logger.Error("Received interval report from {SourceNodeId} intended for {DestinationNodeId}", sourceNodeId, nodeId);
            //    return;
            //}

            var interval = TimeSpan.FromSeconds(EndianHelper.ToInt32(commandClassBytes[2..5]));
            IntervalReport?.Invoke(this, new WakeUpIntervalEventArgs(sourceNodeId, interval));
        }
    }
}