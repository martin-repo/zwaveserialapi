// -------------------------------------------------------------------------------------------------
// <copyright file="INodeFunctionRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion
{
    internal interface INodeFunctionRx : IFunctionRx
    {
        byte SourceNodeId { get; }

        NodeStatus Status { get; }
    }
}