// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerSpecificTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Management
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific;

    public class ManufacturerSpecificTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private ManufacturerSpecificCommandClass _manufacturerSpecificCommandClass;

        [TestCase(1, "72-05-01-02-03-04-05-06", 258, 772, 1286)]
        public void GetAsync_ShouldProcessData(
            byte destinationNodeId,
            string bytesString,
            int expectedManufacturerId,
            int expectedProductTypeId,
            int expectedProductId)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(10));

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var reportTask = _manufacturerSpecificCommandClass.GetAsync(destinationNodeId, It.IsAny<CancellationToken>());
            _manufacturerSpecificCommandClass.ProcessCommandClassBytes(destinationNodeId, bytes);
            var report = reportTask.GetAwaiter().GetResult();

            Assert.That(report.ManufacturerId, Is.EqualTo(expectedManufacturerId));
            Assert.That(report.ProductTypeId, Is.EqualTo(expectedProductTypeId));
            Assert.That(report.ProductId, Is.EqualTo(expectedProductId));
        }

        [TestCase(1, "72-04")]
        public void GetAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(10));

            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            try
            {
                _manufacturerSpecificCommandClass.GetAsync(destinationNodeId, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (TimeoutException timeoutException) when (timeoutException.Message == "Timeout waiting for response.")
            {
            }

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

            _manufacturerSpecificCommandClass = new ManufacturerSpecificCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}