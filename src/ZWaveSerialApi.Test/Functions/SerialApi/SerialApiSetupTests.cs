// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiSetupTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.SerialApi
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.SerialApi;

    public class SerialApiSetupTests
    {
        [TestCase(true, "0B-02-01")]
        [TestCase(false, "0B-02-00")]
        public void Create(bool enableStatusReport, string expectedFunctionArgsBytesString)
        {
            var functionTx = SerialApiSetupTx.Create(enableStatusReport);

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase(true, "0B-00-01", "01")]
        [TestCase(true, "0B-FF-01", "01")]
        [TestCase(true, "0B-00-01-02", "01-02")]
        public void CreateReturnValue(bool enableStatusReport, string returnValueBytesString, string expectedResponseBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = SerialApiSetupTx.Create(enableStatusReport);
            var returnValue = (SerialApiSetupRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(BitConverter.ToString(returnValue.Response), Is.EqualTo(expectedResponseBytesString));
        }

        [TestCase(true, "0B-00-01")]
        [TestCase(true, "0B-FF-01")]
        public void IsValidReturnValue(bool enableStatusReport, string returnValueBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = SerialApiSetupTx.Create(enableStatusReport);
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.True);
        }
    }
}