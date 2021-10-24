// -------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerSpecificTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses.Management
{
    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses.Management.ManufacturerSpecific;

    public class ManufacturerSpecificTests
    {
        private Mock<IZWaveSerialClient> _clientMock;

        private ManufacturerSpecificCommandClass _manufacturerSpecificCommandClass;

        [SetUp]
        public void Setup()
        {
            _clientMock = new Mock<IZWaveSerialClient>();
            _clientMock.SetupGet(mock => mock.NodeId).Returns(1);

            var loggerMock = new Mock<ILogger>();
            _manufacturerSpecificCommandClass = new ManufacturerSpecificCommandClass(loggerMock.Object, _clientMock.Object);
        }
    }
}