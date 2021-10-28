// -------------------------------------------------------------------------------------------------
// <copyright file="ZWavePlusInfoCommandClassTests.cs" company="Martin Karlsson">
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

    using ZWaveSerialApi.CommandClasses.Management.ZWavePlusInfo;

    public class ZWavePlusInfoCommandClassTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private ZWavePlusInfoCommandClass _zwavePlusInfoCommandClass;

        [TestCase(
            1,
            "5E-02-10-05-00-13-14-15-16",
            16,
            SlaveRoleType.AlwaysOn,
            NodeType.Node,
            "13-14",
            "15-16")]
        public void GetAsync_ShouldProcessData(
            byte destinationNodeId,
            string bytesString,
            byte expectedZWavePlusVersion,
            SlaveRoleType expectedRoleType,
            NodeType expectedNodeType,
            string expectedInstallerIconBytesString,
            string expectedUserIconBytesString)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(1));

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var reportTask = _zwavePlusInfoCommandClass.GetAsync(destinationNodeId, CancellationToken.None);
            _zwavePlusInfoCommandClass.ProcessCommandClassBytes(destinationNodeId, bytes);
            var report = reportTask.GetAwaiter().GetResult();

            Assert.That(report.ZWavePlusVersion, Is.EqualTo(expectedZWavePlusVersion));
            Assert.That(report.RoleType, Is.EqualTo(expectedRoleType));
            Assert.That(report.NodeType, Is.EqualTo(expectedNodeType));
            Assert.That(BitConverter.ToString(report.InstallerIcon), Is.EqualTo(expectedInstallerIconBytesString));
            Assert.That(BitConverter.ToString(report.UserIcon), Is.EqualTo(expectedUserIconBytesString));
        }

        [TestCase(1, "5E-01")]
        public void GetAsync_ShouldSendData(byte destinationNodeId, string expectedBytesString)
        {
            _clientMock.SetupGet(mock => mock.CallbackTimeout).Returns(TimeSpan.FromMilliseconds(1));

            var bytesString = string.Empty;
            _clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                       .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                       .Returns(Task.FromResult(true));

            try
            {
                _zwavePlusInfoCommandClass.GetAsync(destinationNodeId, CancellationToken.None).GetAwaiter().GetResult();
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

            _zwavePlusInfoCommandClass = new ZWavePlusInfoCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}