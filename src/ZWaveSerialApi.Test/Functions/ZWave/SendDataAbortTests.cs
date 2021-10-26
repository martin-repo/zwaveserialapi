// -------------------------------------------------------------------------------------------------
// <copyright file="SendDataAbortTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave.SendData;

    public class SendDataAbortTests
    {
        [TestCase("16")]
        public void Create(string expectedFunctionArgsBytesString)
        {
            var functionTx = SendDataAbortTx.Create();

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.False);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }
    }
}