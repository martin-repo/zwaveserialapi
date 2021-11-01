// -------------------------------------------------------------------------------------------------
// <copyright file="ZWaveSerialPort.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Protocol
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Ports;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    internal class ZWaveSerialPort : IDisposable
    {
        private const int MaxAttempts = 1 + RetransmissionCount;
        private const int RetransmissionCount = 3;

        // 6.2.2 Data frame delivery timeout @ INS12350-Serial-API-Host-Appl.-Prg.-Guide.pdf
        private readonly TimeSpan _frameDeliveryTimeout = TimeSpan.FromMilliseconds(1600);
        private readonly ILogger _logger;
        private readonly SerialPort _port;
        private readonly ManualResetEventSlim _portBytesAvailableEvent = new();
        private readonly SemaphoreSlim _writeSemaphore = new(1, 1);

        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveFramesTask;

        public ZWaveSerialPort(ILogger logger, string portName)
        {
            _logger = logger.ForContext<ZWaveSerialPort>().ForContext(Constants.ClassName, GetType().Name);

            _port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            _port.DataReceived += OnPortDataReceived;
        }

        ~ZWaveSerialPort()
        {
            Dispose(false);
        }

        public event EventHandler<FrameEventArgs>? FrameReceived;

        private event EventHandler<MessageTypeEventArgs>? MessageTypeReceived;

        public bool IsConnected => _port.IsOpen;

        public bool ReconnectOnFailure { get; set; } = true;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_port.IsOpen)
            {
                throw new InvalidOperationException("Already connected.");
            }

            _port.Open();
            _port.DiscardInBuffer();

            _logger.Debug("Serial port {PortName} opened.", _port.PortName);

            _cancellationTokenSource = new CancellationTokenSource();
            _receiveFramesTask = Task.Run(() => ReceiveFrames(_cancellationTokenSource.Token), cancellationToken);

            // 6.1.1 With hard reset @ INS12350-Serial-API-Host-Appl.-Prg.-Guide.pdf
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (!_port.IsOpen)
            {
                throw new InvalidOperationException("Already disconnected.");
            }

            _cancellationTokenSource?.Cancel();
            if (_receiveFramesTask != null
                && await Task.WhenAny(_receiveFramesTask, Task.Delay(TimeSpan.FromSeconds(1), cancellationToken)).ConfigureAwait(false)
                != _receiveFramesTask)
            {
                _logger.Warning("Timed out waiting for receiveFrameTask to finish.");
            }

            _port.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task TransmitFrameAsync(Frame frame, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await TryTransmitFrameAsync(frame, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }

                if (!ReconnectOnFailure)
                {
                    throw new TransmitException($"Frame transmission failed after {MaxAttempts} attempts.");
                }

                _logger.Information("Frame transmission failed after {Attempts} attempts.", MaxAttempts);

                // 6.3 Retransmission @ INS12350-Serial-API-Host-Appl.-Prg.-Guide.pdf
                _logger.Debug("Reconnecting serial port.");
                await DisconnectAsync(cancellationToken).ConfigureAwait(false);
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private void Dispose(bool disposeManaged)
        {
            if (!disposeManaged)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _port.Dispose();
        }

        private void OnPortDataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
        {
            _portBytesAvailableEvent.Set();
        }

        private void ReceiveFrames(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!WaitForPortContent(cancellationToken))
                {
                    continue;
                }

                if (!TryReadMessageType(out var messageType))
                {
                    continue;
                }

                MessageTypeReceived?.Invoke(this, new MessageTypeEventArgs(messageType.Value));
                if (messageType != MessageType.StartOfFrame)
                {
                    continue;
                }

                if (!TryReadFrameData(out var frame))
                {
                    continue;
                }

                if (!frame.IsChecksumValid)
                {
                    _logger.Warning("Invalid frame checksum.");
                    WriteMessageType(MessageType.Nack, cancellationToken);
                    continue;
                }

                WriteMessageType(MessageType.Ack, cancellationToken);

                FrameReceived?.Invoke(this, new FrameEventArgs(frame));
            }
        }

        private bool TryReadFrameData([NotNullWhen(true)] out Frame? frame)
        {
            frame = null;

            if (_port.BytesToRead == 0)
            {
                _logger.Error("Serial buffer does not contain frame data.");
                return false;
            }

            var frameLength = (byte)_port.ReadByte();

            var frameBytes = new byte[frameLength + 2];
            frameBytes[0] = (byte)MessageType.StartOfFrame;
            frameBytes[1] = frameLength;

            if (_port.BytesToRead < frameLength)
            {
                _logger.Error("Serial buffer contains less data than indicated by frame length byte.");
                _port.DiscardInBuffer();
                return false;
            }

            _port.Read(frameBytes, 2, frameLength);

            _logger.Debug("<< {FrameBytes}", BitConverter.ToString(frameBytes));

            frame = new Frame(frameBytes);
            return true;
        }

        private bool TryReadMessageType([NotNullWhen(true)] out MessageType? messageType)
        {
            if (_port.BytesToRead == 0)
            {
                _logger.Warning("Attempt to read message type when serial buffer is empty.");
                messageType = null;
                return false;
            }

            messageType = (MessageType)_port.ReadByte();
            switch (messageType)
            {
                case MessageType.StartOfFrame:
                    return true;
                case MessageType.Ack:
                case MessageType.Nack:
                case MessageType.Cancel:
                    _logger.Debug("<< {MessageType}", messageType);
                    return true;
                default:
                    _logger.Error("Invalid message type {MessageType}", messageType);
                    _port.DiscardInBuffer();
                    return false;
            }
        }

        private async Task<bool> TryTransmitFrameAsync(Frame frame, CancellationToken cancellationToken)
        {
            for (var transmission = 0; transmission < MaxAttempts; transmission++)
            {
                var messageTypeSource = new TaskCompletionSource<MessageType>();

                void MessageTypeHandler(object? sender, MessageTypeEventArgs eventArgs)
                {
                    if (eventArgs.MessageType != MessageType.StartOfFrame)
                    {
                        messageTypeSource.TrySetResult(eventArgs.MessageType);
                    }
                }

                MessageTypeReceived += MessageTypeHandler;
                try
                {
                    await WriteFrameAsync(frame, cancellationToken).ConfigureAwait(false);

                    if (await Task.WhenAny(messageTypeSource.Task, Task.Delay(_frameDeliveryTimeout, cancellationToken)).ConfigureAwait(false)
                        == messageTypeSource.Task
                        && messageTypeSource.Task.Result == MessageType.Ack)
                    {
                        return true;
                    }
                }
                finally
                {
                    MessageTypeReceived -= MessageTypeHandler;
                }

                // 6.3 Retransmission @ INS12350-Serial-API-Host-Appl.-Prg.-Guide.pdf
                var retransmissionDelay = TimeSpan.FromMilliseconds(100 + transmission * 1000);
                await Task.Delay(retransmissionDelay, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }

        private bool WaitForPortContent(CancellationToken cancellationToken)
        {
            _portBytesAvailableEvent.Reset();
            if (_port.BytesToRead != 0)
            {
                return true;
            }

            try
            {
                _portBytesAvailableEvent.Wait(cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private async Task WriteFrameAsync(Frame frame, CancellationToken cancellationToken)
        {
            await _writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _port.Write(frame.FrameBytes, 0, frame.FrameBytes.Length);
                _logger.Debug(">> {FrameBytes}", BitConverter.ToString(frame.FrameBytes));
            }
            finally
            {
                _writeSemaphore.Release();
            }
        }

        private void WriteMessageType(MessageType messageType, CancellationToken cancellationToken)
        {
            var messageTypeBytes = new[] { (byte)messageType };

            _writeSemaphore.Wait(cancellationToken);
            try
            {
                _port.Write(messageTypeBytes, 0, messageTypeBytes.Length);
                _logger.Debug(">> {MessageType}", messageType);
            }
            finally
            {
                _writeSemaphore.Release();
            }
        }
    }
}