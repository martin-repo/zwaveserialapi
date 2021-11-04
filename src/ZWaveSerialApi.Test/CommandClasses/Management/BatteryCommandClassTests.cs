// -------------------------------------------------------------------------------------------------
// <copyright file="BatteryCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Management
{
    using System;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Management.Battery;

    public class BatteryCommandClassTests
    {
        private BatteryCommandClass _batteryCommandClass;

        [TestCase(1, "80-03-50", false, 80)]
        [TestCase(1, "80-03-FF", true, 0)]
        public void ProcessCommandClassBytes_ShouldInvokeReportEvent(
            byte sourceNodeId,
            string bytesString,
            bool expectedIsLow,
            byte expectedPercentage)
        {
            var eventInvoked = false;

            _batteryCommandClass.Report += (_, eventArgs) =>
            {
                eventInvoked = true;
                Assert.That(eventArgs.Percentage, Is.EqualTo(expectedPercentage));
                Assert.That(eventArgs.IsLow, Is.EqualTo(expectedIsLow));
            };

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _batteryCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventInvoked, Is.True);
        }

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(mock => mock.ForContext<It.IsAnyType>()).Returns(loggerMock.Object);
            loggerMock.Setup(mock => mock.ForContext(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(loggerMock.Object);

            var clientMock = new Mock<IZWaveSerialClient>();

            _batteryCommandClass = new BatteryCommandClass(loggerMock.Object, clientMock.Object);
        }
    }
}