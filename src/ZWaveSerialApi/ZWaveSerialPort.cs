// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveSerialPort.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Ports;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using ZWaveSerialApi.Frames;
    using ZWaveSerialApi.Utilities;

    public class ZWaveSerialPort : IZWaveSerialPort, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ILogger _logger;
        private readonly SerialPort _port;
        private readonly AsyncManualResetEvent _portBytesAvailableEvent = new();
        private readonly SemaphoreSlim _portSemaphore = new(1, 1);

        public ZWaveSerialPort(ILogger logger, string portName)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);

            _port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            _port.DataReceived += OnPortDataReceived;
            _port.Open();

            _logger.Debug("Serial port {PortName} opened.", portName);

            _port.DiscardInBuffer();
            _ = ReceiveFramesAsync(_cancellationTokenSource.Token);
        }

        public event EventHandler<ControlFrameEventArgs>? ControlFrameReceived;

        public event EventHandler<DataFrameEventArgs>? DataFrameReceived;

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            _port.DataReceived -= OnPortDataReceived;
            _port.Close();
            _port.Dispose();
        }

        public async Task WriteDataFrameAsync(IDataFrame dataFrame, CancellationToken cancellationToken)
        {
            await _portSemaphore.WaitAsync(cancellationToken);
            try
            {
                _port.Write(dataFrame.Bytes, 0, dataFrame.Bytes.Length);
                _logger.Debug(">> {FrameBytes}", BitConverter.ToString(dataFrame.Bytes));
            }
            finally
            {
                _portSemaphore.Release();
            }
        }

        private void OnPortDataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
        {
            _portBytesAvailableEvent.Set();
        }

        private async Task ReceiveFramesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await WaitForPortContentAsync(cancellationToken);

                await _portSemaphore.WaitAsync(cancellationToken);
                try
                {
                    if (!TryReadFrame(out var frame))
                    {
                        continue;
                    }

                    if (frame is ControlFrame controlFrame)
                    {
                        ControlFrameReceived?.Invoke(this, new ControlFrameEventArgs(controlFrame.Preamble));
                        continue;
                    }

                    var dataFrame = (DataFrame)frame;
                    if (!dataFrame.IsChecksumValid)
                    {
                        WriteControlFrame(FramePreamble.Nack);
                        _logger.Warning("Invalid checksum.");
                        continue;
                    }

                    WriteControlFrame(FramePreamble.Ack);

                    DataFrameReceived?.Invoke(this, new DataFrameEventArgs(dataFrame));
                }
                finally
                {
                    _portSemaphore.Release();
                }
            }
        }

        private bool TryReadFrame([NotNullWhen(true)] out Frame? frame)
        {
            frame = null;

            if (_port.BytesToRead == 0)
            {
                _logger.Warning("Attempt to read frame when serial buffer is empty.");
                return false;
            }

            var framePreamble = (FramePreamble)_port.ReadByte();
            switch (framePreamble)
            {
                case FramePreamble.StartOfFrame:
                    break;
                case FramePreamble.Ack:
                case FramePreamble.Nack:
                case FramePreamble.Cancel:
                    _logger.Debug("<< {FrameType}", framePreamble);
                    frame = new ControlFrame((byte)framePreamble);
                    return true;
                default:
                    _logger.Error("Invalid frame type {FrameType}", framePreamble);
                    _port.DiscardInBuffer();
                    return false;
            }

            if (_port.BytesToRead == 0)
            {
                _logger.Warning("Serial buffer does not contain frame data.");
                return false;
            }

            var frameLength = (byte)_port.ReadByte();

            var frameBytes = new byte[frameLength + 2];
            frameBytes[0] = (byte)framePreamble;
            frameBytes[1] = frameLength;

            if (_port.BytesToRead < frameLength)
            {
                _logger.Warning("Serial buffer contains less data than indicated by frame length byte.");
                _port.DiscardInBuffer();
                return false;
            }

            _port.Read(frameBytes, 2, frameLength);

            _logger.Debug("<< {FrameBytes}", BitConverter.ToString(frameBytes));

            frame = new DataFrame(frameBytes);
            return true;
        }

        private async Task WaitForPortContentAsync(CancellationToken cancellationToken)
        {
            _portBytesAvailableEvent.Reset();
            if (_port.BytesToRead != 0)
            {
                return;
            }

            await _portBytesAvailableEvent.WaitAsync(cancellationToken);
        }

        private void WriteControlFrame(FramePreamble framePreamble)
        {
            var frameBytes = new[] { (byte)framePreamble };
            _logger.Debug(">> {FrameType}", framePreamble);
            _port.Write(frameBytes, 0, frameBytes.Length);
        }
    }
}