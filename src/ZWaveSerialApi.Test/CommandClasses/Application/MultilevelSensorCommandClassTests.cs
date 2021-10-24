//// -------------------------------------------------------------------------------------------------
//// <copyright file="MultilevelSensorCommandClassTests.cs" company="Martin Karlsson">
////   Copyright (c) Martin Karlsson. All rights reserved.
//// </copyright>
//// -------------------------------------------------------------------------------------------------

//namespace ZWaveSerialApi.Test.CommandClasses.Application
//{
//    using System;
//    using System.Linq;

//    using Moq;

//    using NUnit.Framework;

//    using Serilog;

//    using ZWaveSerialApi.CommandClasses.Application.MultilevelSensor;

//    public class MultilevelSensorCommandClassTests
//    {
//        private Mock<IZWaveSerialClient> _clientMock;

//        private MultilevelSensorCommandClass _multilevelSensorCommandClass;

//        [TestCase(1, MultilevelSensorType.AirTemperature, TemperatureScale.Celcius, "31-04-01-00")]
//        [TestCase(1, MultilevelSensorType.AirTemperature, TemperatureScale.Fahrenheit, "31-04-01-08")]
//        [TestCase(1, MultilevelSensorType.DewPoint, TemperatureScale.Celcius, "31-04-0B-00")]
//        [TestCase(1, MultilevelSensorType.DewPoint, TemperatureScale.Fahrenheit, "31-04-0B-08")]
//        [TestCase(1, MultilevelSensorType.Humidity, HumidityScale.Percentage, "31-04-05-00")]
//        [TestCase(1, MultilevelSensorType.Illuminance, IlluminanceScale.Percentage, "31-04-03-00")]
//        [TestCase(1, MultilevelSensorType.Illuminance, IlluminanceScale.Lux, "31-04-03-08")]
//        [TestCase(1, MultilevelSensorType.Ultraviolet, UltravioletScale.UvIndex, "31-04-1B-00")]
//        public void GetAsync_ShouldSendData(
//            byte destinationNodeId,
//            MultilevelSensorType type,
//            Enum scale,
//            string expectedBytesString)
//        {
//            CommandClassTestHelper.TestSendData(
//                destinationNodeId,
//                expectedBytesString,
//                _clientMock,
//                (nodeId, cancellationToken) => _multilevelSensorCommandClass.GetAsync(nodeId, type, scale, cancellationToken));
//        }

//        [TestCase(
//            1,
//            "31-05-01-42-04-D2",
//            MultilevelSensorType.AirTemperature,
//            12.34,
//            "C",
//            "Celcius",
//            TemperatureScale.Celcius)]
//        [TestCase(
//            1,
//            "31-05-01-62-04-D2",
//            MultilevelSensorType.AirTemperature,
//            1.234,
//            "C",
//            "Celcius",
//            TemperatureScale.Celcius)]
//        [TestCase(
//            1,
//            "31-05-01-4A-04-D2",
//            MultilevelSensorType.AirTemperature,
//            12.34,
//            "F",
//            "Fahrenheit",
//            TemperatureScale.Fahrenheit)]
//        [TestCase(
//            1,
//            "31-05-0B-42-04-D2",
//            MultilevelSensorType.DewPoint,
//            12.34,
//            "C",
//            "Celcius",
//            TemperatureScale.Celcius)]
//        [TestCase(
//            1,
//            "31-05-0B-4A-04-D2",
//            MultilevelSensorType.DewPoint,
//            12.34,
//            "F",
//            "Fahrenheit",
//            TemperatureScale.Fahrenheit)]
//        [TestCase(
//            1,
//            "31-05-05-42-04-D2",
//            MultilevelSensorType.Humidity,
//            12.34,
//            "%",
//            "Relative humidity",
//            HumidityScale.Percentage)]
//        [TestCase(
//            1,
//            "31-05-03-42-04-D2",
//            MultilevelSensorType.Illuminance,
//            12.34,
//            "%",
//            "",
//            IlluminanceScale.Percentage)]
//        [TestCase(
//            1,
//            "31-05-03-4A-04-D2",
//            MultilevelSensorType.Illuminance,
//            12.34,
//            "Lux",
//            "",
//            IlluminanceScale.Lux)]
//        [TestCase(
//            1,
//            "31-05-1B-42-04-D2",
//            MultilevelSensorType.Ultraviolet,
//            12.34,
//            "UV index",
//            "",
//            UltravioletScale.UvIndex)]
//        public void ProcessCommandClassBytes_ShouldInvokeReportEvent(
//            byte sourceNodeId,
//            string bytesString,
//            MultilevelSensorType expectedSensorType,
//            double expectedValue,
//            string expectedUnit,
//            string expectedLabel,
//            Enum expectedScale)
//        {
//            var eventInvoked = false;

//            _multilevelSensorCommandClass.Report += (_, eventArgs) =>
//            {
//                eventInvoked = true;
//                Assert.That(eventArgs.SensorType, Is.EqualTo(expectedSensorType));
//                Assert.That(eventArgs.Value, Is.EqualTo(expectedValue));
//                Assert.That(eventArgs.Unit, Is.EqualTo(expectedUnit));
//                Assert.That(eventArgs.Label, Is.EqualTo(expectedLabel));
//                Assert.That(eventArgs.Scale, Is.EqualTo(expectedScale));
//            };

//            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
//            _multilevelSensorCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

//            Assert.That(eventInvoked, Is.True);
//        }

//        [SetUp]
//        public void Setup()
//        {
//            _clientMock = new Mock<IZWaveSerialClient>();
//            _clientMock.SetupGet(mock => mock.NodeId).Returns(1);

//            var loggerMock = new Mock<ILogger>();
//            _multilevelSensorCommandClass = new MultilevelSensorCommandClass(loggerMock.Object, _clientMock.Object);
//        }
//    }
//}