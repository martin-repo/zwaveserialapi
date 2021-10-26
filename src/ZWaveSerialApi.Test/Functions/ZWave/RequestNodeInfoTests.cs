// -------------------------------------------------------------------------------------------------
// <copyright file="RequestNodeInfoTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave.RequestNodeInfo;

    public class RequestNodeInfoTests
    {
        [TestCase(1, "60-01")]
        [TestCase(2, "60-02")]
        public void Create(byte destinationNodeId, string expectedFunctionArgsBytesString)
        {
            var functionTx = RequestNodeInfoTx.Create(destinationNodeId);

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase(1, "60-01", true)]
        [TestCase(2, "60-01", true)]
        [TestCase(1, "60-00", false)]
        public void CreateReturnValue(byte destinationNodeId, string returnValueBytesString, bool expectedSuccess)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = RequestNodeInfoTx.Create(destinationNodeId);
            var returnValue = (RequestNodeInfoRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(returnValue.Success, Is.EqualTo(expectedSuccess));
        }

        [TestCase(1, "60-01")]
        [TestCase(1, "60-00")]
        public void IsValidReturnValue(byte destinationNodeId, string returnValueBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = RequestNodeInfoTx.Create(destinationNodeId);
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.True);
        }
    }
}