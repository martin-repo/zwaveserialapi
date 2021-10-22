// -------------------------------------------------------------------------------------------------
// <copyright file="ColorSwitchCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Application
{
    using System.Collections.Generic;

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
        public void IntervalCapabilitiesGetAsync_ShouldSendData(
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

            CommandClassTestHelper.TestSendData(
                destinationNodeId,
                expectedBytesString,
                _clientMock,
                (nodeId, cancellationToken) => _colorSwitchCommandClass.SetAsync(nodeId, colorComponents, duration, cancellationToken));
        }

        [SetUp]
        public void Setup()
        {
            _clientMock = new Mock<IZWaveSerialClient>();
            _clientMock.SetupGet(mock => mock.NodeId).Returns(1);

            var loggerMock = new Mock<ILogger>();
            _colorSwitchCommandClass = new ColorSwitchCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}