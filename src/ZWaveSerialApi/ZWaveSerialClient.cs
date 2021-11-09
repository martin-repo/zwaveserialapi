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
    using ZWaveSerialApi.CommandClasses.Application.Configuration;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch;
    using ZWaveSerialApi.CommandClasses.Application.Notification;
    using ZWaveSerialApi.CommandClasses.Management.Battery;
    using ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific;
    using ZWaveSerialApi.CommandClasses.Management.WakeUp;
    using ZWaveSerialApi.CommandClasses.Management.ZWavePlusInfo;
    using ZWaveSerialApi.CommandClasses.TransportEncapsulation.Crc16Encap;
    using ZWaveSerialApi.Functions;
    using ZWaveSerialApi.Functions.SerialApi;
    using ZWaveSerialApi.Functions.ZWave;
    using ZWaveSerialApi.Functions.ZWave.NodeInclusion;
    using ZWaveSerialApi.Functions.ZWave.NodeInclusion.AddNodeToNetwork;
    using ZWaveSerialApi.Functions.ZWave.NodeInclusion.RemoveNodeFromNetwork;
    using ZWaveSerialApi.Functions.ZWave.RequestNodeInfo;
    using ZWaveSerialApi.Functions.ZWave.SendData;
    using ZWaveSerialApi.Functions.ZWave.TypeLibrary;
    using ZWaveSerialApi.Protocol;

    public class ZWaveSerialClient : IZWaveSerialClient, IDisposable
    {
        private readonly Dictionary<CommandClassType, CommandClass> _commandClasses;
        private readonly SemaphoreSlim _functionSemaphore = new(1, 1);
        private readonly ILogger _logger;
        private readonly ZWaveSerialPort _port;

        private byte _nextCallbackFuncId = 1;

        public ZWaveSerialClient(ILogger logger, string portName)
        {
            _logger = logger.ForContext<ZWaveSerialClient>().ForContext(Constants.ClassName, GetType().Name);

            _port = new ZWaveSerialPort(logger, portName);
            _port.FrameReceived += OnFrameReceived;

            _commandClasses = CreateCommandClasses();

            DeviceNodeIds = Array.Empty<byte>();
        }

        public TimeSpan CallbackTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public byte ControllerNodeId { get; private set; }

        public byte[] DeviceNodeIds { get; private set; }

        public bool IsConnected => _port.IsConnected;

        public bool ReconnectOnFailure
        {
            get => _port.ReconnectOnFailure;
            set => _port.ReconnectOnFailure = value;
        }

        public async Task<AddNodeToNetworkResponse> AddNodeToNetworkAsync(
            byte listeningNodeCount,
            Action? controllerReadyCallback = null,
            CancellationToken cancellationToken = default,
            CancellationToken abortRequestedToken = default)
        {
            var callbackFuncId = GetNextCallbackFuncId();

            var addNodeSequence = new AddNodeSequence(
                _logger,
                listeningNodeCount,
                this,
                _port,
                callbackFuncId,
                controllerReadyCallback);
            var (success, nodeFunctionRx) = await addNodeSequence.ExecuteAsync(cancellationToken, abortRequestedToken).ConfigureAwait(false);

            var addNodeToNetworkRx = (AddNodeToNetworkRx)nodeFunctionRx;
            return new AddNodeToNetworkResponse(
                success,
                addNodeToNetworkRx.SourceNodeId,
                addNodeToNetworkRx.DeviceClass,
                addNodeToNetworkRx.CommandClasses);
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _port.ConnectAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                (ControllerNodeId, DeviceNodeIds) = await InitializeAsync(cancellationToken).ConfigureAwait(false);
                _logger.Information("Serial communication initialized.");
            }
            catch
            {
                await _port.DisconnectAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await _port.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _port.Dispose();
            GC.SuppressFinalize(this);
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

        public async Task<GetNodeProtocolInfoResponse> GetNodeProtocolInfoAsync(byte destinationNodeId, CancellationToken cancellationToken)
        {
            var result = (GetNodeProtocolInfoRx)await InvokeValueFunctionAsync(GetNodeProtocolInfoTx.Create(destinationNodeId), cancellationToken)
                                                    .ConfigureAwait(false);
            return new GetNodeProtocolInfoResponse(result.Listening);
        }

        public async Task<byte> GetSucNodeIdAsync(CancellationToken cancellationToken)
        {
            var result = (GetSucNodeIdRx)await InvokeValueFunctionAsync(GetSucNodeIdTx.Create(), cancellationToken).ConfigureAwait(false);
            return result.NodeId;
        }

        public async Task<MemoryGetIdResponse> MemoryGetIdAsync(CancellationToken cancellationToken)
        {
            var result = (MemoryGetIdRx)await InvokeValueFunctionAsync(MemoryGetIdTx.Create(), cancellationToken).ConfigureAwait(false);
            return new MemoryGetIdResponse(result.HomeId, result.NodeId);
        }

        public async Task<RemoveNodeFromNetworkResponse> RemoveNodeFromNetworkAsync(
            Action? controllerReadyCallback = null,
            CancellationToken cancellationToken = default,
            CancellationToken abortRequestedToken = default)
        {
            var callbackFuncId = GetNextCallbackFuncId();

            var removeNodeSequence = new RemoveNodeSequence(_logger, this, _port, callbackFuncId, controllerReadyCallback);

            try
            {
                var (success, nodeFunctionRx) = await removeNodeSequence.ExecuteAsync(cancellationToken, abortRequestedToken).ConfigureAwait(false);
                var removeNodeFromNetworkRx = (RemoveNodeFromNetworkRx)nodeFunctionRx;
                return new RemoveNodeFromNetworkResponse(success, removeNodeFromNetworkRx.SourceNodeId);
            }
            catch (NodeTimeoutException nodeTimeoutException) when (nodeTimeoutException.NodeTimeout == NodeTimeout.AddNodeTimeout)
            {
                return new RemoveNodeFromNetworkResponse(false, nodeTimeoutException.NodeId);
            }
        }

        public async Task<NodeInfo> RequestNodeInfoAsync(byte destinationNodeId, CancellationToken cancellationToken = default)
        {
            var requestNodeInfoTx = RequestNodeInfoTx.Create(destinationNodeId);

            static bool TryGetCallbackValue(Frame frame, out ApplicationSlaveUpdateRx? callbackValue)
            {
                if (frame.Type == FrameType.Request && frame.FunctionType == FunctionType.ApplicationUpdate)
                {
                    callbackValue = new ApplicationSlaveUpdateRx(frame.SerialCommandBytes);
                    return true;
                }

                callbackValue = default;
                return false;
            }

            var applicationSlaveUpdate = await InvokeValueFunctionWithCallbackAsync<RequestNodeInfoTx, RequestNodeInfoRx, ApplicationSlaveUpdateRx?>(
                                                 requestNodeInfoTx,
                                                 functionRx => functionRx.Success,
                                                 TryGetCallbackValue,
                                                 cancellationToken)
                                             .ConfigureAwait(false);
            if (applicationSlaveUpdate!.State != UpdateState.NodeInfoReceived)
            {
                throw new TimeoutException("No RequestNodeInfo ack received within timeout period.");
            }

            return new NodeInfo(applicationSlaveUpdate.DeviceClass, applicationSlaveUpdate.CommandClasses);
        }

        public async Task SendDataAsync(byte destinationNodeId, byte[] commandClassBytes, CancellationToken cancellationToken = default)
        {
            await SendDataAsync(destinationNodeId, commandClassBytes, SendDataTx.DefaultTransmitOptions, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendDataAsync(
            byte destinationNodeId,
            byte[] commandClassBytes,
            TransmitOption transmitOptions,
            CancellationToken cancellationToken = default)
        {
            var callbackFuncId = GetNextCallbackFuncId();

            var sendDataTx = SendDataTx.Create(destinationNodeId, commandClassBytes, transmitOptions, callbackFuncId);

            bool TryGetCallbackValue(Frame frame, out TransmitComplete callbackValue)
            {
                SendDataCallbackRx? sendDataCallbackRx;
                if (frame.Type == FrameType.Request
                    && (sendDataCallbackRx = SendDataCallbackRx.TryCreate(frame.SerialCommandBytes)) != null
                    && sendDataCallbackRx.CallbackFuncId == callbackFuncId)
                {
                    callbackValue = sendDataCallbackRx.Status;
                    return true;
                }

                callbackValue = 0;
                return false;
            }

            try
            {
                var transmitComplete = await InvokeValueFunctionWithCallbackAsync<SendDataTx, SendDataRx, TransmitComplete>(
                                               sendDataTx,
                                               functionRx => functionRx.Success,
                                               TryGetCallbackValue,
                                               cancellationToken)
                                           .ConfigureAwait(false);
                if (transmitComplete != TransmitComplete.Ok)
                {
                    throw new TransmitException($"Invalid SendData result: {transmitComplete}.");
                }
            }
            catch
            {
                _ = InvokeVoidFunctionAsync(SendDataAbortTx.Create(), CancellationToken.None);
                throw;
            }
        }

        public async Task<SerialApiGetInitDataResponse> SerialApiGetInitDataAsync(CancellationToken cancellationToken)
        {
            var result = (SerialApiGetInitDataRx)await InvokeValueFunctionAsync(SerialApiGetInitDataTx.Create(), cancellationToken)
                                                     .ConfigureAwait(false);
            return new SerialApiGetInitDataResponse(result.IsStaticUpdateController, result.DeviceNodeIds);
        }

        public async Task<byte[]> SerialApiSetupAsync(bool enableSendDataCallbackMetrics, CancellationToken cancellationToken)
        {
            var result = (SerialApiSetupRx)await InvokeValueFunctionAsync(SerialApiSetupTx.Create(enableSendDataCallbackMetrics), cancellationToken)
                                               .ConfigureAwait(false);
            return result.Response;
        }

        public async Task<LibraryType> TypeLibraryAsync(CancellationToken cancellationToken)
        {
            var result = (TypeLibraryRx)await InvokeValueFunctionAsync(TypeLibraryTx.Create(), cancellationToken).ConfigureAwait(false);
            return (LibraryType)result.LibraryType;
        }

        internal async Task InvokeVoidFunctionAsync(IFunctionTx function, CancellationToken cancellationToken)
        {
            await _functionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var frame = Frame.Create(FrameType.Request, function.FunctionArgsBytes);
                await _port.TransmitFrameAsync(frame, cancellationToken).ConfigureAwait(false);
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
                                     { CommandClassType.Basic, new BasicCommandClass(_logger, this) },
                                     { CommandClassType.ColorSwitch, new ColorSwitchCommandClass(_logger, this) },
                                     { CommandClassType.Configuration, new ConfigurationCommandClass(_logger, this) },
                                     { CommandClassType.MultilevelSensor, new MultilevelSensorCommandClass(_logger, this) },
                                     { CommandClassType.MultilevelSwitch, new MultilevelSwitchCommandClass(_logger, this) },
                                     { CommandClassType.Notification, new NotificationCommandClass(_logger, this) },

                                     // Management
                                     { CommandClassType.Battery, new BatteryCommandClass(_logger, this) },
                                     { CommandClassType.ManufacturerSpecific, new ManufacturerSpecificCommandClass(_logger, this) },
                                     { CommandClassType.WakeUp, new WakeUpCommandClass(_logger, this) },
                                     { CommandClassType.ZWavePlusInfo, new ZWavePlusInfoCommandClass(_logger, this) },

                                     // Transport encapsulation
                                     { CommandClassType.Crc16Encap, new Crc16EncapCommandClass(_logger, this) }
                                 };
            return commandClasses;
        }

        private byte GetNextCallbackFuncId()
        {
            var callbackFuncId = _nextCallbackFuncId++;
            if (_nextCallbackFuncId == 0)
            {
                _nextCallbackFuncId = 1;
            }

            return callbackFuncId;
        }

        private async Task<(byte ControllerNodeId, byte[] DeviceNodeIds)> InitializeAsync(CancellationToken cancellationToken)
        {
            var (_, controllerNodeId) = await MemoryGetIdAsync(cancellationToken).ConfigureAwait(false);

            var (_, deviceNodeIds) = await SerialApiGetInitDataAsync(cancellationToken).ConfigureAwait(false);
            deviceNodeIds = deviceNodeIds.Where(nodeId => nodeId != controllerNodeId).ToArray();

            _ = await SerialApiSetupAsync(false, cancellationToken).ConfigureAwait(false);

            return (controllerNodeId, deviceNodeIds);
        }

        private async Task<IFunctionRx> InvokeValueFunctionAsync(IFunctionTx function, CancellationToken cancellationToken)
        {
            var callbackSource = new TaskCompletionSource<IFunctionRx>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Callback(object? sender, FrameEventArgs eventArgs)
            {
                var frame = eventArgs.Frame;
                if (frame.Type != FrameType.Response || !function.IsValidReturnValue(frame.SerialCommandBytes))
                {
                    return;
                }

                var returnValue = function.CreateReturnValue(frame.SerialCommandBytes);
                callbackSource.TrySetResult(returnValue);
            }

            await _functionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _port.FrameReceived += Callback;

                var frame = Frame.Create(FrameType.Request, function.FunctionArgsBytes);
                await _port.TransmitFrameAsync(frame, cancellationToken).ConfigureAwait(false);

                if (await Task.WhenAny(callbackSource.Task, Task.Delay(CallbackTimeout, cancellationToken)).ConfigureAwait(false)
                    != callbackSource.Task)
                {
                    throw new TimeoutException("Timeout waiting for function transmit response.");
                }

                return await callbackSource.Task;
            }
            finally
            {
                _functionSemaphore.Release();
                _port.FrameReceived -= Callback;
            }
        }

        private async Task<TCallbackValue> InvokeValueFunctionWithCallbackAsync<TFunctionTx, TFunctionRx, TCallbackValue>(
            TFunctionTx function,
            Func<TFunctionRx, bool> functionSuccessfulFunc,
            TryGetCallbackValue<TCallbackValue> tryGetCallbackValue,
            CancellationToken cancellationToken)
            where TFunctionTx : IFunctionTx where TFunctionRx : IFunctionRx
        {
            var callbackSource = new TaskCompletionSource<TCallbackValue>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Callback(object? sender, FrameEventArgs eventArgs)
            {
                if (tryGetCallbackValue(eventArgs.Frame, out var callbackValue))
                {
                    callbackSource.TrySetResult(callbackValue);
                }
            }

            _port.FrameReceived += Callback;
            try
            {
                var result = await InvokeValueFunctionAsync(function, cancellationToken).ConfigureAwait(false);
                if (!functionSuccessfulFunc((TFunctionRx)result))
                {
                    throw new TransmitException("Transmit queue full.");
                }

                if (await Task.WhenAny(callbackSource.Task, Task.Delay(CallbackTimeout, cancellationToken)).ConfigureAwait(false)
                    != callbackSource.Task)
                {
                    throw new TimeoutException("Timeout waiting for callback.");
                }

                var callbackValue = await callbackSource.Task;
                return callbackValue;
            }
            finally
            {
                _port.FrameReceived -= Callback;
            }
        }

        private void OnFrameReceived(object? sender, FrameEventArgs eventArgs)
        {
            ProcessFrame(eventArgs.Frame);
        }

        private void ProcessApplicationCommand(ApplicationCommandHandlerBridgeRx applicationCommand)
        {
            if (!_commandClasses.ContainsKey(applicationCommand.CommandClassType))
            {
                _logger.Error("Unsupported command class {CommandClass}", BitConverter.ToString(new[] { (byte)applicationCommand.CommandClassType }));
                return;
            }

            var commandClass = _commandClasses[applicationCommand.CommandClassType];
            commandClass.ProcessCommandClassBytes(applicationCommand.SourceNodeId, applicationCommand.CommandClassBytes);
        }

        private void ProcessFrame(Frame frame)
        {
            if (frame.Type != FrameType.Request)
            {
                return;
            }

            if (!Enum.IsDefined(frame.FunctionType))
            {
                _logger.Error("Unsupported function {FunctionType}", BitConverter.ToString(new[] { (byte)frame.FunctionType }));
                return;
            }

            switch (frame.FunctionType)
            {
                case FunctionType.ApplicationCommandHandlerBridge:
                    var applicationCommand = new ApplicationCommandHandlerBridgeRx(frame.SerialCommandBytes);
                    _ = Task.Run(() => ProcessApplicationCommand(applicationCommand))
                            .ContinueWith(
                                failedTask => _logger.Error(failedTask.Exception, nameof(ProcessApplicationCommand)),
                                CancellationToken.None,
                                TaskContinuationOptions.OnlyOnFaulted,
                                TaskScheduler.Default);
                    break;
            }
        }

        private delegate bool TryGetCallbackValue<T>(Frame frame, out T callbackValue);
    }
}