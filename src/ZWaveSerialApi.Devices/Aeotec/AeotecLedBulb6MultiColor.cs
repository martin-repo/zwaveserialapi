// -------------------------------------------------------------------------------------------------
// <copyright file="AeotecLedBulb6MultiColor.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Aeotec
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application;
    using ZWaveSerialApi.CommandClasses.Application.ColorSwitch;
    using ZWaveSerialApi.CommandClasses.Application.MultilevelSwitch;

    public class AeotecLedBulb6MultiColor : Device
    {
        public AeotecLedBulb6MultiColor(ZWaveSerialClient client, byte nodeId)
            : base(client, nodeId)
        {
        }

        public async Task<bool> SetColdWhiteAsync(byte value, DurationType duration, CancellationToken cancellationToken)
        {
            var colorComponents = new[]
                                  {
                                      new ColorComponent(ColorComponentType.WarmWhite, 0), new ColorComponent(ColorComponentType.ColdWhite, value)
                                  };
            return await Client.GetCommandClass<ColorSwitchCommandClass>().SetAsync(NodeId, colorComponents, duration, cancellationToken);
        }

        public async Task<bool> SetColorAsync(Color color, DurationType duration, CancellationToken cancellationToken)
        {
            var colorComponents = new[]
                                  {
                                      new ColorComponent(ColorComponentType.WarmWhite, 0),
                                      new ColorComponent(ColorComponentType.ColdWhite, 0),
                                      new ColorComponent(ColorComponentType.Red, color.R),
                                      new ColorComponent(ColorComponentType.Green, color.G),
                                      new ColorComponent(ColorComponentType.Blue, color.B)
                                  };
            return await Client.GetCommandClass<ColorSwitchCommandClass>().SetAsync(NodeId, colorComponents, duration, cancellationToken);
        }

        public async Task<bool> SetIntensityAsync(byte intensity, DurationType duration, CancellationToken cancellationToken)
        {
            if (intensity > 99)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity), "Intensity must be between 0 and 99 (inclusive).");
            }

            return await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, intensity, duration, cancellationToken);
        }

        public async Task<bool> SetWarmWhiteAsync(byte value, DurationType duration, CancellationToken cancellationToken)
        {
            var colorComponents = new[] { new ColorComponent(ColorComponentType.WarmWhite, value) };
            return await Client.GetCommandClass<ColorSwitchCommandClass>().SetAsync(NodeId, colorComponents, duration, cancellationToken);
        }

        public async Task<bool> TurnOffAsync(DurationType duration, CancellationToken cancellationToken)
        {
            return await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, 0x00, duration, cancellationToken);
        }

        public async Task<bool> TurnOnAsync(DurationType duration, CancellationToken cancellationToken)
        {
            return await Client.GetCommandClass<MultilevelSwitchCommandClass>().SetAsync(NodeId, 0xFF, duration, cancellationToken);
        }
    }
}