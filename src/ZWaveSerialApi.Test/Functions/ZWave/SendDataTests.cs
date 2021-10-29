// -------------------------------------------------------------------------------------------------
// <copyright file="SendDataTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave.SendData;

    internal class SendDataTests
    {
        private const TransmitOption TransmitOptions = TransmitOption.Ack | TransmitOption.AutoRoute | TransmitOption.Explore;

        [TestCase(1, "AA-BB-CC", TransmitOptions, 1, "13-01-03-AA-BB-CC-25-01")]
        [TestCase(1, "AA-BB-CC-DD", TransmitOptions, 1, "13-01-04-AA-BB-CC-DD-25-01")]
        [TestCase(1, "DD-EE-FF", TransmitOptions, 1, "13-01-03-DD-EE-FF-25-01")]
        [TestCase(2, "AA-BB-CC", TransmitOptions, 1, "13-02-03-AA-BB-CC-25-01")]
        [TestCase(1, "AA-BB-CC", TransmitOptions, 2, "13-01-03-AA-BB-CC-25-02")]
        [TestCase(1, "AA-BB-CC", TransmitOption.Ack, 1, "13-01-03-AA-BB-CC-01-01")]
        public void Create(
            byte destinationNodeId,
            string commandClassBytesString,
            TransmitOption transmitOptions,
            byte completedFuncId,
            string expectedFunctionArgsBytesString)
        {
            var commandClassBytes = commandClassBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = SendDataTx.Create(destinationNodeId, commandClassBytes, transmitOptions, completedFuncId);

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase(
            1,
            "AA-BB-CC",
            TransmitOptions,
            1,
            "13-01",
            true)]
        [TestCase(
            2,
            "AA-BB-CC",
            TransmitOptions,
            2,
            "13-01",
            true)]
        [TestCase(
            1,
            "AA-BB-CC",
            TransmitOptions,
            1,
            "13-00",
            false)]
        public void CreateReturnValue(
            byte destinationNodeId,
            string commandClassBytesString,
            TransmitOption transmitOptions,
            byte completedFuncId,
            string returnValueBytesString,
            bool expectedSuccess)
        {
            var commandClassBytes = commandClassBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = SendDataTx.Create(destinationNodeId, commandClassBytes, transmitOptions, completedFuncId);
            var returnValue = (SendDataRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(returnValue.Success, Is.EqualTo(expectedSuccess));
        }

        [TestCase(
            1,
            "AA-BB-CC",
            TransmitOptions,
            1,
            "13-01",
            true)]
        [TestCase(
            1,
            "AA-BB-CC",
            TransmitOptions,
            1,
            "13-00",
            false)]
        [TestCase(
            1,
            "AA-BB-CC",
            TransmitOptions,
            2,
            "13-02",
            true)]
        public void IsValidReturnValue(
            byte destinationNodeId,
            string commandClassBytesString,
            TransmitOption transmitOptions,
            byte callbackFuncId,
            string returnValueBytesString,
            bool expectedIsValidReturnValue)
        {
            var commandClassBytes = commandClassBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = SendDataTx.Create(destinationNodeId, commandClassBytes, transmitOptions, callbackFuncId);
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.EqualTo(expectedIsValidReturnValue));
        }
    }
}