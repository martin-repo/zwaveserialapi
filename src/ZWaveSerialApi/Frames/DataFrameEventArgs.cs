// -------------------------------------------------------------------------------------------------
// <copyright file="DataFrameEventArgs.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    using System;

    public class DataFrameEventArgs : EventArgs
    {
        public DataFrameEventArgs(IDataFrame dataFrame)
        {
            DataFrame = dataFrame;
        }

        public IDataFrame DataFrame { get; }
    }
}