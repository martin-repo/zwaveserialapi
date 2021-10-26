// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveDevices.cs" company="Martin Karlsson">
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
    using ZWaveSerialApi.Devices.Brands;
    using ZWaveSerialApi.Devices.Brands.Aeotec;
    using ZWaveSerialApi.Devices.Brands.Fibaro;
    using ZWaveSerialApi.Devices.Settings;
    using ZWaveSerialApi.Devices.Utilities;

    public class ZWaveDevices : IDisposable
    {
        private readonly TimeSpan _addDeviceTimeout = TimeSpan.FromSeconds(15);
        private readonly TimeSpan _requestFromEventTimeout = TimeSpan.FromSeconds(5);
        private readonly ZWaveSerialClient _client;
        private readonly List<Device> _devices = new();
        private readonly ILogger _logger;

        private readonly Dictionary<ManufacturerSpecific, DeviceType> _manufacturerDeviceTypes = Enum.GetValues(typeof(DeviceType))
            .Cast<DeviceType>()
            .ToDictionary(deviceType => AttributeHelper.GetManufacturerSpecific(deviceType), deviceType => deviceType);

        private readonly WakeUpCommandClass _wakeUp;

        private Network _network = new();

        public ZWaveDevices(ILogger logger, string portName)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);

            _client = new ZWaveSerialClient(logger, portName);

            _wakeUp = _client.GetCommandClass<WakeUpCommandClass>();
        }

        public ZWaveDevices(string portName)
            : this(new LoggerConfiguration().CreateLogger(), portName)
        {
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _client.ConnectAsync(cancellationToken);
            await InitializeAsync(cancellationToken);

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

        public T GetDevice<T>(byte nodeId)
            where T : Device
        {
            return _devices.OfType<T>().First(device => device.NodeId == nodeId);
        }

        public T GetDevice<T>(string location)
            where T : Device
        {
            var devices = _devices.OfType<T>().Where(device => device.Location == location).ToList();
            if (devices.Count > 1)
            {
                throw new InvalidOperationException("Multiple devices of the same type assigned to the same location.");
            }

            return devices.Single();
        }

        public IEnumerable<T> GetDevices<T>()
            where T : Device
        {
            return _devices.OfType<T>();
        }

        public async Task LoadAsync(string settingsFilePath, CancellationToken cancellationToken = default)
        {
            await using var fileStream = File.OpenRead(settingsFilePath);
            var network = await JsonSerializer.DeserializeAsync<Network>(fileStream, new JsonSerializerOptions(), cancellationToken);
            _network = network ?? throw new InvalidDataException("File did not contain valid settings json.");
        }

        public async Task SaveAsync(string settingsFilePath, CancellationToken cancellationToken = default)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            await using FileStream fileStream = File.Create(settingsFilePath);
            await JsonSerializer.SerializeAsync(fileStream, _network, options, cancellationToken);
        }

        private async Task AddDeviceToNetworkIfMissing(byte deviceNodeId, CancellationToken cancellationToken)
        {
            if (_network.Devices.ContainsKey(deviceNodeId))
            {
                return;
            }

            var deviceNodeIdHex = ByteToHexString(deviceNodeId);

            try
            {
                var networkDevice = new NetworkDevice();

                var nodeInfo = await _client.RequestNodeInfoAsync(deviceNodeId, cancellationToken);
                if (nodeInfo == null)
                {
                    _logger.Information(
                        "Node {NodeId} did not respond and will not be available until it wakes up.",
                        deviceNodeIdHex,
                        nameof(CommandClassType.ManufacturerSpecific));
                    return;
                }

                if (!nodeInfo.CommandClasses.Contains((byte)CommandClassType.ManufacturerSpecific))
                {
                    _logger.Warning(
                        "Node {NodeId} does not support {CommandClass} and will not be available.",
                        deviceNodeIdHex,
                        nameof(CommandClassType.ManufacturerSpecific));
                    return;
                }

                var manufacturerSpecificCommandClass = _client.GetCommandClass<ManufacturerSpecificCommandClass>();
                var report = await manufacturerSpecificCommandClass.GetAsync(deviceNodeId, cancellationToken);
                var deviceManufacturerSpecific = new ManufacturerSpecific(report.ManufacturerId, report.ProductTypeId, report.ProductId);
                if (!_manufacturerDeviceTypes.ContainsKey(deviceManufacturerSpecific))
                {
                    _logger.Warning(
                        "Node {NodeId} {ManufacturerCode} is not supported and will not be available.",
                        deviceNodeIdHex,
                        deviceManufacturerSpecific);
                    return;
                }

                networkDevice.Type = _manufacturerDeviceTypes[deviceManufacturerSpecific];
                networkDevice.Name = AttributeHelper.GetManufacturerName(networkDevice.Type);

                var nodeProtocolInfo = await _client.GetNodeProtocolInfo(deviceNodeId, cancellationToken);
                networkDevice.IsListening = nodeProtocolInfo.Listening;

                if (nodeInfo.CommandClasses.Contains((byte)CommandClassType.ZWavePlusInfo))
                {
                    var zwavePlusReport = await _client.GetCommandClass<ZWavePlusInfoCommandClass>().GetAsync(deviceNodeId, cancellationToken);
                    networkDevice.IsAlwaysOn = zwavePlusReport.RoleType == SlaveRoleType.AlwaysOn;
                }

                _network.Devices[deviceNodeId] = networkDevice;
                _devices.Add(CreateDevice(deviceNodeId, networkDevice));

                _logger.Information("{DeviceName} ({DeviceNodeId}) was added.", networkDevice.Name, deviceNodeIdHex);
            }
            catch (TimeoutException)
            {
                _logger.Warning(
                    "Timeout while adding {NodeId}. It will not be available.",
                    deviceNodeIdHex,
                    nameof(CommandClassType.ManufacturerSpecific));
            }
            catch (TransmitException)
            {
                _logger.Warning(
                    "Trasmission error while adding {NodeId}. It will not be available.",
                    deviceNodeIdHex,
                    nameof(CommandClassType.ManufacturerSpecific));
            }
        }

        private string ByteToHexString(byte @byte)
        {
            return "0x" + BitConverter.ToString(new[] { @byte });
        }

        private Device CreateDevice(byte deviceNodeId, NetworkDevice networkDevice)
        {
            switch (networkDevice.Type)
            {
                case DeviceType.AeotecAerqSensor:
                    return new AeotecAerqSensor(_client, deviceNodeId, networkDevice);
                case DeviceType.AeotecLedBulb6MultiColor:
                    return new AeotecLedBulb6MultiColor(_client, deviceNodeId, networkDevice);
                case DeviceType.AeotecMultiSensor6:
                    return new AeotecMultiSensor6(_client, deviceNodeId, networkDevice);
                case DeviceType.FibaroMotionSensor:
                    return new FibaroMotionSensor(_client, deviceNodeId, networkDevice);
                default:
                    throw new ArgumentOutOfRangeException($"DeviceType {networkDevice.Type} is not connected to any implementation.");
            }
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            foreach (var deviceNodeId in _network.Devices.Keys.ToList())
            {
                if (!_client.DeviceNodeIds.Contains(deviceNodeId))
                {
                    _network.Devices.Remove(deviceNodeId);
                }
                else
                {
                    _devices.Add(CreateDevice(deviceNodeId, _network.Devices[deviceNodeId]));

                    _logger.Information(
                        "{DeviceName} ({DeviceNodeId}) was added.",
                        _network.Devices[deviceNodeId].Name,
                        ByteToHexString(deviceNodeId));
                }
            }

            foreach (var deviceNodeId in _client.DeviceNodeIds)
            {
                await AddDeviceToNetworkIfMissing(deviceNodeId, cancellationToken);
            }
        }

        public event EventHandler<DeviceEventArgs>? DeviceWakeUp;

        private async void OnWakeUpNotification(object? sender, WakeUpNotificationEventArgs eventArgs)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(_addDeviceTimeout))
            {
                await AddDeviceToNetworkIfMissing(eventArgs.SourceNodeId, cancellationTokenSource.Token);
            }

            var device = _devices.FirstOrDefault(device => device.NodeId == eventArgs.SourceNodeId);

            _logger.Debug("Device {NodeIdHex} wake up ({DeviceName})", ByteToHexString(eventArgs.SourceNodeId), device != null ? device.Name : "Unknown");

            if (device != null)
            {
                DeviceWakeUp?.Invoke(this, new DeviceEventArgs(device));
            }

            using (var cancellationTokenSource = new CancellationTokenSource(_requestFromEventTimeout))
            {
                await _wakeUp.NoMoreInformationAsync(eventArgs.SourceNodeId, cancellationTokenSource.Token);
            }
        }
    }
}