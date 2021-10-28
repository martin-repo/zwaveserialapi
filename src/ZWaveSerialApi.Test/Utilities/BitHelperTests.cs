// -------------------------------------------------------------------------------------------------
// <copyright file="BitHelperTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Utilities
{
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Utilities;

    public class BitHelperTests
    {
        [TestCase(0x00, "00000000")]
        [TestCase(0x01, "00000001")]
        [TestCase(0x02, "00000010")]
        [TestCase(0x04, "00000100")]
        [TestCase(0x08, "00001000")]
        [TestCase(0x10, "00010000")]
        [TestCase(0x20, "00100000")]
        [TestCase(0x40, "01000000")]
        [TestCase(0x80, "10000000")]
        [TestCase(0xAA, "10101010")]
        [TestCase(0xF0, "11110000")]
        [TestCase(0xFF, "11111111")]
        public void IsSet(byte @byte, string expectedIsSetString)
        {
            var expectedIsSet = expectedIsSetString.ToCharArray().Reverse().Select(@char => @char != '0').ToList();

            for (var index = 0; index < 8; index++)
            {
                var isSet = BitHelper.IsSet(@byte, index);
                Assert.That(isSet, Is.EqualTo(expectedIsSet[index]));
            }
        }
    }
}