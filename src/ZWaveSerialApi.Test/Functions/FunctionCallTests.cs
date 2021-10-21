// -------------------------------------------------------------------------------------------------
// <copyright file="FunctionCallTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using Serilog;

    using ZWaveSerialApi.Frames;
    using ZWaveSerialApi.Functions;

    public class FunctionCallTests
    {
        private CancellationTokenSource _cancellationTokenSource;

        private Mock<ILogger> _loggerMock;
        private Mock<IZWaveSerialPort> _portMock;

        [Test]
        public async Task ExecuteAsync_WhenNoReturnValue()
        {
            _portMock.Setup(mock => mock.WriteDataFrameAsync(It.IsAny<DataFrame>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var functionMock = new Mock<IFunctionTx>();
            functionMock.SetupGet(mock => mock.HasReturnValue).Returns(false);
            functionMock.SetupGet(mock => mock.FunctionArgsBytes).Returns(new byte[] { 0x00 });

            var functionCall = new FunctionCall(
                _loggerMock.Object,
                _portMock.Object,
                functionMock.Object,
                1,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);

            _cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var task = functionCall.ExecuteAsync(_cancellationTokenSource.Token);

            await Task.Delay(1);
            _portMock.Raise(mock => mock.ControlFrameReceived += null, new ControlFrameEventArgs(FramePreamble.Ack));

            var (transmitSuccess, returnValue) = await task;

            Assert.That(transmitSuccess, Is.True);
            Assert.That(returnValue, Is.Null);
        }

        [Test]
        public async Task ExecuteAsync_WhenReturnValue()
        {
            _portMock.Setup(mock => mock.WriteDataFrameAsync(It.IsAny<DataFrame>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var resultMock = new Mock<IFunctionRx>();

            var functionMock = new Mock<IFunctionTx>();
            functionMock.SetupGet(mock => mock.HasReturnValue).Returns(true);
            functionMock.SetupGet(mock => mock.FunctionArgsBytes).Returns(new byte[] { 0x00 });
            functionMock.Setup(mock => mock.IsValidReturnValue(It.IsAny<byte[]>())).Returns(true);
            functionMock.Setup(mock => mock.CreateReturnValue(It.IsAny<byte[]>())).Returns(resultMock.Object);

            var dataFrameMock = new Mock<IDataFrame>();
            dataFrameMock.SetupGet(mock => mock.Type).Returns(FrameType.Response);
            dataFrameMock.SetupGet(mock => mock.SerialCommandBytes).Returns(Array.Empty<byte>());

            var functionCall = new FunctionCall(
                _loggerMock.Object,
                _portMock.Object,
                functionMock.Object,
                1,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);

            _cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var task = functionCall.ExecuteAsync(_cancellationTokenSource.Token);

            await Task.Delay(1);
            _portMock.Raise(mock => mock.ControlFrameReceived += null, new ControlFrameEventArgs(FramePreamble.Ack));
            await Task.Delay(1);
            _portMock.Raise(mock => mock.DataFrameReceived += null, new DataFrameEventArgs(dataFrameMock.Object));

            var (transmitSuccess, returnValue) = await task;

            Assert.That(transmitSuccess, Is.True);
            Assert.That(returnValue, Is.EqualTo(resultMock.Object));
        }

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(mock => mock.ForContext(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(_loggerMock.Object);

            _portMock = new Mock<IZWaveSerialPort>();

            _cancellationTokenSource = new CancellationTokenSource();
        }
    }
}