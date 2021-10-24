// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveSerialClient.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi
{
    using System;
    using System.Collections.Generic;
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
    using ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific;
    using ZWaveSerialApi.CommandClasses.Management.WakeUp;
    using ZWaveSerialApi.CommandClasses.TransportEncapsulation.Crc16Encap;
    using ZWaveSerialApi.Frames;
    using ZWaveSerialApi.Functions;
    using ZWaveSerialApi.Functions.SerialApi;
    using ZWaveSerialApi.Functions.ZWave;
    using ZWaveSerialApi.Functions.ZWave.SendData;
    using ZWaveSerialApi.Functions.ZWave.TypeLibrary;

    // TODO: Read/write device configuration parameters
    // TODO: Inclusion/exclusion of devices
    public class ZWaveSerialClient : IZWaveSerialClient, IDisposable
    {
        private readonly Dictionary<CommandClassType, CommandClass> _commandClasses;
        private readonly SemaphoreSlim _functionSemaphore = new(1, 1);
        private readonly ILogger _logger;
        private readonly ZWaveSerialPort _port;

        public ZWaveSerialClient(string portName, ILogger logger)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);

            _port = new ZWaveSerialPort(portName, logger);
            _port.DataFrameReceived += OnDataFrameReceived;

            _commandClasses = CreateCommandClasses();

            (NodeId, DeviceNodeIds) = InitializeAsync(CancellationToken.None).Result;
        }

        public ZWaveSerialClient(string portName) : this(portName, new LoggerConfiguration().CreateLogger())
        {
        }

        public byte[] DeviceNodeIds { get; }

        public byte NodeId { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        public void Dispose()
        {
            _port.Dispose();
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

        public async Task<byte> GetSucNodeIdAsync(CancellationToken cancellationToken)
        {
            var result = (GetSucNodeIdRx)await InvokeFunction(GetSucNodeIdTx.Create(), cancellationToken);
            return result.NodeId;
        }

        public async Task SendDataAsync(byte destinationNodeId, byte[] commandClassBytes, CancellationToken cancellationToken)
        {
            const TransmitOption TransmitOptions = TransmitOption.Ack | TransmitOption.AutoRoute | TransmitOption.Explore;

            var callbackFuncId = SendDataTx.GetNextCompletedFuncId();

            var callbackSource = new TaskCompletionSource<TransmitComplete>();

            void Callback(object? sender, DataFrameEventArgs eventArgs)
            {
                var dataFrame = eventArgs.DataFrame;
                if (dataFrame.Type != FrameType.Request
                    || dataFrame.FunctionType != FunctionType.SendData
                    || dataFrame.SerialCommandBytes[1] != callbackFuncId)
                {
                    return;
                }

                var complete = (TransmitComplete)dataFrame.SerialCommandBytes[2];
                callbackSource.TrySetResult(complete);
            }

            _port.DataFrameReceived += Callback;
            try
            {
                var executeTask = InvokeFunction(
                    SendDataTx.Create(destinationNodeId, commandClassBytes, TransmitOptions, callbackFuncId),
                    cancellationToken);
                if (await Task.WhenAny(executeTask, Task.Delay(Timeout, cancellationToken)) != executeTask)
                {
                    throw new TimeoutException("Timeout waiting for SendData to transmit.");
                }

                var result = (SendDataRx)executeTask.GetAwaiter().GetResult();
                if (!result.Success)
                {
                    throw new TransmitException("Serial port did not respond.");
                }

                if (await Task.WhenAny(callbackSource.Task, Task.Delay(Timeout, cancellationToken)) != callbackSource.Task)
                {
                    throw new TimeoutException("Timeout waiting for SendData callback.");
                }

                var complete = callbackSource.Task.GetAwaiter().GetResult();
                if (complete != TransmitComplete.Ok)
                {
                    throw new TransmitException($"SendData error {complete}.");
                }
            }
            finally
            {
                _port.DataFrameReceived -= Callback;
            }
        }

        public async Task<SerialApiGetInitDataResponse> SerialApiGetInitDataAsync(CancellationToken cancellationToken)
        {
            var result = (SerialApiGetInitDataRx)await InvokeFunction(SerialApiGetInitDataTx.Create(), cancellationToken);
            return new SerialApiGetInitDataResponse(result.IsStaticUpdateController, result.DeviceNodeIds);
        }

        /// <summary>
        /// Enable/disable transmission metrics appended to SendData callback.
        /// </summary>
        /// <remarks>
        /// INS12350-Serial-API-Host-Appl.-Prg.-Guide.pdf
        /// 4.1.4 Version 6
        /// </remarks>
        public async Task<byte[]> SerialApiSetupAsync(bool enableStatusReport, CancellationToken cancellationToken)
        {
            var result = (SerialApiSetupRx)await InvokeFunction(SerialApiSetupTx.Create(enableStatusReport), cancellationToken);
            return result.Response;
        }

        public async Task<LibraryType> TypeLibraryAsync(CancellationToken cancellationToken)
        {
            var result = (TypeLibraryRx)await InvokeFunction(TypeLibraryTx.Create(), cancellationToken);
            return (LibraryType)result.LibraryType;
        }

        private Dictionary<CommandClassType, CommandClass> CreateCommandClasses()
        {
            var commandClasses = new Dictionary<CommandClassType, CommandClass>
                                 {
                                     // Application
                                     { CommandClassType.Basic, new BasicCommandClass(_logger, this) },
                                     { CommandClassType.ColorSwitch, new ColorSwitchCommandClass(_logger, this) },
                                     { CommandClassType.MultilevelSensor, new MultilevelSensorCommandClass(_logger, this) },
                                     { CommandClassType.MultilevelSwitch, new MultilevelSwitchCommandClass(_logger, this) },
                                     { CommandClassType.Notification, new NotificationCommandClass(_logger, this) },

                                     // Management
                                     { CommandClassType.Battery, new BatteryCommandClass(_logger, this) },
                                     { CommandClassType.ManufacturerSpecific, new ManufacturerSpecificCommandClass(_logger, this) },
                                     { CommandClassType.WakeUp, new WakeUpCommandClass(_logger, this) },

                                     // Transport encapsulation
                                     { CommandClassType.Crc16Encap, new Crc16EncapCommandClass(_logger, this) }
                                 };
            return commandClasses;
        }

        private async Task<(byte NodeId, byte[] DeviceNodeIds)> InitializeAsync(CancellationToken cancellationToken)
        {
            var libraryType = await TypeLibraryAsync(cancellationToken);
            if (libraryType != LibraryType.BridgeController)
            {
                throw new NotSupportedException("Only bridge controllers are supported.");
            }

            var (isStaticUpdateController, deviceNodeIds) = await SerialApiGetInitDataAsync(cancellationToken);
            if (!isStaticUpdateController)
            {
                throw new NotSupportedException("Only static update controllers are supported.");
            }

            var nodeId = await GetSucNodeIdAsync(cancellationToken);

            _ = await SerialApiSetupAsync(false, cancellationToken);

            return (nodeId, deviceNodeIds);
        }

        private async Task<IFunctionRx> InvokeFunction(FunctionTx function, CancellationToken cancellationToken)
        {
            var callbackSource = new TaskCompletionSource<IFunctionRx>();

            void Callback(object? sender, DataFrameEventArgs eventArgs)
            {
                var dataFrame = eventArgs.DataFrame;
                if (dataFrame.Type != FrameType.Response || !function.IsValidReturnValue(dataFrame.SerialCommandBytes))
                {
                    return;
                }

                var returnValue = function.CreateReturnValue(dataFrame.SerialCommandBytes);
                callbackSource.TrySetResult(returnValue);
            }

            await _functionSemaphore.WaitAsync(cancellationToken);
            try
            {
                _port.DataFrameReceived += Callback;

                var dataFrame = DataFrame.Create(FrameType.Request, function.FunctionArgsBytes);
                await _port.WriteDataFrameAsync(dataFrame, cancellationToken);

                if (await Task.WhenAny(callbackSource.Task, Task.Delay(Timeout, cancellationToken)) != callbackSource.Task)
                {
                    throw new TimeoutException("Timeout waiting for function transmit response.");
                }

                return callbackSource.Task.Result;
            }
            finally
            {
                _functionSemaphore.Release();
                _port.DataFrameReceived -= Callback;
            }
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