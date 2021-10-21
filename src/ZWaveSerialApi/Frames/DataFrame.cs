// -------------------------------------------------------------------------------------------------
// <copyright file="DataFrame.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Frames
{
    using System.Linq;

    using ZWaveSerialApi.Functions;

    public class DataFrame : Frame, IDataFrame
    {
        public DataFrame(byte[] frameBytes)
            : base(frameBytes)
        {
            Length = Bytes[1];
            Type = (FrameType)Bytes[2];
            Checksum = Bytes[^1];
            IsChecksumValid = CalculateChecksum(frameBytes) == Checksum;

            SerialCommandBytes = Bytes[3..^1];
            FunctionType = (FunctionType)SerialCommandBytes[0];
        }

        public byte Checksum { get; }

        public FunctionType FunctionType { get; }

        public bool IsChecksumValid { get; }

        public byte Length { get; }

        public byte[] SerialCommandBytes { get; }

        public FrameType Type { get; }

        public static DataFrame Create(FrameType frameType, byte[] functionArgsBytes)
        {
            var frameBytes = new byte[functionArgsBytes.Length + 4];

            frameBytes[0] = (byte)FramePreamble.StartOfFrame;
            frameBytes[1] = (byte)(frameBytes.Length - 2);
            frameBytes[2] = (byte)frameType;
            functionArgsBytes.CopyTo(frameBytes, 3);
            frameBytes[^1] = CalculateChecksum(frameBytes);

            return new DataFrame(frameBytes);
        }

        private static byte CalculateChecksum(byte[] frameBytes)
        {
            return frameBytes[1..^1].Aggregate(byte.MaxValue, (checksum, value) => (byte)(checksum ^ value));
        }
    }
}