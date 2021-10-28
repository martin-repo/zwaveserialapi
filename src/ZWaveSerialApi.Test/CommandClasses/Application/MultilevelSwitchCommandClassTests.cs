// -------------------------------------------------------------------------------------------------
// <copyright file="MultilevelSwitchCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Application;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch;

    public class MultilevelSwitchCommandClassTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private MultilevelSwitchCommandClass _multilevelSwitchCommandClass;

        [TestCase(1, 10, true, "26-01-0A-FF")]
        [TestCase(1, 80, true, "26-01-50-FF")]
        [TestCase(1, 255, true, "26-01-FF-FF")]
        [TestCase(1, 10, false, "26-01-0A-00")]
        public void SetAsync_ShouldSendData(
            byte destinationNodeId,
            byte level,
            bool defaultDuration,
            string expectedBytesString)
        {
            var duration = defaultDuration ? DurationType.Default : DurationType.Instant;

            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            _multilevelSwitchCommandClass.SetAsync(destinationNodeId, level, duration, CancellationToken.None).GetAwaiter().GetResult();

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

            _multilevelSwitchCommandClass = new MultilevelSwitchCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}