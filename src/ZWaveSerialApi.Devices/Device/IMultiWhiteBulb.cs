// -------------------------------------------------------------------------------------------------
// <copyright file="IMultiWhiteBulb.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Devices.Device
{
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.CommandClasses.Application;

    public interface IMultiWhiteBulb
    {
        Task SetColdWhiteAsync(byte value, DurationType duration, CancellationToken cancellationToken);

        Task SetIntensityAsync(byte intensity, DurationType duration, CancellationToken cancellationToken);

        Task SetWarmWhiteAsync(byte value, DurationType duration, CancellationToken cancellationToken);

        Task TurnOffAsync(DurationType duration, CancellationToken cancellationToken);

        Task TurnOnAsync(DurationType duration, CancellationToken cancellationToken);
    }
}