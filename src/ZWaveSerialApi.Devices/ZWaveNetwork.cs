// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveNetwork.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.CommandClasses;
    using ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific;
    using ZWaveSerialApi.CommandClasses.Management.WakeUp;
    using ZWaveSerialApi.CommandClasses.Management.ZWavePlusInfo;
    using ZWaveSerialApi.Devices.Device;

    public class ZWaveNetwork : IDisposable
    {
        private readonly TimeSpan _addDeviceTimeout = TimeSpan.FromSeconds(15);
        private readonly ZWaveSerialClient _client;
        private readonly CustomDeviceFactory _customDeviceFactory;
        private readonly DeviceFactory _deviceFactory;
        private readonly List<IDevice> _devices = new();
        private readonly ILogger _logger;
        private readonly Dictionary<byte, DeviceState> _nodeIdDeviceStates = new();
        private readonly TimeSpan _requestFromEventTimeout = TimeSpan.FromSeconds(5);
        private readonly Dictionary<byte, DeviceType> _unsupportedDeviceTypes = new();
        private readonly WakeUpCommandClass _wakeUp;

        public ZWaveNetwork(ILogger logger, string portName)
        {
            _logger = logger.ForContext<ZWaveNetwork>().ForContext(Constants.ClassName, GetType().Name);

            _client = new ZWaveSerialClient(logger, portName);

            _customDeviceFactory = new CustomDeviceFactory(_client);
            _deviceFactory = new DeviceFactory(_client);

            _wakeUp = _client.GetCommandClass<WakeUpCommandClass>();
        }

        public ZWaveNetwork(string portName)
            : this(new LoggerConfiguration().CreateLogger(), portName)
        {
        }

        public async Task<AddDeviceResult> AddDeviceAsync(
            Action? controllerReadyCallback = null,
            Func<WakeUpDevice, Task>? wakeUpInitializationFunc = null,
            CancellationToken cancellationToken = default)
        {
            var listeningNodesCount = (byte)_devices.Count(device => device.IsListening);

            _client.ReconnectOnFailure = false;
            try
            {
                var result = await _client.AddNodeToNetworkAsync(listeningNodesCount, controllerReadyCallback, cancellationToken)
                                          .ConfigureAwait(false);
                if (!result.Success)
                {
                    // Node added, but with errors
                    _logger.Debug("Node was added but with errors.");
                }

                var (added, deviceState) = await AddDeviceStateIfMissingAsync(result.NodeId, cancellationToken).ConfigureAwait(false);
                if (!added || deviceState == null)
                {
                    return new AddDeviceResult(false, null);
                }

                // BASIC_TYPE_* @ ZW_classcmd.h
                const byte BasicTypeRoutingSlave = 0x04;
                if (deviceState.DeviceClass.Basic == BasicTypeRoutingSlave)
                {
                    // TODO:
                    // DeleteSucReturnRoute 01 05 00 55 15 08 B2
                    // AssignSucReturnRoute 01 06 00 51 15 09 09 BD
                    // Association CC 01 0B 00 13 15 04 85 01 01 01 25 0B 5C
                }

                var device = CreateDevice(deviceState);
                if (device is WakeUpDevice wakeUpDevice)
                {
                    wakeUpDevice.IsAwake = true;
                    try
                    {
                        if (wakeUpInitializationFunc != null)
                        {
                            await wakeUpInitializationFunc(wakeUpDevice).ConfigureAwait(false);
                        }

                        await _wakeUp.NoMoreInformationAsync(wakeUpDevice.NodeId, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        wakeUpDevice.IsAwake = false;
                    }
                }

                return new AddDeviceResult(true, device);
            }
            finally
            {
                _client.ReconnectOnFailure = true;
            }
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _client.ConnectAsync(cancellationToken).ConfigureAwait(false);
            await InitializeAsync(cancellationToken).ConfigureAwait(false);

            _logger.Information("Network initialized.");

            _wakeUp.Notification += OnWakeUpNotification;
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            _wakeUp.Notification -= OnWakeUpNotification;

            await _client.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public T GetDevice<T>(string location)
            where T : IDevice
        {
            var devices = _devices.OfType<T>().Where(device => device.Location == location).ToList();
            if (devices.Count > 1)
            {
                throw new InvalidOperationException("Multiple devices of the same type assigned to the same location.");
            }

            return devices.Single();
        }

        public IEnumerable<T> GetDevices<T>()
            where T : IDevice
        {
            return _devices.OfType<T>();
        }

        public IEnumerable<IDevice> GetDevices()
        {
            return _devices.ToList();
        }

        public IEnumerable<DeviceType> GetUnsupportedDeviceTypes()
        {
            return _unsupportedDeviceTypes.Values.Distinct()
                                          .OrderBy(deviceType => deviceType.ManufacturerId)
                                          .ThenBy(deviceType => deviceType.ProductTypeId)
                                          .ThenBy(deviceType => deviceType.ProductId);
        }

        public async Task LoadAsync(string settingsFilePath, CancellationToken cancellationToken = default)
        {
            if (_client.IsConnected)
            {
                throw new InvalidOperationException("Network state must be loaded before connecting.");
            }

            _nodeIdDeviceStates.Clear();

            await using var fileStream = File.OpenRead(settingsFilePath);

            var settings = await JsonSerializer.DeserializeAsync<NetworkSettings>(fileStream, new JsonSerializerOptions(), cancellationToken)
                                               .ConfigureAwait(false);
            if (settings == null)
            {
                throw new InvalidDataException("File did not contain valid settings json.");
            }

            foreach (var deviceState in settings.DeviceStates)
            {
                _nodeIdDeviceStates.Add(deviceState.NodeId, deviceState);
            }
        }

        public void RegisterCustomDeviceType(DeviceType deviceType, DeviceConstructionDelegate deviceConstructionDelegate)
        {
            _customDeviceFactory.RegisterDeviceType(deviceType, deviceConstructionDelegate);

            if (_deviceFactory.CanCreate(deviceType))
            {
                _logger.Warning("Supported device registered as custom type. {DeviceType}", deviceType);
            }
        }

        public async Task RemoveDeviceAsync(Action? controllerReadyCallback = null, CancellationToken cancellationToken = default)
        {
            var result = await _client.RemoveNodeFromNetworkAsync(controllerReadyCallback, cancellationToken).ConfigureAwait(false);
            if (!result.Success)
            {
                // TODO: Check if result.NodeId is still in list, and if so call this method again
                _logger.Debug("Errors during removal. Please check if node was removed.");
            }

            if (_nodeIdDeviceStates.ContainsKey(result.NodeId))
            {
                _nodeIdDeviceStates.Remove(result.NodeId);
            }

            if (_unsupportedDeviceTypes.ContainsKey(result.NodeId))
            {
                _unsupportedDeviceTypes.Remove(result.NodeId);
            }

            var device = _devices.FirstOrDefault(device => device.NodeId == result.NodeId);
            if (device != null)
            {
                _devices.Remove(device);
            }
        }

        public async Task SaveAsync(string settingsFilePath, CancellationToken cancellationToken = default)
        {
            var settings = new NetworkSettings { DeviceStates = _nodeIdDeviceStates.Values.ToList() };

            var options = new JsonSerializerOptions { WriteIndented = true };

            await using FileStream fileStream = File.Create(settingsFilePath);

            await JsonSerializer.SerializeAsync(fileStream, settings, options, cancellationToken).ConfigureAwait(false);
        }

        private static string ByteToHexString(byte @byte)
        {
            return "0x" + BitConverter.ToString(new[] { @byte });
        }

        private async Task<(bool Added, DeviceState? DeviceState)> AddDeviceStateIfMissingAsync(
            byte deviceNodeId,
            CancellationToken cancellationToken)
        {
            if (_nodeIdDeviceStates.ContainsKey(deviceNodeId) || _unsupportedDeviceTypes.ContainsKey(deviceNodeId))
            {
                return (false, null);
            }

            var (success, deviceState) = await TryGetDeviceStateAsync(deviceNodeId, cancellationToken).ConfigureAwait(false);
            if (!success || deviceState == null)
            {
                return (false, null);
            }

            if (!_customDeviceFactory.CanCreate(deviceState.DeviceType) && !_deviceFactory.CanCreate(deviceState.DeviceType))
            {
                _logger.Warning("Node {NodeId} device type ({DeviceType}) is not supported.", ByteToHexString(deviceNodeId), deviceState.DeviceType);
                _unsupportedDeviceTypes.Add(deviceState.NodeId, deviceState.DeviceType);
                return (false, null);
            }

            _nodeIdDeviceStates.Add(deviceState.NodeId, deviceState);
            return (true, deviceState);
        }

        private IDevice CreateDevice(DeviceState deviceState)
        {
            var device = _customDeviceFactory.CanCreate(deviceState.DeviceType)
                             ? _customDeviceFactory.Create(deviceState)
                             : _deviceFactory.Create(deviceState);

            _devices.Add(device);
            if (!string.IsNullOrEmpty(device.Location))
            {
                _logger.Debug(
                    "Device {DeviceNodeId} - {DeviceName} @ {Location} added.",
                    ByteToHexString(device.NodeId),
                    device.Name,
                    device.Location);
            }
            else
            {
                _logger.Debug("Device {DeviceNodeId} - {DeviceName} added.", ByteToHexString(device.NodeId), device.Name);
            }

            return device;
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            PruneInvalidDeviceStates();

            foreach (var deviceNodeId in _client.DeviceNodeIds)
            {
                _ = await AddDeviceStateIfMissingAsync(deviceNodeId, cancellationToken).ConfigureAwait(false);
            }

            _devices.Clear();
            foreach (var deviceState in _nodeIdDeviceStates.Values)
            {
                _ = CreateDevice(deviceState);
            }
        }

        private async Task OnDeviceWakeUpAsync(byte nodeId)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource(_addDeviceTimeout);
                var (added, deviceState) = await AddDeviceStateIfMissingAsync(nodeId, cancellationTokenSource.Token).ConfigureAwait(false);
                if (added)
                {
                    _ = CreateDevice(deviceState!);
                }
            }
            catch (TransmitException transmitException)
            {
                _logger.Debug(transmitException, nameof(TransmitException));
            }
            catch (TimeoutException timeoutException)
            {
                _logger.Debug(timeoutException, nameof(TransmitException));
            }
            catch (OperationCanceledException)
            {
            }

            var device = _devices.FirstOrDefault(device => device.NodeId == nodeId);

            _logger.Debug("Device {DeviceNodeId} - {DeviceName} woke up.", ByteToHexString(nodeId), device != null ? device.Name : "Unknown");

            if (device is WakeUpDevice wakeUpDevice)
            {
                wakeUpDevice.IsAwake = true;
                try
                {
                    await wakeUpDevice.OnWakeUpNotificationAsync();
                }
                finally
                {
                    wakeUpDevice.IsAwake = false;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);

            try
            {
                var cancellationTokenSource = new CancellationTokenSource(_requestFromEventTimeout);
                await _wakeUp.NoMoreInformationAsync(nodeId, cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TransmitException transmitException)
            {
                _logger.Debug(transmitException, nameof(TransmitException));
            }
            catch (TimeoutException timeoutException)
            {
                _logger.Debug(timeoutException, nameof(TransmitException));
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnWakeUpNotification(object? sender, WakeUpNotificationEventArgs eventArgs)
        {
            _ = Task.Run(async () => await OnDeviceWakeUpAsync(eventArgs.SourceNodeId))
                    .ContinueWith(
                        failedTask => _logger.Error(failedTask.Exception, nameof(OnDeviceWakeUpAsync)),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnFaulted,
                        TaskScheduler.Default);
        }

        private void PruneInvalidDeviceStates()
        {
            foreach (var deviceState in _nodeIdDeviceStates.Values.OrderBy(deviceState => deviceState.NodeId).ToList())
            {
                if (!_client.DeviceNodeIds.Contains(deviceState.NodeId))
                {
                    _logger.Debug("Saved device state has no matching node.");
                    _nodeIdDeviceStates.Remove(deviceState.NodeId);
                    continue;
                }

                if (!_customDeviceFactory.CanCreate(deviceState.DeviceType) && !_deviceFactory.CanCreate(deviceState.DeviceType))
                {
                    _logger.Debug(
                        "Saved node {NodeId} device type ({DeviceType}) is not supported.",
                        ByteToHexString(deviceState.NodeId),
                        deviceState.DeviceType);
                    _nodeIdDeviceStates.Remove(deviceState.NodeId);
                }
            }
        }

        private async Task<(bool Success, DeviceState? DeviceState)> TryGetDeviceStateAsync(byte deviceNodeId, CancellationToken cancellationToken)
        {
            try
            {
                var nodeInfo = await _client.RequestNodeInfoAsync(deviceNodeId, cancellationToken).ConfigureAwait(false);
                if (!nodeInfo.CommandClasses.Contains((byte)CommandClassType.ManufacturerSpecific))
                {
                    _logger.Warning(
                        "Node {NodeId} does not support {CommandClass} and will not be available.",
                        ByteToHexString(deviceNodeId),
                        nameof(CommandClassType.ManufacturerSpecific));
                    return (false, null);
                }

                var manufacturerSpecificCommandClass = _client.GetCommandClass<ManufacturerSpecificCommandClass>();
                var report = await manufacturerSpecificCommandClass.GetAsync(deviceNodeId, cancellationToken).ConfigureAwait(false);
                var deviceType = new DeviceType(report.ManufacturerId, report.ProductTypeId, report.ProductId);

                var nodeProtocolInfo = await _client.GetNodeProtocolInfoAsync(deviceNodeId, cancellationToken).ConfigureAwait(false);
                var isListening = nodeProtocolInfo.Listening;

                var isAlwaysOn = false;
                if (nodeInfo.CommandClasses.Contains((byte)CommandClassType.ZWavePlusInfo))
                {
                    var zwavePlusReport = await _client.GetCommandClass<ZWavePlusInfoCommandClass>()
                                                       .GetAsync(deviceNodeId, cancellationToken)
                                                       .ConfigureAwait(false);
                    isAlwaysOn = zwavePlusReport.RoleType == SlaveRoleType.AlwaysOn;
                }

                var deviceState = new DeviceState
                                  {
                                      DeviceClass = nodeInfo.DeviceClass,
                                      DeviceType = deviceType,
                                      IsAlwaysOn = isAlwaysOn,
                                      IsListening = isListening,
                                      NodeId = deviceNodeId,
                                      CommandClasses = nodeInfo.CommandClasses
                                  };
                return (true, deviceState);
            }
            catch (TimeoutException)
            {
                _logger.Debug("Timeout while querying node {NodeId}. It will not be available until it wakes up.", ByteToHexString(deviceNodeId));
                return (false, null);
            }
            catch (TransmitException)
            {
                _logger.Information(
                    "Transmission error while querying node {NodeId}. It will not be available until network reconnect.",
                    ByteToHexString(deviceNodeId));
                return (false, null);
            }
        }
    }
}