// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpCommandClassTests.cs" company="Martin Karlsson">
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

    using ZWaveSerialApi.CommandClasses.Management.WakeUp;
    using ZWaveSerialApi.Functions.ZWave.SendData;

    public class WakeUpCommandClassTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private WakeUpCommandClass _wakeUpCommandClass;

        [TestCase(
            1,
            "84-0A-00-00-F0-00-0E-10-00-0E-10-00-00-3C",
            240,
            3600,
            3600,
            60)]
        public void IntervalCapabilitiesGetAsync_ShouldProcessData(
            byte destinationNodeId,
            string bytesString,
            int expectedMinimumIntervalSeconds,
            int expectedMaximumIntervalSeconds,
            int expectedDefaultIntervalSeconds,
            int expectedIntervalStepSeconds)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(10));

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var reportTask = _wakeUpCommandClass.IntervalCapabilitiesGetAsync(destinationNodeId, CancellationToken.None);
            _wakeUpCommandClass.ProcessCommandClassBytes(destinationNodeId, bytes);
            var report = reportTask.GetAwaiter().GetResult();

            Assert.That(report.MinimumInterval.TotalSeconds, Is.EqualTo(expectedMinimumIntervalSeconds));
            Assert.That(report.MaximumInterval.TotalSeconds, Is.EqualTo(expectedMaximumIntervalSeconds));
            Assert.That(report.DefaultInterval.TotalSeconds, Is.EqualTo(expectedDefaultIntervalSeconds));
            Assert.That(report.IntervalStep.TotalSeconds, Is.EqualTo(expectedIntervalStepSeconds));
        }

        [TestCase(1, "84-09")]
        public void IntervalCapabilitiesGetAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(10));

            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            try
            {
                _wakeUpCommandClass.IntervalCapabilitiesGetAsync(destinationNodeId, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (TimeoutException timeoutException) when (timeoutException.Message == "Timeout waiting for response.")
            {
            }

            _clientMock.Verify(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(bytesString, Is.EqualTo(expectedBytesString));
        }

        [TestCase(1, "84-06-00-0E-10-01", 3600)]
        public void IntervalGetAsync_ShouldProcessData(byte destinationNodeId, string bytesString, int expectedIntervalSeconds)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(10));

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var intervalTask = _wakeUpCommandClass.IntervalGetAsync(destinationNodeId, CancellationToken.None);
            _wakeUpCommandClass.ProcessCommandClassBytes(destinationNodeId, bytes);
            var interval = intervalTask.GetAwaiter().GetResult();

            Assert.That(interval.TotalSeconds, Is.EqualTo(expectedIntervalSeconds));
        }

        [TestCase(1, "84-05")]
        public void IntervalGetAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(10));

            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            try
            {
                _wakeUpCommandClass.IntervalGetAsync(destinationNodeId, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (TimeoutException timeoutException) when (timeoutException.Message == "Timeout waiting for response.")
            {
            }

            _clientMock.Verify(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(bytesString, Is.EqualTo(expectedBytesString));
        }

        [TestCase(1, 3600, "84-04-00-0E-10-01")]
        [TestCase(2, 3600, "84-04-00-0E-10-01")]
        public void IntervalSetAsync_ShouldSendData(byte destinationNodeId, int intervalSeconds, string expectedBytesString)
        {
            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            _wakeUpCommandClass.IntervalSetAsync(destinationNodeId, TimeSpan.FromSeconds(intervalSeconds), CancellationToken.None)
                               .GetAwaiter()
                               .GetResult();

            _clientMock.Verify(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(bytesString, Is.EqualTo(expectedBytesString));
        }

        [TestCase(1, "84-08")]
        public void NoMoreInformationAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), TransmitOption.Ack, It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], TransmitOption, CancellationToken>(
                           (
                               _,
                               frameBytes,
                               _,
                               _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            _wakeUpCommandClass.NoMoreInformationAsync(destinationNodeId, CancellationToken.None).GetAwaiter().GetResult();

            _clientMock.Verify(
                mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), TransmitOption.Ack, It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.That(bytesString, Is.EqualTo(expectedBytesString));
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
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(mock => mock.ForContext<It.IsAnyType>()).Returns(loggerMock.Object);

            _clientMock = new Mock<IZWaveSerialClient>();
            _clientMock.SetupGet(mock => mock.ControllerNodeId).Returns(1);

            _wakeUpCommandClass = new WakeUpCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}