// -------------------------------------------------------------------------------------------------
// <copyright file="MemoryGetIdTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave;

    public class MemoryGetIdTests
    {
        [TestCase("20")]
        public void Create(string expectedFunctionArgsBytesString)
        {
            var functionTx = MemoryGetIdTx.Create();

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase("20-01-02-03-04-05", "01-02-03-04", 5)]
        [TestCase("20-11-12-13-14-15", "11-12-13-14", 21)]
        public void CreateReturnValue(string returnValueBytesString, string expectedHomeIdBytesString, byte expectedNodeId)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = MemoryGetIdTx.Create();
            var returnValue = (MemoryGetIdRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(BitConverter.ToString(returnValue.HomeId), Is.EqualTo(expectedHomeIdBytesString));
            Assert.That(returnValue.NodeId, Is.EqualTo(expectedNodeId));
        }

        [TestCase("20-01-02-03-04-05")]
        [TestCase("20-11-12-13-14-15")]
        public void IsValidReturnValue(string returnValueBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = MemoryGetIdTx.Create();
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.True);
        }
    }
}