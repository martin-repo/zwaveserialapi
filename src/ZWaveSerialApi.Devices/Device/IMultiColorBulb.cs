// -------------------------------------------------------------------------------------------------
// <copyright file="IMultiColorBulb.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application;

    public interface IMultiColorBulb : IMultiWhiteBulb
    {
        Task SetColorAsync(Color color, DurationType duration, CancellationToken cancellationToken);
    }
}