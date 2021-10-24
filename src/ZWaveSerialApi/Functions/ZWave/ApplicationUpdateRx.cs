// -------------------------------------------------------------------------------------------------
// <copyright file="ApplicationUpdateRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.ZWave
{
    internal class ApplicationUpdateRx : FunctionRx
    {
        public ApplicationUpdateRx(byte[] returnValueBytes)
            : base(FunctionType.ApplicationUpdate, returnValueBytes)
        {
            Status = (ApplicationUpdateStatus)returnValueBytes[1];
            SourceNodeId = returnValueBytes[2];

            // Remaining content depends on ApplicationUpdateStatus
            // See 4.3.1.8 ApplicationControllerUpdate
        }

        public byte SourceNodeId { get; }

        public ApplicationUpdateStatus Status { get; }
    }
}