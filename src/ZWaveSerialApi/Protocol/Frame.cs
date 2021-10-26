// -------------------------------------------------------------------------------------------------
// <copyright file="Frame.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Protocol
{
    using System.Linq;

    using ZWaveSerialApi.Functions;

    internal class Frame
    {
        public Frame(byte[] frameBytes)
        {
            FrameBytes = frameBytes;

            Length = FrameBytes[1];
            Type = (FrameType)FrameBytes[2];
            Checksum = FrameBytes[^1];
            IsChecksumValid = CalculateChecksum(frameBytes) == Checksum;

            SerialCommandBytes = FrameBytes[3..^1];
            FunctionType = (FunctionType)SerialCommandBytes[0];
        }

        public byte Checksum { get; }

        public byte[] FrameBytes { get; }

        public FunctionType FunctionType { get; }

        public bool IsChecksumValid { get; }

        public byte Length { get; }

        public byte[] SerialCommandBytes { get; }

        public FrameType Type { get; }

        public static Frame Create(FrameType frameType, byte[] functionArgsBytes)
        {
            var frameBytes = new byte[functionArgsBytes.Length + 4];

            frameBytes[0] = (byte)MessageType.StartOfFrame;
            frameBytes[1] = (byte)(frameBytes.Length - 2);
            frameBytes[2] = (byte)frameType;
            functionArgsBytes.CopyTo(frameBytes, 3);
            frameBytes[^1] = CalculateChecksum(frameBytes);

            return new Frame(frameBytes);
        }

        private static byte CalculateChecksum(byte[] frameBytes)
        {
            return frameBytes[1..^1].Aggregate(byte.MaxValue, (checksum, value) => (byte)(checksum ^ value));
        }
    }
}