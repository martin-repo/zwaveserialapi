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

    public interface IMultiWhiteBulb : IDevice
    {
        Task<byte> GetIntensityAsync(CancellationToken cancellationToken);

        Task SetColdWhiteAsync(byte percentage, DurationType duration, CancellationToken cancellationToken);

        Task SetIntensityAsync(byte percentage, DurationType duration, CancellationToken cancellationToken);

        Task SetWarmWhiteAsync(byte percentage, DurationType duration, CancellationToken cancellationToken);

        Task TurnOffAsync(DurationType duration, CancellationToken cancellationToken);

        Task TurnOnAsync(DurationType duration, CancellationToken cancellationToken);
    }
}