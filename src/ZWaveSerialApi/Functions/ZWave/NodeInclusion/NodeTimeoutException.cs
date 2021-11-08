// -------------------------------------------------------------------------------------------------
// <copyright file="NodeTimeoutException.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion
{
    using System;

    internal class NodeTimeoutException : TimeoutException
    {
        public NodeTimeoutException(string message, byte nodeId, NodeTimeout nodeTimeout)
            : base(message)
        {
            NodeId = nodeId;
            NodeTimeout = nodeTimeout;
        }

        public byte NodeId { get; set; }

        public NodeTimeout NodeTimeout { get; set; }
    }
}