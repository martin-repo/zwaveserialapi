// -------------------------------------------------------------------------------------------------
// <copyright file="CommandClassTestHelper.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.CommandClasses
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    public static class CommandClassTestHelper
    {
        public static void TestSendData(
            byte destinationNodeId,
            string expectedByteString,
            Mock<IZWaveSerialClient> clientMock,
            Func<byte, CancellationToken, Task> commandClassAction)
        {
            var bytesString = string.Empty;
            clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), CancellationToken.None))
                      .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                      .Returns(Task.FromResult(true));

            commandClassAction(destinationNodeId, CancellationToken.None).GetAwaiter().GetResult();

            clientMock.Verify(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), CancellationToken.None), Times.Once);

            Assert.That(bytesString, Is.EqualTo(expectedByteString));
        }
    }
}