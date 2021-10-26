// -------------------------------------------------------------------------------------------------
// <copyright file="GetSucNodeIdTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave;

    public class GetSucNodeIdTests
    {
        [TestCase("56")]
        public void Create(string expectedFunctionArgsBytesString)
        {
            var functionTx = GetSucNodeIdTx.Create();

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase("56-01", 1)]
        [TestCase("56-02", 2)]
        public void CreateReturnValue(string returnValueBytesString, byte expectedNodeId)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = GetSucNodeIdTx.Create();
            var returnValue = (GetSucNodeIdRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(returnValue.NodeId, Is.EqualTo(expectedNodeId));
        }

        [TestCase("56-01")]
        [TestCase("56-02")]
        public void IsValidReturnValue(string returnValueBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = GetSucNodeIdTx.Create();
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.True);
        }
    }
}