// -------------------------------------------------------------------------------------------------
// <copyright file="LoggerExtensions.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses
{
    using System;
    using System.Linq;

    using Serilog;

    public static class LoggerExtensions
    {
        public static void InboundCommand(
            this ILogger logger,
            byte sourceNodeId,
            byte[] commandClassBytes,
            params Enum[] commandParts)
        {
            var nodeId = "0x" + BitConverter.ToString(new[] { sourceNodeId });
            var command = string.Join(':', commandParts.Select(commandPart => commandPart.ToString()));
            logger.Debug("<< (Node {NodeId}) {Command} {CommandClassBytes}", nodeId, command, BitConverter.ToString(commandClassBytes));
        }

        public static void OutboundCommand(
            this ILogger logger,
            byte destinationNodeId,
            byte[] commandClassBytes,
            params Enum[] commandParts)
        {
            var nodeId = "0x" + BitConverter.ToString(new[] { destinationNodeId });
            var command = string.Join(':', commandParts.Select(commandPart => commandPart.ToString()));
            logger.Debug(">> (Node {NodeId}) {Command} {CommandClassBytes}", nodeId, command, BitConverter.ToString(commandClassBytes));
        }
    }
}