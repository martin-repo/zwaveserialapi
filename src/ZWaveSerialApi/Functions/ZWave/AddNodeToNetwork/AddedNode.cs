// -------------------------------------------------------------------------------------------------
// <copyright file="AddedNode.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.AddNodeToNetwork
{
    public class AddedNode
    {
        public AddedNode(
            byte nodeId,
            DeviceClass deviceClass,
            byte[] commandClasses,
            bool success)
        {
            NodeId = nodeId;
            DeviceClass = deviceClass;
            CommandClasses = commandClasses;
            Success = success;
        }

        public byte[] CommandClasses { get; }

        public DeviceClass DeviceClass { get; }

        public byte NodeId { get; }

        public bool Success { get; }
    }
}