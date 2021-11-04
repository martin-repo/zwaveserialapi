// -------------------------------------------------------------------------------------------------
// <copyright file="AeotecLedBulb6MultiColor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Brands.Aeotec
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application;
    using ZWaveSerialApi.CommandClasses.Application.ColorSwitch;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch;
    using ZWaveSerialApi.Devices.Device;
    using ZWaveSerialApi.Devices.Utilities;

    [DeviceName("Aeotec LED Bulb 6 MultiColor")]
    [DeviceType(0x0371, 0x0003, 0x0002, "EU")]
    [DeviceType(0x0371, 0x0103, 0x0002, "US")]
    public class AeotecLedBulb6MultiColor : Device, IMultiColorBulb
    {
        internal AeotecLedBulb6MultiColor(IZWaveSerialClient client, DeviceState deviceState)
            : base(client, deviceState)
        {
        }

        public async Task SetColdWhiteAsync(byte percentage, DurationType duration, CancellationToken cancellationToken)
        {
            if (percentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), "percentage must be 0 to 100.");
            }

            var value = (byte)Math.Round((double)percentage / 100 * 0xFF, MidpointRounding.AwayFromZero);
            var colorComponents = new[]
                                  {
                                      new ColorComponent(ColorComponentType.WarmWhite, 0), new ColorComponent(ColorComponentType.ColdWhite, value)
                                  };
            await Client.GetCommandClass<ColorSwitchCommandClass>()
                        .SetAsync(NodeId, colorComponents, duration, cancellationToken)
                        .ConfigureAwait(false);
        }

        public async Task SetColorAsync(Color color, DurationType duration, CancellationToken cancellationToken)
        {
            var colorComponents = new[]
                                  {
                                      new ColorComponent(ColorComponentType.WarmWhite, 0),
                                      new ColorComponent(ColorComponentType.ColdWhite, 0),
                                      new ColorComponent(ColorComponentType.Red, color.R),
                                      new ColorComponent(ColorComponentType.Green, color.G),
                                      new ColorComponent(ColorComponentType.Blue, color.B)
                                  };
            await Client.GetCommandClass<ColorSwitchCommandClass>()
                        .SetAsync(NodeId, colorComponents, duration, cancellationToken)
                        .ConfigureAwait(false);
        }

        public async Task SetIntensityAsync(byte percentage, DurationType duration, CancellationToken cancellationToken)
        {
            if (percentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), "percentage must be 0 to 100.");
            }

            var value = (byte)Math.Round((double)percentage / 100 * 99, MidpointRounding.AwayFromZero);
            await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, value, duration, cancellationToken).ConfigureAwait(false);
        }

        public async Task SetWarmWhiteAsync(byte percentage, DurationType duration, CancellationToken cancellationToken)
        {
            if (percentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), "percentage must be 0 to 100.");
            }

            var value = (byte)Math.Round((double)percentage / 100 * 0xFF, MidpointRounding.AwayFromZero);
            var colorComponents = new[] { new ColorComponent(ColorComponentType.WarmWhite, value) };
            await Client.GetCommandClass<ColorSwitchCommandClass>()
                        .SetAsync(NodeId, colorComponents, duration, cancellationToken)
                        .ConfigureAwait(false);
        }

        public async Task TurnOffAsync(DurationType duration, CancellationToken cancellationToken)
        {
            await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, 0x00, duration, cancellationToken).ConfigureAwait(false);
        }

        public async Task TurnOnAsync(DurationType duration, CancellationToken cancellationToken)
        {
            await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, 0xFF, duration, cancellationToken).ConfigureAwait(false);
        }
    }
}