// -------------------------------------------------------------------------------------------------
// <copyright file="Crc16EncapCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.TransportEncapsulation.Crc16Encap
{
    using System;
    using System.Collections.Generic;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class Crc16EncapCommandClass : CommandClass
    {
        private readonly ILogger _logger;

        public Crc16EncapCommandClass(ILogger logger, IZWaveSerialClient client)
            : base(CommandClassType.Crc16Encap, client)
        {
            _logger = logger.ForContext<Crc16EncapCommandClass>().ForContext(Constants.ClassName, GetType().Name);
        }

        internal override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (Crc16EncapCommand)commandClassBytes[1];
            if (command != Crc16EncapCommand.Crc16Encap)
            {
                _logger.Error("Unsupported command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                return;
            }

            _logger.InboundCommand(sourceNodeId, commandClassBytes, Type, command);

            var calculatedChecksum = ZW_CheckCrc16(0x1D0F, commandClassBytes[..^2]);
            var checksum = EndianHelper.ToUInt16(commandClassBytes[^2..]);
            if (calculatedChecksum != checksum)
            {
                var calculatedChecksumBytes = BitConverter.GetBytes(calculatedChecksum);
                _logger.Warning(
                    "Invalid checksum for {Bytes}. Expected {ExpectedBytes} but was {ActualBytes}.",
                    BitConverter.ToString(commandClassBytes),
                    BitConverter.ToString(calculatedChecksumBytes),
                    BitConverter.ToString(commandClassBytes[^2..]));
                return;
            }

            var encapCommandClassBytes = commandClassBytes[2..^2];
            var encapCommandClassType = (CommandClassType)encapCommandClassBytes[0];
            var encapCommandClass = Client.GetCommandClass(encapCommandClassType);
            encapCommandClass.ProcessCommandClassBytes(sourceNodeId, encapCommandClassBytes);
        }

        private static ushort ZW_CheckCrc16(ushort crc, IEnumerable<byte> bytes)
        {
            const ushort Poly = 0x1021;

            foreach (var @byte in bytes)
            {
                for (var bitMask = 0x80; bitMask != 0; bitMask >>= 1)
                {
                    // Align test bit with next bit of the message byte, starting with msb.
                    var newBit = ((@byte & bitMask) != 0) ^ ((crc & 0x8000) != 0);
                    crc <<= 1;
                    if (newBit)
                    {
                        crc ^= Poly;
                    }
                }
            }

            return crc;
        }
    }
}