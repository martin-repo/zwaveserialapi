// -------------------------------------------------------------------------------------------------
// <copyright file="ZWavePlusInfoReport.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.Management.ZWavePlusInfo
{
    public class ZWavePlusInfoReport
    {
        public ZWavePlusInfoReport(
            byte zwavePlusVersion,
            SlaveRoleType roleType,
            NodeType nodeType,
            byte[] installerIcon,
            byte[] userIcon)
        {
            ZWavePlusVersion = zwavePlusVersion;
            RoleType = roleType;
            NodeType = nodeType;
            InstallerIcon = installerIcon;
            UserIcon = userIcon;
        }

        public byte[] InstallerIcon { get; }

        public NodeType NodeType { get; }

        public SlaveRoleType RoleType { get; }

        public byte[] UserIcon { get; }

        public byte ZWavePlusVersion { get; }
    }
}