// -------------------------------------------------------------------------------------------------
// <copyright file="NotificationCommandClassTests.cs" company="Martin Karlsson">
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

    using ZWaveSerialApi.CommandClasses.Application.Notification;

    public class NotificationCommandClassTests
    {
        private NotificationCommandClass _notificationCommandClass;

        [TestCase(1, "71-05-00-00-00-FF-07-00-00", HomeSecurityState.Idle, "")]
        [TestCase(1, "71-05-00-00-00-FF-07-03-00", HomeSecurityState.CoverTampering, "")]
        [TestCase(1, "71-05-00-00-00-FF-07-08-00", HomeSecurityState.MotionDetection, "")]
        [TestCase(1, "71-05-00-00-00-FF-07-00-01-08", HomeSecurityState.Idle, "08")]
        [TestCase(1, "71-05-00-00-00-FF-07-00-02-08-09", HomeSecurityState.Idle, "08-09")]
        [TestCase(1, "71-05-00-00-00-FF-07-00-80-01", HomeSecurityState.Idle, "")]
        [TestCase(1, "71-05-00-00-00-FF-07-00-81-08-01", HomeSecurityState.Idle, "08")]
        public void ProcessCommandClassBytes_ShouldInvokeHomeSecurityStateChangedEvent(
            byte sourceNodeId,
            string bytesString,
            HomeSecurityState expectedState,
            string expectedBytesString)
        {
            var eventInvoked = false;

            _notificationCommandClass.HomeSecurityStateChanged += (_, eventArgs) =>
            {
                eventInvoked = true;
                Assert.That(eventArgs.State, Is.EqualTo(expectedState));
                Assert.That(BitConverter.ToString(eventArgs.Parameters), Is.EqualTo(expectedBytesString));
            };

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _notificationCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventInvoked, Is.True);
        }

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IZWaveSerialClient>();
            _notificationCommandClass = new NotificationCommandClass(loggerMock.Object, clientMock.Object);
        }
    }
}