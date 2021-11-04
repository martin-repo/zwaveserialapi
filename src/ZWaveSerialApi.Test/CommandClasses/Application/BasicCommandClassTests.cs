// -------------------------------------------------------------------------------------------------
// <copyright file="BasicCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Application
{
    using System;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Application.Basic;

    public class BasicCommandClassTests
    {
        private BasicCommandClass _basicCommandClass;

        [TestCase(1, "20-03-10", 16)]
        public void ProcessCommandClassBytes_ShouldInvokeReportEvent(byte sourceNodeId, string bytesString, byte expectedValue)
        {
            var eventInvoked = false;

            _basicCommandClass.Report += (_, eventArgs) =>
            {
                eventInvoked = true;
                Assert.That(eventArgs.Value, Is.EqualTo(expectedValue));
            };

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _basicCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventInvoked, Is.True);
        }

        [TestCase(1, "20-01-10", 16)]
        public void ProcessCommandClassBytes_ShouldInvokeSetEvent(byte sourceNodeId, string bytesString, byte expectedValue)
        {
            var eventInvoked = false;

            _basicCommandClass.Set += (_, eventArgs) =>
            {
                eventInvoked = true;
                Assert.That(eventArgs.Value, Is.EqualTo(expectedValue));
            };

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _basicCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventInvoked, Is.True);
        }

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(mock => mock.ForContext<It.IsAnyType>()).Returns(loggerMock.Object);
            loggerMock.Setup(mock => mock.ForContext(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(loggerMock.Object);

            var clientMock = new Mock<IZWaveSerialClient>();

            _basicCommandClass = new BasicCommandClass(loggerMock.Object, clientMock.Object);
        }
    }
}