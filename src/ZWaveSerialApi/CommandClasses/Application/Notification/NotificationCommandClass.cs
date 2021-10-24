// -------------------------------------------------------------------------------------------------
// <copyright file="NotificationCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Application.Notification
{
    using System;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class NotificationCommandClass : CommandClass
    {
        private readonly ILogger _logger;

        public NotificationCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
        }

        public event EventHandler<HomeSecurityEventArgs>? HomeSecurityStateChanged;

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (NotificationCommand)commandClassBytes[1];
            if (command != NotificationCommand.Report)
            {
                _logger.Error("Unsupported notification command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                return;
            }

            var type = (NotificationType)commandClassBytes[6];
            switch (type)
            {
                case NotificationType.HomeSecurity:
                    ProcessHomeSecurity(sourceNodeId, commandClassBytes);
                    break;
                default:
                    _logger.Error("Unsupported notification type {Type}", BitConverter.ToString(commandClassBytes, 2, 1));
                    break;
            }
        }

        private (bool HasSequenceNumber, byte ParameterLength) DeconstructMetadataByte(byte metadataByte)
        {
            //  7 6 5 4 3 2 1 0
            // |-| sequence
            //   |---| reserved
            //       |---------| length
            var hasSequenceNumber = metadataByte >> 7 != 0;
            var parameterLength = (byte)(metadataByte & BitHelper.Bit4Mask);

            return (hasSequenceNumber, parameterLength);
        }

        private void ProcessHomeSecurity(byte sourceNodeId, byte[] commandClassBytes)
        {
            var (hasSequenceNumber, parameterLength) = DeconstructMetadataByte(commandClassBytes[8]);
            var sequenceNumber = hasSequenceNumber ? commandClassBytes[9 + parameterLength] : -1;

            var state = (HomeSecurityState)commandClassBytes[7];
            switch (state)
            {
                case HomeSecurityState.Idle:
                case HomeSecurityState.CoverTampering:
                case HomeSecurityState.MotionDetection:
                    var parameters = parameterLength > 0 ? commandClassBytes[9..(9 + parameterLength)] : Array.Empty<byte>();
                    HomeSecurityStateChanged?.Invoke(this, new HomeSecurityEventArgs(sourceNodeId, state, parameters));
                    break;
                default:
                    _logger.Error("Unsupported home security state {State}", BitConverter.ToString(commandClassBytes, 7, 1));
                    return;
            }
        }
    }
}