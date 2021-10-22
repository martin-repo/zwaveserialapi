// -------------------------------------------------------------------------------------------------
// <copyright file="FunctionCall.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serilog;

    using Stateless;

    using ZWaveSerialApi.Frames;

    internal class FunctionCall
    {
        private readonly int _attempts;
        private readonly StateMachine<TransmitState, TransmitTrigger>.TriggerWithParameters<bool, IFunctionRx?> _completeTrigger;
        private readonly TaskCompletionSource<FunctionCallResult> _completionSource = new();
        private readonly IFunctionTx _function;
        private readonly ILogger _logger;
        private readonly StateMachine<TransmitState, TransmitTrigger> _machine = new(TransmitState.Idle);
        private readonly TimeSpan _networkTimeout;
        private readonly IZWaveSerialPort _port;
        private readonly TimeSpan _retryDelay;

        private int _attempt;
        private CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationTokenRegistration;
        private CancellationTokenSource? _timeoutCancellationTokenSource;

        public FunctionCall(
            ILogger logger,
            IZWaveSerialPort port,
            IFunctionTx function,
            int attempts,
            TimeSpan networkTimeout,
            TimeSpan retryDelay)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);

            _port = port;
            _port.ControlFrameReceived += OnControlFrameReceived;
            _port.DataFrameReceived += OnDataFrameReceived;

            _function = function;
            _attempts = attempts;
            _networkTimeout = networkTimeout;
            _retryDelay = retryDelay;

            _completeTrigger = _machine.SetTriggerParameters<bool, IFunctionRx?>(TransmitTrigger.Complete);

            _machine.Configure(TransmitState.Idle).Permit(TransmitTrigger.SendCommand, TransmitState.SendingCommand);
            _machine.Configure(TransmitState.SendingCommand)
                    .OnEntryAsync(SendCommandAsync)
                    .Permit(TransmitTrigger.AwaitAck, TransmitState.AwaitingAck)
                    .Permit(TransmitTrigger.Complete, TransmitState.Completed)
                    .Permit(TransmitTrigger.Cancel, TransmitState.Cancelled);
            _machine.Configure(TransmitState.AwaitingAck)
                    .Permit(TransmitTrigger.SendCommand, TransmitState.SendingCommand)
                    .Permit(TransmitTrigger.RetryAfterDelay, TransmitState.RetryingAfterDelay)
                    .Permit(TransmitTrigger.AwaitResponse, TransmitState.AwaitingResponse)
                    .Permit(TransmitTrigger.Complete, TransmitState.Completed)
                    .Permit(TransmitTrigger.Cancel, TransmitState.Cancelled);
            _machine.Configure(TransmitState.RetryingAfterDelay)
                    .OnEntryAsync(RetryAfterDelayAsync)
                    .Permit(TransmitTrigger.SendCommand, TransmitState.SendingCommand)
                    .Permit(TransmitTrigger.Cancel, TransmitState.Cancelled);
            _machine.Configure(TransmitState.AwaitingResponse)
                    .Permit(TransmitTrigger.SendCommand, TransmitState.SendingCommand)
                    .Permit(TransmitTrigger.Complete, TransmitState.Completed)
                    .Permit(TransmitTrigger.Cancel, TransmitState.Cancelled);
            _machine.Configure(TransmitState.Completed).OnEntryFrom(_completeTrigger, OnCompleted);
        }

        public Task<FunctionCallResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_machine.IsInState(TransmitState.Idle))
            {
                throw new InvalidOperationException("Function is already executing.");
            }

            _cancellationToken = cancellationToken;
            _cancellationTokenRegistration = _cancellationToken.Register(() => _machine.Fire(TransmitTrigger.Cancel));

            _ = _machine.FireAsync(TransmitTrigger.SendCommand);
            return _completionSource.Task;
        }

        private async Task FireSendCommandAfterDelayAsync(TimeSpan delay)
        {
            _timeoutCancellationTokenSource?.Cancel();
            _timeoutCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(delay, _timeoutCancellationTokenSource.Token);
                await _machine.FireAsync(TransmitTrigger.SendCommand);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnCompleted(bool success, IFunctionRx? functionRx)
        {
            _cancellationTokenRegistration.Dispose();

            _logger.Debug("Completed");
            _completionSource.TrySetResult(new FunctionCallResult(success, functionRx));
        }

        private void OnControlFrameReceived(object? sender, ControlFrameEventArgs eventArgs)
        {
            if (!_machine.IsInState(TransmitState.AwaitingAck))
            {
                return;
            }

            _timeoutCancellationTokenSource?.Cancel();

            _logger.Debug("{Preamble} received", eventArgs.FramePreamble);

            switch (eventArgs.FramePreamble)
            {
                case FramePreamble.Ack:
                    if (_function.HasReturnValue)
                    {
                        _ = FireSendCommandAfterDelayAsync(_networkTimeout);
                        _machine.Fire(TransmitTrigger.AwaitResponse);
                    }
                    else
                    {
                        _machine.Fire(_completeTrigger, true, null);
                    }

                    break;
                case FramePreamble.Nack:
                    _machine.Fire(TransmitTrigger.SendCommand);
                    break;
                case FramePreamble.Cancel:
                    _machine.Fire(TransmitTrigger.RetryAfterDelay);
                    break;
                default:
                    _logger.Error("Invalid control frame preamble {FramePreamble}", eventArgs.FramePreamble);
                    _machine.Fire(_completeTrigger, false, null);
                    break;
            }
        }

        private void OnDataFrameReceived(object? sender, DataFrameEventArgs eventArgs)
        {
            if (!_machine.IsInState(TransmitState.AwaitingResponse))
            {
                return;
            }

            var dataFrame = eventArgs.DataFrame;
            if (dataFrame.Type != FrameType.Response || !_function.IsValidReturnValue(dataFrame.SerialCommandBytes))
            {
                return;
            }

            _timeoutCancellationTokenSource?.Cancel();

            _logger.Debug("Response received");

            var returnValue = _function.CreateReturnValue(dataFrame.SerialCommandBytes);
            _machine.Fire(_completeTrigger, true, returnValue);
        }

        private async Task RetryAfterDelayAsync()
        {
            try
            {
                await Task.Delay(_retryDelay, _cancellationToken);
                await _machine.FireAsync(TransmitTrigger.SendCommand);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task SendCommandAsync()
        {
            if (++_attempt > _attempts)
            {
                _logger.Debug("Aborting after {Attempts} attempt(s)", _attempts);
                await _machine.FireAsync(_completeTrigger, false, null);
            }

            var dataFrame = DataFrame.Create(FrameType.Request, _function.FunctionArgsBytes);

            _logger.Debug("Sending request");

            _ = FireSendCommandAfterDelayAsync(_networkTimeout);
            await _port.WriteDataFrameAsync(dataFrame, _cancellationToken);
            await _machine.FireAsync(TransmitTrigger.AwaitAck);
        }

        private enum TransmitState
        {
            Idle,
            SendingCommand,
            AwaitingAck,
            AwaitingResponse,
            Completed,
            RetryingAfterDelay,
            Cancelled
        }

        private enum TransmitTrigger
        {
            SendCommand,
            AwaitAck,
            AwaitResponse,
            Complete,
            RetryAfterDelay,
            Cancel
        }
    }
}