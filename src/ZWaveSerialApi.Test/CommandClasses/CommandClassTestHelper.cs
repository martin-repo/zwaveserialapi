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
            Func<byte, CancellationToken, Task<bool>> commandClassAction)
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            var bytesString = string.Empty;
            clientMock.Setup(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), cancellationTokenSource.Token))
                      .Callback<byte, byte[], CancellationToken>((_, frameBytes, _) => bytesString = BitConverter.ToString(frameBytes))
                      .Returns(Task.FromResult(true));

            var result = commandClassAction(destinationNodeId, cancellationTokenSource.Token).Result;

            clientMock.Verify(mock => mock.SendDataAsync(destinationNodeId, It.IsAny<byte[]>(), cancellationTokenSource.Token), Times.Once);

            Assert.That(result, Is.True);
            Assert.That(bytesString, Is.EqualTo(expectedByteString));
        }
    }
}