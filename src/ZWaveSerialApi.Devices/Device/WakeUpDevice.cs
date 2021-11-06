// -------------------------------------------------------------------------------------------------
// <copyright file="WakeUpDevice.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.Threading;

    using ZWaveSerialApi.CommandClasses.Management.WakeUp;

    public abstract class WakeUpDevice : Device
    {
        private readonly WakeUpCommandClass _wakeUp;

        internal WakeUpDevice(IZWaveSerialClient client, DeviceState deviceState)
            : base(client, deviceState)
        {
            _wakeUp = Client.GetCommandClass<WakeUpCommandClass>();
        }

        public event AsyncEventHandler? WakeUpNotification;

        public bool IsAwake { get; internal set; }

        public async Task<TimeSpan> GetWakeUpIntervalAsync(CancellationToken cancellationToken = default)
        {
            AssertAwake();
            return await _wakeUp.IntervalGetAsync(NodeId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<WakeUpIntervalCapabilitiesReport> GetWakeUpIntervalCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            AssertAwake();
            return await _wakeUp.IntervalCapabilitiesGetAsync(NodeId, cancellationToken).ConfigureAwait(false);
        }

        public async Task SetWakeUpIntervalAsync(TimeSpan interval, CancellationToken cancellationToken = default)
        {
            AssertAwake();
            await _wakeUp.IntervalSetAsync(NodeId, interval, cancellationToken).ConfigureAwait(false);
        }

        internal void AssertAwake()
        {
            if (!IsAwake && !IsAlwaysOn)
            {
                throw new InvalidOperationException(
                    $"Cannot communicate with device ({nameof(IsAwake)}={IsAwake} {nameof(IsAlwaysOn)}={IsAlwaysOn}).");
            }
        }

        internal async Task OnWakeUpNotificationAsync()
        {
            await (WakeUpNotification?.InvokeAsync(this, EventArgs.Empty) ?? Task.CompletedTask);
        }
    }
}