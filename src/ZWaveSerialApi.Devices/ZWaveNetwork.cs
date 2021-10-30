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

        public event EventHandler<DeviceEventArgs>? DeviceWakeUp;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _client.ConnectAsync(cancellationToken);
            await InitializeAsync(cancellationToken);

            _logger.Debug("Network initialized.");

            _wakeUp.Notification += OnWakeUpNotification;
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            _wakeUp.Notification -= OnWakeUpNotification;

            await _client.DisconnectAsync(cancellationToken);
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

            var settings = await JsonSerializer.DeserializeAsync<NetworkSettings>(fileStream, new JsonSerializerOptions(), cancellationToken);
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

        public async Task SaveAsync(string settingsFilePath, CancellationToken cancellationToken = default)
        {
            var settings = new NetworkSettings { DeviceStates = _nodeIdDeviceStates.Values.ToList() };

            var options = new JsonSerializerOptions { WriteIndented = true };

            await using FileStream fileStream = File.Create(settingsFilePath);

            await JsonSerializer.SerializeAsync(fileStream, settings, options, cancellationToken);
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

            var (success, deviceState) = await TryGetDeviceStateAsync(deviceNodeId, cancellationToken);
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
                _ = await AddDeviceStateIfMissingAsync(deviceNodeId, cancellationToken);
            }

            _devices.Clear();
            foreach (var deviceState in _nodeIdDeviceStates.Values)
            {
                _ = CreateDevice(deviceState);
            }
        }

        private async void OnWakeUpNotification(object? sender, WakeUpNotificationEventArgs eventArgs)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(_addDeviceTimeout))
            {
                var (added, deviceState) = await AddDeviceStateIfMissingAsync(eventArgs.SourceNodeId, cancellationTokenSource.Token);
                if (added)
                {
                    _ = CreateDevice(deviceState!);
                }
            }

            var device = _devices.FirstOrDefault(device => device.NodeId == eventArgs.SourceNodeId);

            _logger.Debug(
                "Device {DeviceNodeId} - {DeviceName} woke up.",
                ByteToHexString(eventArgs.SourceNodeId),
                device != null ? device.Name : "Unknown");

            if (device != null)
            {
                DeviceWakeUp?.Invoke(this, new DeviceEventArgs(device));
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            using (var cancellationTokenSource = new CancellationTokenSource(_requestFromEventTimeout))
            {
                await _wakeUp.NoMoreInformationAsync(eventArgs.SourceNodeId, cancellationTokenSource.Token);
            }
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
                var nodeInfo = await _client.RequestNodeInfoAsync(deviceNodeId, cancellationToken);
                if (!nodeInfo.CommandClasses.Contains((byte)CommandClassType.ManufacturerSpecific))
                {
                    _logger.Warning(
                        "Node {NodeId} does not support {CommandClass} and will not be available.",
                        ByteToHexString(deviceNodeId),
                        nameof(CommandClassType.ManufacturerSpecific));
                    return (false, null);
                }

                var manufacturerSpecificCommandClass = _client.GetCommandClass<ManufacturerSpecificCommandClass>();
                var report = await manufacturerSpecificCommandClass.GetAsync(deviceNodeId, cancellationToken);
                var deviceType = new DeviceType(report.ManufacturerId, report.ProductTypeId, report.ProductId);

                var nodeProtocolInfo = await _client.GetNodeProtocolInfo(deviceNodeId, cancellationToken);
                var isListening = nodeProtocolInfo.Listening;

                var isAlwaysOn = false;
                if (nodeInfo.CommandClasses.Contains((byte)CommandClassType.ZWavePlusInfo))
                {
                    var zwavePlusReport = await _client.GetCommandClass<ZWavePlusInfoCommandClass>().GetAsync(deviceNodeId, cancellationToken);
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