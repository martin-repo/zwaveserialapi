// -------------------------------------------------------------------------------------------------
// <copyright file="ApplicationCommandHandlerBridgeTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.CommandClasses;
    using ZWaveSerialApi.Functions.ZWave;

    public class ApplicationCommandHandlerBridgeTests
    {
        [TestCase("A8-00-01-02-03-71-FF-FF-00-FF", 1, 2, "71-FF-FF", CommandClassType.Notification)]
        [TestCase("A8-00-02-01-03-71-FF-FF-00-FF", 2, 1, "71-FF-FF", CommandClassType.Notification)]
        [TestCase("A8-00-01-02-04-71-FF-FF-FF-00-FF", 1, 2, "71-FF-FF-FF", CommandClassType.Notification)]
        [TestCase("A8-00-01-02-03-20-FF-FF-00-FF", 1, 2, "20-FF-FF", CommandClassType.Basic)]
        [TestCase("A8-00-01-02-03-71-FF-FF-01-AA-FF", 1, 2, "71-FF-FF", CommandClassType.Notification)]
        public void Constructor(
            string bytesString,
            byte expectedDestinationNodeId,
            byte expectedSourceNodeId,
            string expectedCommandClassBytesString,
            CommandClassType expectedCommandClassType)
        {
            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionRx = new ApplicationCommandHandlerBridgeRx(bytes);

            Assert.That(functionRx.DestinationNodeId, Is.EqualTo(expectedDestinationNodeId));
            Assert.That(functionRx.SourceNodeId, Is.EqualTo(expectedSourceNodeId));
            Assert.That(functionRx.CommandClassType, Is.EqualTo(expectedCommandClassType));
            Assert.That(BitConverter.ToString(functionRx.CommandClassBytes), Is.EqualTo(expectedCommandClassBytesString));
        }
    }
}