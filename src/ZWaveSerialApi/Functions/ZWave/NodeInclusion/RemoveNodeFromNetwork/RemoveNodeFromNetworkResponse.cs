// -------------------------------------------------------------------------------------------------
// <copyright file="RemoveNodeFromNetworkResponse.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion.RemoveNodeFromNetwork
{
    public class RemoveNodeFromNetworkResponse
    {
        internal RemoveNodeFromNetworkResponse(bool success, byte nodeId)
        {
            Success = success;
            NodeId = nodeId;
        }

        public byte NodeId { get; }

        public bool Success { get; }
    }
}