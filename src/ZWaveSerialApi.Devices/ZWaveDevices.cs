// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveDevices.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific;
    using ZWaveSerialApi.Devices.Brands;
    using ZWaveSerialApi.Devices.Brands.Aeotec;
    using ZWaveSerialApi.Devices.Utilities;
    using ZWaveSerialApi.Functions.ZWave.SendData;

    public class ZWaveDevices : IDisposable
    {
        private readonly ZWaveSerialClient _client;

        private readonly List<Device> _devices = new();

        // TODO: Monitor WakeUpReport for devices that doesn't reply to ManufacturerSpecificGet, and call again when they wake up
        // TODO: Save network info class to disk, json
        // TODO: User-defined "location" property
        // TODO: Reset() method to clear network info class
        public ZWaveDevices(string portName, ILogger logger)
        {
            _client = new ZWaveSerialClient(portName, logger) { Timeout = TimeSpan.FromSeconds(1) };

            InitializeAsync(CancellationToken.None).Wait();
        }

        public ZWaveDevices(string portName)
            : this(portName, new LoggerConfiguration().CreateLogger())
        {
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
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetDevices<T>()
            where T : Device
        {
            return _devices.OfType<T>();
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            var manufacturerSpecificCommandClass = _client.GetCommandClass<ManufacturerSpecificCommandClass>();

            var manufacturerDeviceTypes = Enum.GetValues(typeof(DeviceType))
                                              .Cast<DeviceType>()
                                              .ToDictionary(
                                                  deviceType => AttributeHelper.GetManufacturerSpecific(deviceType),
                                                  deviceType => deviceType);

            foreach (var deviceNodeId in _client.DeviceNodeIds)
            {
                if (deviceNodeId == _client.NodeId)
                {
                    continue;
                }

                try
                {
                    var report = await manufacturerSpecificCommandClass.GetAsync(deviceNodeId, cancellationToken);
                    var deviceManufacturerSpecific = new ManufacturerSpecific(report.ManufacturerId, report.ProductTypeId, report.ProductId);
                    var deviceType = manufacturerDeviceTypes[deviceManufacturerSpecific];
                    var deviceName = AttributeHelper.GetManufacturerName(deviceType);
                    switch (deviceType)
                    {
                        case DeviceType.AeotecLedBulb6MultiColor:
                            var a = new AeotecLedBulb6MultiColor(_client, deviceNodeId);
                            _devices.Add(a);
                            break;
                        case DeviceType.AeotecMultiSensor6:
                            var b = new AeotecMultiSensor6(_client, deviceNodeId);
                            _devices.Add(b);
                            break;
                    }
                }
                catch (TimeoutException timeoutException)
                {
                }
                catch (TransmitException transmitException)
                {
                }
            }

            // TODO: Connect with Device enum into network info class
        }
    }
}