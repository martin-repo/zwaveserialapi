// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpCommandClassTests.cs" company="Martin Karlsson">
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

    using ZWaveSerialApi.CommandClasses.Management.WakeUp;

    public class WakeUpCommandClassTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private WakeUpCommandClass _wakeUpCommandClass;

        [TestCase(1, "84-09")]
        public void IntervalCapabilitiesGetAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            CommandClassTestHelper.TestSendData(
                destinationNodeId,
                expectedBytesString,
                _clientMock,
                (nodeId, cancellationToken) => _wakeUpCommandClass.IntervalCapabilitiesGetAsync(nodeId, cancellationToken));
        }

        [TestCase(1, "84-05")]
        public void IntervalGetAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            CommandClassTestHelper.TestSendData(
                destinationNodeId,
                expectedBytesString,
                _clientMock,
                (nodeId, cancellationToken) => _wakeUpCommandClass.IntervalGetAsync(nodeId, cancellationToken));
        }

        [TestCase(1, 3600, "84-04-00-0E-10-01")]
        [TestCase(2, 3600, "84-04-00-0E-10-02")]
        public void IntervalSetAsync_ShouldSendData(byte destinationNodeId, int intervalSeconds, string expectedBytesString)
        {
            CommandClassTestHelper.TestSendData(
                destinationNodeId,
                expectedBytesString,
                _clientMock,
                (nodeId, cancellationToken) =>
                    _wakeUpCommandClass.IntervalSetAsync(nodeId, TimeSpan.FromSeconds(intervalSeconds), cancellationToken));
        }

        [TestCase(1, "84-08")]
        public void NoMoreInformationAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            CommandClassTestHelper.TestSendData(
                destinationNodeId,
                expectedBytesString,
                _clientMock,
                (nodeId, cancellationToken) => _wakeUpCommandClass.NoMoreInformationAsync(nodeId, cancellationToken));
        }

        [TestCase(
            1,
            "84-0A-00-00-F0-00-0E-10-00-0E-10-00-00-3C",
            240,
            3600,
            3600,
            60)]
        [TestCase(
            2,
            "84-0A-00-00-F0-00-0E-10-00-0E-10-00-00-3C",
            240,
            3600,
            3600,
            60)]
        public void ProcessCommandClassBytes_ShouldInvokeIntervalCapabilitiesReportEvent(
            byte sourceNodeId,
            string bytesString,
            int expectedMinimumIntervalSeconds,
            int expectedMaximumIntervalSeconds,
            int expectedDefaultIntervalSeconds,
            int expectedIntervalStepSeconds)
        {
            var eventInvoked = false;

            _wakeUpCommandClass.IntervalCapabilitiesReport += (_, eventArgs) =>
            {
                eventInvoked = true;
                Assert.That(eventArgs.MinimumInterval.TotalSeconds, Is.EqualTo(expectedMinimumIntervalSeconds));
                Assert.That(eventArgs.MaximumInterval.TotalSeconds, Is.EqualTo(expectedMaximumIntervalSeconds));
                Assert.That(eventArgs.DefaultInterval.TotalSeconds, Is.EqualTo(expectedDefaultIntervalSeconds));
                Assert.That(eventArgs.IntervalStep.TotalSeconds, Is.EqualTo(expectedIntervalStepSeconds));
            };

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _wakeUpCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventInvoked, Is.True);
        }

        [TestCase(1, "84-06-00-01-2C-01", 300)]
        [TestCase(2, "84-06-00-01-2C-01", 300)]
        public void ProcessCommandClassBytes_ShouldInvokeIntervalReportEvent(byte sourceNodeId, string bytesString, int expectedIntervalSeconds)
        {
            var eventInvoked = false;

            _wakeUpCommandClass.IntervalReport += (_, eventArgs) =>
            {
                eventInvoked = true;
                Assert.That(eventArgs.Interval.TotalSeconds, Is.EqualTo(expectedIntervalSeconds));
            };

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _wakeUpCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventInvoked, Is.True);
        }

        [TestCase(1, "84-07")]
        [TestCase(2, "84-07")]
        public void ProcessCommandClassBytes_ShouldInvokeNotificationEvent(byte sourceNodeId, string bytesString)
        {
            var eventInvoked = false;

            _wakeUpCommandClass.Notification += (_, _) =>
            {
                eventInvoked = true;
            };

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _wakeUpCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(eventInvoked, Is.True);
        }

        [SetUp]
        public void Setup()
        {
            _clientMock = new Mock<IZWaveSerialClient>();
            _clientMock.SetupGet(mock => mock.NodeId).Returns(1);

            var loggerMock = new Mock<ILogger>();
            _wakeUpCommandClass = new WakeUpCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}