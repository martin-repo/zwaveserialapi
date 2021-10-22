// -------------------------------------------------------------------------------------------------
// <copyright file="IZWaveSerialPort.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using ZWaveSerialApi.Frames;

    internal interface IZWaveSerialPort
    {
        event EventHandler<ControlFrameEventArgs>? ControlFrameReceived;

        event EventHandler<DataFrameEventArgs>? DataFrameReceived;

        Task WriteDataFrameAsync(IDataFrame dataFrame, CancellationToken cancellationToken);
    }
}