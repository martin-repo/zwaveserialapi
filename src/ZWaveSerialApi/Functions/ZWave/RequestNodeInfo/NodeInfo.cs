// -------------------------------------------------------------------------------------------------
// <copyright file="NodeInfo.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave.RequestNodeInfo
{
    public class NodeInfo
    {
        public NodeInfo(DeviceClass deviceClass, byte[] commandClasses)
        {
            DeviceClass = deviceClass;
            CommandClasses = commandClasses;
        }

        public byte[] CommandClasses { get; }

        public DeviceClass DeviceClass { get; }
    }
}