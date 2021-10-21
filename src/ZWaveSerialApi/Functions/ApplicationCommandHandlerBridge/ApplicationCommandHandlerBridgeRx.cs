// -------------------------------------------------------------------------------------------------
// <copyright file="ApplicationCommandHandlerBridgeRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    using System;

    using ZWaveSerialApi.CommandClasses;

    public class ApplicationCommandHandlerBridgeRx : FunctionRx
    {
        public ApplicationCommandHandlerBridgeRx(byte[] returnValueBytes)
            : base(FunctionType.ApplicationCommandHandlerBridge, returnValueBytes)
        {
            var receiveStatus = returnValueBytes[1];
            DestinationNodeId = returnValueBytes[2];
            SourceNodeId = returnValueBytes[3];
            var commandClassLength = returnValueBytes[4];
            CommandClassBytes = returnValueBytes[5..(5 + commandClassLength)];

            var index = 5 + commandClassLength;
            var multiNodeMaskLength = returnValueBytes[index++];
            var multiNodeMask = multiNodeMaskLength > 0 ? returnValueBytes[index..(index + multiNodeMaskLength)] : Array.Empty<byte>();
            index += multiNodeMaskLength;

            var receiveSignalStrength = (sbyte)returnValueBytes[index];

            CommandClassType = (CommandClassType)CommandClassBytes[0];
        }

        public byte[] CommandClassBytes { get; }

        public CommandClassType CommandClassType { get; }

        public byte DestinationNodeId { get; }

        public byte SourceNodeId { get; }
    }
}