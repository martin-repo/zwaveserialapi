// -------------------------------------------------------------------------------------------------
// <copyright file="ColorSwitchCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Application;
    using ZWaveSerialApi.CommandClasses.Application.ColorSwitch;

    public class ColorSwitchCommandClassTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private ColorSwitchCommandClass _colorSwitchCommandClass;

        [TestCase(
            1,
            50,
            100,
            150,
            true,
            "33-05-03-02-32-03-64-04-96-FF")]
        [TestCase(
            1,
            150,
            100,
            50,
            false,
            "33-05-03-02-96-03-64-04-32-00")]
        public void SetAsync_ShouldSendData(
            byte destinationNodeId,
            byte red,
            byte green,
            byte blue,
            bool defaultDuration,
            string expectedBytesString)
        {
            var colorComponents = new List<ColorComponent>
                                  {
                                      new(ColorComponentType.Red, red), new(ColorComponentType.Green, green), new(ColorComponentType.Blue, blue)
                                  };
            var duration = defaultDuration ? DurationType.Default : DurationType.Instant;

            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            _colorSwitchCommandClass.SetAsync(destinationNodeId, colorComponents, duration, CancellationToken.None).GetAwaiter().GetResult();

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

            _colorSwitchCommandClass = new ColorSwitchCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}