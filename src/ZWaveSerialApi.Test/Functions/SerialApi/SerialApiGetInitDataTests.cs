// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiGetInitDataTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.SerialApi
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.SerialApi;

    public class SerialApiGetInitDataTests
    {
        [TestCase("02")]
        public void Create(string expectedFunctionArgsBytesString)
        {
            var functionTx = SerialApiGetInitDataTx.Create();

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase("02-FF-08-02-01-00-FF-FF", true, "01")]
        [TestCase("02-FF-07-02-01-00-FF-FF", false, "01")]
        [TestCase("02-FF-08-03-01-00-00-FF-FF", true, "01")]
        [TestCase("02-FF-08-02-05-00-FF-FF", true, "01-03")]
        [TestCase("02-FF-08-02-01-05-FF-FF", true, "01-09-0B")]
        public void CreateReturnValue(string returnValueBytesString, bool expectedIsStaticUpdateController, string expectedDeviceNodeIdBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = SerialApiGetInitDataTx.Create();
            var returnValue = (SerialApiGetInitDataRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(returnValue.IsStaticUpdateController, Is.EqualTo(expectedIsStaticUpdateController));
            Assert.That(BitConverter.ToString(returnValue.DeviceNodeIds), Is.EqualTo(expectedDeviceNodeIdBytesString));
        }

        [TestCase("02-FF-06-02-01-00-FF-FF")]
        public void IsValidReturnValue(string returnValueBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = SerialApiGetInitDataTx.Create();
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.True);
        }
    }
}