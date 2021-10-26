// -------------------------------------------------------------------------------------------------
// <copyright file="GetNodeProtocolInfoTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave;

    public class GetNodeProtocolInfoTests
    {
        [TestCase(1, "41-01")]
        [TestCase(2, "41-02")]
        public void Create(byte destinationNodeId, string expectedFunctionArgsBytesString)
        {
            var functionTx = GetNodeProtocolInfoTx.Create(destinationNodeId);

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase(1, "41-80-00-00-00-00-00", true)]
        [TestCase(1, "41-81-00-00-00-00-00", true)]
        [TestCase(1, "41-7F-00-00-00-00-00", false)]
        public void CreateReturnValue(byte destinationNodeId, string returnValueBytesString, bool expectedListening)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = GetNodeProtocolInfoTx.Create(destinationNodeId);
            var returnValue = (GetNodeProtocolInfoRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(returnValue.Listening, Is.EqualTo(expectedListening));
        }

        [TestCase(1, "41-80-00-00-00-00-00")]
        [TestCase(1, "41-81-00-00-00-00-00")]
        public void IsValidReturnValue(byte destinationNodeId, string returnValueBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = GetNodeProtocolInfoTx.Create(destinationNodeId);
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.True);
        }
    }
}