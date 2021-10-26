// -------------------------------------------------------------------------------------------------
// <copyright file="TypeLibraryTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Functions.ZWave
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Functions.ZWave.TypeLibrary;

    public class TypeLibraryTests
    {
        [TestCase("BD")]
        public void Create(string expectedFunctionArgsBytesString)
        {
            var functionTx = TypeLibraryTx.Create();

            var functionArgsBytesString = BitConverter.ToString(functionTx.FunctionArgsBytes);

            Assert.That(functionTx.HasReturnValue, Is.True);
            Assert.That(functionArgsBytesString, Is.EqualTo(expectedFunctionArgsBytesString));
        }

        [TestCase("BD-01", 1)]
        [TestCase("BD-02", 2)]
        public void CreateReturnValue(string returnValueBytesString, byte expectedLibraryType)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = TypeLibraryTx.Create();
            var returnValue = (TypeLibraryRx)functionTx.CreateReturnValue(returnValueBytes);

            Assert.That(returnValue.LibraryType, Is.EqualTo(expectedLibraryType));
        }

        [TestCase("BD-01")]
        [TestCase("BD-02")]
        public void IsValidReturnValue(string returnValueBytesString)
        {
            var returnValueBytes = returnValueBytesString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var functionTx = TypeLibraryTx.Create();
            var isValidReturnValue = functionTx.IsValidReturnValue(returnValueBytes);

            Assert.That(isValidReturnValue, Is.True);
        }
    }
}