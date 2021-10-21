// -------------------------------------------------------------------------------------------------
// <copyright file="IDataFrame.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    using ZWaveSerialApi.Functions;

    public interface IDataFrame : IFrame
    {
        FunctionType FunctionType { get; }

        byte[] SerialCommandBytes { get; }

        FrameType Type { get; }
    }
}