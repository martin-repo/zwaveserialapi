// -------------------------------------------------------------------------------------------------
// <copyright file="EndianHelperTests.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Test.Utilities
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using ZWaveSerialApi.Utilities;

    public class EndianHelperTests
    {
        [TestCase(197121, 3, "03-02-01")]
        [TestCase(67305985, 4, "04-03-02-01")]
        public void GetBytes_WhenInt32(int value, int byteLength, string expectedByteArrayString)
        {
            var byteArray = EndianHelper.GetBytes(value, byteLength);
            var byteArrayString = BitConverter.ToString(byteArray);

            Assert.That(byteArrayString, Is.EqualTo(expectedByteArrayString));
        }

        [TestCase((ushort)513, "02-01")]
        public void GetBytes_WhenUInt16(ushort value, string expectedByteArrayString)
        {
            var byteArray = EndianHelper.GetBytes(value);
            var byteArrayString = BitConverter.ToString(byteArray);

            Assert.That(byteArrayString, Is.EqualTo(expectedByteArrayString));
        }

        [TestCase("02-01", 513)]
        [TestCase("7F-FF", 32767)]
        [TestCase("80-00", -32768)]
        [TestCase("80-01", -32767)]
        [TestCase("FF-FF", -1)]
        public void ToInt16(string byteArrayString, int expectedValue)
        {
            var byteArray = byteArrayString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var value = EndianHelper.ToInt16(byteArray);

            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [TestCase("03-02-01", 197121)]
        [TestCase("04-03-02-01", 67305985)]
        public void ToInt32(string byteArrayString, int expectedValue)
        {
            var byteArray = byteArrayString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var value = EndianHelper.ToInt32(byteArray);

            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [TestCase("02-01", 513)]
        [TestCase("7F-FF", 32767)]
        [TestCase("80-00", 32768)]
        [TestCase("80-01", 32769)]
        [TestCase("FF-FF", 65535)]
        public void ToUInt16(string byteArrayString, int expectedValue)
        {
            var byteArray = byteArrayString.Split('-').Select(byteString => Convert.ToByte(byteString, 16)).ToArray();

            var value = EndianHelper.ToUInt16(byteArray);

            Assert.That(value, Is.EqualTo(expectedValue));
        }
    }
}