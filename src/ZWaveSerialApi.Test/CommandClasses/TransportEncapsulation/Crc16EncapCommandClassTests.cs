// -------------------------------------------------------------------------------------------------
// <copyright file="Crc16EncapCommandClassTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.TransportEncapsulation
{
    using System;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses;
    using ZWaveSerialApi.CommandClasses.TransportEncapsulation.Crc16Encap;

    public class Crc16EncapCommandClassTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private Crc16EncapCommandClass _crc16EncapCommandClass;

        [TestCase(1, "56-01-20-02-4D-26", CommandClassType.Basic)]
        [TestCase(1, "56-01-84-07-CC-39", CommandClassType.WakeUp)]
        public void ProcessCommandClassBytes_ShouldCallProcessCommandClassBytes(
            byte sourceNodeId,
            string bytesString,
            CommandClassType expectedCommandClassType)
        {
            var unitTestCommandClass = new UnitTestCommandClass(_clientMock.Object);
            _clientMock.Setup(mock => mock.GetCommandClass(expectedCommandClassType)).Returns(unitTestCommandClass);

            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            _crc16EncapCommandClass.ProcessCommandClassBytes(sourceNodeId, bytes);

            Assert.That(unitTestCommandClass.SourceNodeId, Is.EqualTo(sourceNodeId));
            Assert.That(unitTestCommandClass.CommandClassBytes, Is.EqualTo(bytes[2..^2]));
        }

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(mock => mock.ForContext<It.IsAnyType>()).Returns(loggerMock.Object);
            loggerMock.Setup(mock => mock.ForContext(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(loggerMock.Object);

            _clientMock = new Mock<IZWaveSerialClient>();
            _clientMock.SetupGet(mock => mock.ControllerNodeId).Returns(1);

            _crc16EncapCommandClass = new Crc16EncapCommandClass(loggerMock.Object, _clientMock.Object);
        }

        private class UnitTestCommandClass : CommandClass
        {
            public UnitTestCommandClass(IZWaveSerialClient client)
                : base(CommandClassType.Basic, client)
            {
            }

            public byte[] CommandClassBytes { get; private set; }

            public byte SourceNodeId { get; private set; }

            internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
            {
                SourceNodeId = sourceNodeId;
                CommandClassBytes = commandClassBytes;
            }
        }
    }
}