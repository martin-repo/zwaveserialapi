// -------------------------------------------------------------------------------------------------
// <copyright file="ApplicationSlaveUpdateTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave.RequestNodeInfo;

    internal class ApplicationSlaveUpdateTests
    {
        [TestCase(
            "49-81-00",
            UpdateState.NodeInfoRequestFailed,
            0,
            0,
            0,
            0,
            "")]
        [TestCase(
            "49-84-01-06-10-11-12-AA-BB-CC",
            UpdateState.NodeInfoReceived,
            1,
            16,
            17,
            18,
            "AA-BB-CC")]
        [TestCase(
            "49-84-02-06-10-11-12-AA-BB-CC",
            UpdateState.NodeInfoReceived,
            2,
            16,
            17,
            18,
            "AA-BB-CC")]
        [TestCase(
            "49-84-01-07-10-11-12-AA-BB-CC-DD",
            UpdateState.NodeInfoReceived,
            1,
            16,
            17,
            18,
            "AA-BB-CC-DD")]
        public void Constructor(
            string bytesString,
            UpdateState expectedState,
            byte expectedSourceNodeId,
            byte expectedDeviceClassBasic,
            byte expectedDeviceClassGeneric,
            byte expectedDeviceClassSpecific,
            string expectedCommandClassesBytesString)
        {
            var bytes = bytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionRx = new ApplicationSlaveUpdateRx(bytes);

            Assert.That(functionRx.State, Is.EqualTo(expectedState));
            Assert.That(functionRx.SourceNodeId, Is.EqualTo(expectedSourceNodeId));
            Assert.That(functionRx.DeviceClass.Basic, Is.EqualTo(expectedDeviceClassBasic));
            Assert.That(functionRx.DeviceClass.Generic, Is.EqualTo(expectedDeviceClassGeneric));
            Assert.That(functionRx.DeviceClass.Specific, Is.EqualTo(expectedDeviceClassSpecific));
            Assert.That(BitConverter.ToString(functionRx.CommandClasses), Is.EqualTo(expectedCommandClassesBytesString));
        }
    }
}