// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveSerialClientTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test
{
    using System;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.CommandClasses;

    public class ZWaveSerialClientTests
    {
        [Test]
        public void Test_ThatCommandClassTypesExist()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(mock => mock.ForContext<It.IsAnyType>()).Returns(loggerMock.Object);
            loggerMock.Setup(mock => mock.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>())).Returns(loggerMock.Object);

            var client = new ZWaveSerialClient(loggerMock.Object, "PORT");

            foreach (var commandClassType in Enum.GetValues<CommandClassType>())
            {
                // When this fails, add corresponding creation to ZWaveSerialClient.CreateCommandClasses(...)
                Assert.DoesNotThrow(() => _ = client.GetCommandClass(commandClassType));
            }
        }
    }
}