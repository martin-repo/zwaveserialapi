// -------------------------------------------------------------------------------------------------
// <copyright file="BitHelper.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Utilities
{
    using System;

    public class BitHelper
    {
        /// <summary>
        ///     0 0 0 0 0 0 0 1
        /// </summary>
        public static int Bit0Mask = GetBitMask(1);

        /// <summary>
        ///     0 0 0 0 0 0 1 1
        /// </summary>
        public static int Bit1Mask = GetBitMask(2);

        /// <summary>
        ///     0 0 0 0 0 1 1 1
        /// </summary>
        public static int Bit2Mask = GetBitMask(3);

        /// <summary>
        ///     0 0 0 0 1 1 1 1
        /// </summary>
        public static int Bit3Mask = GetBitMask(4);

        /// <summary>
        ///     0 0 0 1 1 1 1 1
        /// </summary>
        public static int Bit4Mask = GetBitMask(5);

        /// <summary>
        ///     0 0 1 1 1 1 1 1
        /// </summary>
        public static int Bit5Mask = GetBitMask(6);

        /// <summary>
        ///     0 1 1 1 1 1 1 1
        /// </summary>
        public static int Bit6Mask = GetBitMask(7);

        public static bool IsSet(byte @byte, int index)
        {
            return (@byte & (1 << index)) != 0;
        }

        private static int GetBitMask(int bitCount)
        {
            return (int)Math.Pow(2, bitCount) - 1;
        }
    }
}