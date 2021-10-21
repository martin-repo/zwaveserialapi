// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSensorCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.MultilevelSensor
{
    using System;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

    public class MultilevelSensorCommandClassTests
    {
        private MultilevelSensorCommandClass _multilevelSensorCommandClass;
        private Mock<IZWaveSerialClient> _serialControllerClientMock;

        [Test]
        [TestCase(1, "31-05-01-22-00-DA", 1, MultilevelSensorType.AirTemperature, 21.8)]
        public void ProcessCommandClassBytes_ShouldInvokeTemperatureEvent(
            byte sourceNodeId,
            string bytesString,
            byte expectedNodeId,
            MultilevelSensorType expectedSensorType,
            double expectedTemperature)
        {
            var eventCount = 0;
            var nodeId = 0;
            var type = (MultilevelSensorType)0;
            var temperature = 0.0;

            //_multilevelSensorCommandClass.SensorChanged += (sender, eventArgs) => { eventCount++;
            //    nodeId = eventArgs.NodeId;
            //    type = eventArgs.Type;
            //    temperature = eventArgs.Temperature;
            //};

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _multilevelSensorCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(nodeId, Is.EqualTo(expectedNodeId));
            Assert.That(type, Is.EqualTo(expectedSensorType));
            Assert.That(temperature, Is.EqualTo(expectedTemperature));
        }

        [Test]
        [TestCase(1, "31-04-01-00")]
        [TestCase(2, "31-04-01-00")]
        public void RequestValue_ShouldSend(int destinationNodeId, string expectedBytesString)
        {
            //var bytesString = string.Empty;
            //_serialControllerClientMock.Setup(mock => mock.Send(destinationNodeId, It.IsAny<byte[]>())).Callback<int, byte[]>((_, frameBytes) => bytesString = BitConverter.ToString(frameBytes));

            //_multilevelSensorCommandClass.RequestValue(destinationNodeId, MultilevelSensorType.AirTemperature, TemperatureScale.Celcius);

            //_serialControllerClientMock.Verify(mock => mock.Send(destinationNodeId, It.IsAny<byte[]>()), Times.Once);

            //Assert.That(bytesString, Is.EqualTo(expectedBytesString));
        }

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger>();
            _serialControllerClientMock = new Mock<IZWaveSerialClient>();
            _multilevelSensorCommandClass = new MultilevelSensorCommandClass(loggerMock.Object, _serialControllerClientMock.Object);
        }
    }
}