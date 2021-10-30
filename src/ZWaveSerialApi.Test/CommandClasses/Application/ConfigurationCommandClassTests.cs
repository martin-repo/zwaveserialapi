// -------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Application
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Application.Configuration;

    public class ConfigurationCommandClassTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private ConfigurationCommandClass _configurationCommandClass;

        [TestCase(1, 1, "70-06-01-01-01", 1, "01")]
        [TestCase(2, 1, "70-06-01-01-01", 1, "01")]
        [TestCase(1, 2, "70-06-02-01-01", 2, "01")]
        [TestCase(1, 1, "70-06-01-02-01-01", 1, "01-01")]
        [TestCase(1, 1, "70-06-01-03-AA-BB-CC", 1, "AA-BB-CC")]
        public void GetAsync_ShouldProcessData(
            byte destinationNodeId,
            byte parameterNumber,
            string bytesString,
            double expectedParameterNumber,
            string expectedValueBytesString)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(1));

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var reportTask = _configurationCommandClass.GetAsync(destinationNodeId, parameterNumber, CancellationToken.None);
            _configurationCommandClass.ProcessCommandClassBytes(destinationNodeId, bytes);
            var report = reportTask.GetAwaiter().GetResult();

            Assert.That(report.ParameterNumber, Is.EqualTo(expectedParameterNumber));
            Assert.That(BitConverter.ToString(report.Value), Is.EqualTo(expectedValueBytesString));
        }

        [TestCase(1, 1, "70-05-01")]
        [TestCase(2, 1, "70-05-01")]
        [TestCase(1, 2, "70-05-02")]
        public void GetAsync_ShouldSendData(byte destinationNodeId, byte parameterNumber, string expectedBytesString)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(1));

            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            try
            {
                _configurationCommandClass.GetAsync(destinationNodeId, parameterNumber, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (TimeoutException timeoutException) when (timeoutException.Message == "Timeout waiting for response.")
            {
            }

            _clientMock.Verify(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(bytesString, Is.EqualTo(expectedBytesString));
        }

        [TestCase(1, 1, false, "01", "70-04-01-01-01")]
        [TestCase(2, 1, false, "01", "70-04-01-01-01")]
        [TestCase(1, 2, false, "01", "70-04-02-01-01")]
        [TestCase(1, 1, true, "01", "70-04-01-81-01")]
        [TestCase(1, 1, false, "AA-BB-CC", "70-04-01-03-AA-BB-CC")]
        public void SetAsync_ShouldSendData(
            byte destinationNodeId,
            byte parameterNumber,
            bool @default,
            string valueBytesString,
            string expectedBytesString)
        {
            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            var value = valueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _configurationCommandClass.SetAsync(destinationNodeId, parameterNumber, @default, value, CancellationToken.None).GetAwaiter().GetResult();

            _clientMock.Verify(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(bytesString, Is.EqualTo(expectedBytesString));
        }

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(mock => mock.ForContext<It.IsAnyType>()).Returns(loggerMock.Object);

            _clientMock = new Mock<IZWaveSerialClient>();
            _clientMock.SetupGet(mock => mock.ControllerNodeId).Returns(1);

            _configurationCommandClass = new ConfigurationCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}