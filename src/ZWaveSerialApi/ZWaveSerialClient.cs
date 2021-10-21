// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveSerialClient.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.CommandClasses;
    using ZWaveSerialApi.CommandClasses.Application.Basic;
    using ZWaveSerialApi.CommandClasses.Application.ColorSwitch;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch;
    using ZWaveSerialApi.CommandClasses.Application.Notification;
    using ZWaveSerialApi.CommandClasses.Management.Battery;
    using ZWaveSerialApi.CommandClasses.Management.WakeUp;
    using ZWaveSerialApi.CommandClasses.TransportEncapsulation.Crc16Encap;
    using ZWaveSerialApi.Frames;
    using ZWaveSerialApi.Functions;

    public class ZWaveSerialClient : IZWaveSerialClient
    {
        private const int Attempts = 3;
        private readonly Dictionary<CommandClassType, CommandClass> _commandClasses;
        private readonly ILogger _logger;
        private readonly TimeSpan _networkTimeout = TimeSpan.FromSeconds(1);
        private readonly IZWaveSerialPort _port;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);
        private readonly SemaphoreSlim _functionSemaphore = new(1, 1);

        public ZWaveSerialClient(ILogger logger, IZWaveSerialPort port)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
            _port = port;
            _port.DataFrameReceived += OnDataFrameReceived;

            _commandClasses = CreateCommandClasses();
        }

        public T GetCommandClass<T>()
            where T : CommandClass
        {
            var commandClass = _commandClasses.Values.FirstOrDefault(commandClass => commandClass is T);
            if (commandClass == default)
            {
                throw new ArgumentOutOfRangeException($"Unsupported {nameof(CommandClass)} {typeof(T).Name}");
            }

            return (T)commandClass;
        }

        public CommandClass GetCommandClass(CommandClassType type)
        {
            if (!_commandClasses.ContainsKey(type))
            {
                throw new ArgumentOutOfRangeException($"Unsupported {nameof(CommandClass)} {type}");
            }

            return _commandClasses[type];
        }

        public async Task<bool> SendDataAsync(byte destinationNodeId, byte[] commandClassBytes, CancellationToken cancellationToken)
        {
            await _functionSemaphore.WaitAsync(cancellationToken);
            try
            {
                const TransmitOption TransmitOptions = TransmitOption.Ack | TransmitOption.AutoRoute | TransmitOption.Explore;
                var function = SendDataTx.Create(destinationNodeId, commandClassBytes, TransmitOptions, false);
                var functionCall = CreateFunctionCall(function);
                var (transmitSuccess, returnValue) = await functionCall.ExecuteAsync(cancellationToken);
                return transmitSuccess && ((SendDataRx)returnValue!).Success;

            }
            finally
            {
                _functionSemaphore.Release();
            }
        }

        public async Task<SerialApiSetupResponse> SerialApiSetupAsync(bool enableStatusReport, CancellationToken cancellationToken)
        {
            await _functionSemaphore.WaitAsync(cancellationToken);
            try
            {
                var function = SerialApiSetupTx.Create(enableStatusReport);
                var functionCall = CreateFunctionCall(function);
                var (transmitSuccess, returnValue) = await functionCall.ExecuteAsync(cancellationToken);

                var response = new ReadOnlyCollection<byte>(((SerialApiSetupRx)returnValue!).Response);
                return new SerialApiSetupResponse(transmitSuccess, response);
            }
            finally
            {
                _functionSemaphore.Release();
            }


         
        }

        public async Task<bool> SetPromiscuousModeAsync(bool enabled, CancellationToken cancellationToken)
        {
            await _functionSemaphore.WaitAsync(cancellationToken);
            try
            {
                var function = SetPromiscuousModeTx.Create(enabled);
                var functionCall = CreateFunctionCall(function);
                var (transmitSuccess, _) = await functionCall.ExecuteAsync(cancellationToken);
                return transmitSuccess;
            }
            finally
            {
                _functionSemaphore.Release();
            }



        }

        private Dictionary<CommandClassType, CommandClass> CreateCommandClasses()
        {
            var commandClasses = new Dictionary<CommandClassType, CommandClass>
                                 {
                                     // Application
                                     { CommandClassType.Basic, new BasicCommandClass(_logger) },
                                     { CommandClassType.ColorSwitch, new ColorSwitchCommandClass(_logger, this) },
                                     { CommandClassType.MultilevelSensor, new MultilevelSensorCommandClass(_logger, this) },
                                     { CommandClassType.MultilevelSwitch, new MultilevelSwitchCommandClass(_logger, this) },
                                     { CommandClassType.Notification, new NotificationCommandClass(_logger) },

                                     // Management
                                     { CommandClassType.Battery, new BatteryCommandClass(_logger) },
                                     { CommandClassType.WakeUp, new WakeUpCommandClass(_logger, this) },

                                     // Transport encapsulation
                                     { CommandClassType.Crc16Encap, new Crc16EncapCommandClass(_logger, this) }
                                 };
            return commandClasses;
        }

        private FunctionCall CreateFunctionCall(IFunctionTx function)
        {
            return new FunctionCall(
                _logger,
                _port,
                function,
                Attempts,
                _networkTimeout,
                _retryDelay);
        }

        private void OnDataFrameReceived(object? sender, DataFrameEventArgs eventArgs)
        {
            ProcessDataFrame(eventArgs.DataFrame);
        }

        private void ProcessApplicationCommand(ApplicationCommandHandlerBridgeRx applicationCommand)
        {
            if (!_commandClasses.ContainsKey(applicationCommand.CommandClassType))
            {
                _logger.Error("Unsupported command class {CommandClass}", BitConverter.ToString(new[] { (byte)applicationCommand.CommandClassType }));
                return;
            }

            _commandClasses[applicationCommand.CommandClassType]
                .ProcessCommandClassBytes(applicationCommand.SourceNodeId, applicationCommand.CommandClassBytes);
        }

        private void ProcessApplicationUpdate(ApplicationUpdateRx applicationUpdate)
        {
            if (applicationUpdate.Status != ApplicationUpdateStatus.NodeInfoReceived)
            {
                _logger.Error("Unsupported application update status {Status}", applicationUpdate.Status);
            }
        }

        private void ProcessDataFrame(IDataFrame dataFrame)
        {
            if (dataFrame.Type != FrameType.Request)
            {
                return;
            }

            switch (dataFrame.FunctionType)
            {
                case FunctionType.ApplicationCommandHandlerBridge:
                    var applicationCommand = new ApplicationCommandHandlerBridgeRx(dataFrame.SerialCommandBytes);
                    Task.Run(() => ProcessApplicationCommand(applicationCommand));
                    break;
                case FunctionType.ApplicationUpdate:
                    var applicationUpdate = new ApplicationUpdateRx(dataFrame.SerialCommandBytes);
                    Task.Run(() => ProcessApplicationUpdate(applicationUpdate));
                    break;
                default:
                    _logger.Error("Unsupported function {FunctionType}", BitConverter.ToString(new[] { (byte)dataFrame.FunctionType }));
                    break;
            }
        }
    }
}