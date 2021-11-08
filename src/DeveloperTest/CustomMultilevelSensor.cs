// -------------------------------------------------------------------------------------------------
// <copyright file="CustomMultilevelSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace DeveloperTest
{
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.Devices.Device;

    public class CustomMultilevelSensor : IDevice
    {
        private readonly DeviceState _deviceState;
        private readonly MultilevelSensorCommandClass _multilevelSensor;

        internal CustomMultilevelSensor(IZWaveSerialClient client, DeviceState deviceState)
        {
            _deviceState = deviceState;

            _multilevelSensor = client.GetCommandClass<MultilevelSensorCommandClass>();
        }

        public bool IsAlwaysOn => _deviceState.IsAlwaysOn;

        public bool IsListening => _deviceState.IsListening;

        public string Location { get; set; } = string.Empty;

        public string Name => "Custom MultilevelSensor";

        public byte NodeId => _deviceState.NodeId;

        public async Task<MultilevelSensorReport> GetTemperatureAsync(TemperatureScale scale, CancellationToken cancellationToken = default)
        {
            return await _multilevelSensor.GetAsync(NodeId, MultilevelSensorType.AirTemperature, scale, cancellationToken);
        }
    }
}