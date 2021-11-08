// -------------------------------------------------------------------------------------------------
// <copyright file="AddNodeToNetworkResponse.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.NodeInclusion.AddNodeToNetwork
{
    public class AddNodeToNetworkResponse
    {
        internal AddNodeToNetworkResponse(
            bool success,
            byte nodeId,
            DeviceClass deviceClass,
            byte[] commandClasses)
        {
            Success = success;
            NodeId = nodeId;
            DeviceClass = deviceClass;
            CommandClasses = commandClasses;
        }

        public byte[] CommandClasses { get; }

        public DeviceClass DeviceClass { get; }

        public byte NodeId { get; }

        public bool Success { get; }
    }
}