// -------------------------------------------------------------------------------------------------
// <copyright file="MultiSensor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

    public abstract class MultiSensor : Sensor
    {
        protected MultiSensor(ZWaveSerialClient client, byte nodeId)
            : base(client, nodeId)
        {
        }

        protected Task<SensorValue> GetMultilevelSensorValueAsync(MultilevelSensorType sensorType, Enum scale, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<SensorValue>();

            var multilevelSensor = Client.GetCommandClass<MultilevelSensorCommandClass>();

            void SensorChangedHandler(object? sender, MultilevelSensorEventArgs eventArgs)
            {
                if (eventArgs.SourceNodeId != NodeId || eventArgs.SensorType != sensorType)
                {
                    return;
                }

                var sensorValue = new SensorValue(
                    eventArgs.SourceNodeId,
                    eventArgs.SensorType,
                    eventArgs.Value,
                    eventArgs.Unit,
                    eventArgs.Label,
                    eventArgs.Scale);
                completionSource.SetResult(sensorValue);

                multilevelSensor.Report -= SensorChangedHandler;
            }

            multilevelSensor.Report += SensorChangedHandler;
            _ = multilevelSensor.GetAsync(NodeId, sensorType, scale, cancellationToken);

            return completionSource.Task;
        }
    }
}