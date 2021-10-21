// -------------------------------------------------------------------------------------------------
// <copyright file="EndianHelper.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Utilities
{
    using System;
    using System.Linq;

    public class EndianHelper
    {
        public static byte[] GetBytes(int value, int byteLength = 4)
        {
            switch (byteLength)
            {
                case 3:
                    var intBytes = BitConverter.GetBytes(value).Reverse().ToArray();
                    var subSetBytes = new byte[3];
                    intBytes[1..4].CopyTo(subSetBytes, 0);
                    return subSetBytes;
                case 4:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                default:
                    throw new ArgumentException("Byte length must be 3 or 4.", nameof(byteLength));
            }
        }

        public static short ToInt16(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                throw new ArgumentException("Bytes length must be 2.", nameof(bytes));
            }

            return BitConverter.ToInt16(bytes.Reverse().ToArray());
        }

        public static int ToInt32(byte[] bytes)
        {
            switch (bytes.Length)
            {
                case 3:
                    var intBytes = new byte[4];
                    bytes.CopyTo(intBytes, 1);
                    return BitConverter.ToInt32(intBytes.Reverse().ToArray());
                case 4:
                    return BitConverter.ToInt32(bytes.Reverse().ToArray());
                default:
                    throw new ArgumentException("Bytes must be 3 or 4 in length.", nameof(bytes));
            }
        }
    }
}