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
    using ZWaveSerialApi.Devices.Settings;

    public class AeotecLedBulb6MultiColor : Device
    {
        internal AeotecLedBulb6MultiColor(ZWaveSerialClient client, byte nodeId, NetworkDevice networkDevice)
            : base(client, nodeId, networkDevice)
        {
        }

        public async Task SetColdWhiteAsync(byte value, DurationType duration, CancellationToken cancellationToken)
        {
            var colorComponents = new[]
                                  {
                                      new ColorComponent(ColorComponentType.WarmWhite, 0), new ColorComponent(ColorComponentType.ColdWhite, value)
                                  };
            await Client.GetCommandClass<ColorSwitchCommandClass>().SetAsync(NodeId, colorComponents, duration, cancellationToken);
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
            await Client.GetCommandClass<ColorSwitchCommandClass>().SetAsync(NodeId, colorComponents, duration, cancellationToken);
        }

        public async Task SetIntensityAsync(byte intensity, DurationType duration, CancellationToken cancellationToken)
        {
            if (intensity > 99)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity), "Intensity must be between 0 and 99 (inclusive).");
            }

            await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, intensity, duration, cancellationToken);
        }

        public async Task SetWarmWhiteAsync(byte value, DurationType duration, CancellationToken cancellationToken)
        {
            var colorComponents = new[] { new ColorComponent(ColorComponentType.WarmWhite, value) };
            await Client.GetCommandClass<ColorSwitchCommandClass>().SetAsync(NodeId, colorComponents, duration, cancellationToken);
        }

        public async Task TurnOffAsync(DurationType duration, CancellationToken cancellationToken)
        {
            await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, 0x00, duration, cancellationToken);
        }

        public async Task TurnOnAsync(DurationType duration, CancellationToken cancellationToken)
        {
            await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, 0xFF, duration, cancellationToken);
        }
    }
}