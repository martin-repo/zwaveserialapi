// -------------------------------------------------------------------------------------------------
// <copyright file="AeotecMultiSensor6.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Aeotec
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;
    using ZWaveSerialApi.CommandClasses.Application.Notification;
    using ZWaveSerialApi.CommandClasses.Management.WakeUp;

    public class AeotecMultiSensor6 : MultiSensor
    {
        public AeotecMultiSensor6(ZWaveSerialClient client, byte nodeId)
            : base(client, nodeId)
        {
            Client.GetCommandClass<NotificationCommandClass>().HomeSecurityStateChanged += (sender, eventArgs) =>
            {
                if (eventArgs.SourceNodeId == NodeId)
                {
                    switch (eventArgs.State)
                    {
                        case HomeSecurityState.Idle:
                            Idle?.Invoke(sender, new EventArgs());
                            break;
                        case HomeSecurityState.MotionDetection:
                            MotionDetected?.Invoke(sender, new EventArgs());
                            break;
                    }
                }
            };
        }

        public event EventHandler? MotionDetected;
        public event EventHandler? Idle;

        public async Task<SensorValue> GetHumidityAsync(HumidityScale scale, CancellationToken cancellationToken)
        {
            return await GetMultilevelSensorValueAsync(MultilevelSensorType.Humidity, scale, cancellationToken);
        }

        public async Task<SensorValue> GetTemperatureAsync(TemperatureScale scale, CancellationToken cancellationToken)
        {
            return await GetMultilevelSensorValueAsync(MultilevelSensorType.AirTemperature, scale, cancellationToken);
        }
    }
}