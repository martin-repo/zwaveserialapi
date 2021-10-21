// -------------------------------------------------------------------------------------------------
// <copyright file="Crc16EncapCommandClass.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.CommandClasses.TransportEncapsulation.Crc16Encap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Serilog;

    using ZWaveSerialApi.Utilities;

    public class Crc16EncapCommandClass : CommandClass
    {
        private readonly IZWaveSerialClient _client;
        private readonly ILogger _logger;

        public Crc16EncapCommandClass(ILogger logger, IZWaveSerialClient client)
        {
            _logger = logger.ForContext("ClassName", GetType().Name);
            _client = client;
        }

        public override void ProcessCommandClassBytes(byte sourceNodeId, byte[] commandClassBytes)
        {
            var command = (Crc16EncapCommand)commandClassBytes[1];
            if (command != Crc16EncapCommand.Crc16Encap)
            {
                _logger.Error("Unsupported CRC 16 encap command {Command}", BitConverter.ToString(commandClassBytes, 1, 1));
                return;
            }

            var calculatedChecksum = ZW_CheckCrc16(0x1D0F, commandClassBytes[..^2]);
            var checksum = EndianHelper.ToInt16(commandClassBytes[^2..]);
            if (calculatedChecksum != checksum)
            {
                _logger.Warning("Invalid CRC 16 encap checksum {Bytes}", BitConverter.ToString(commandClassBytes));
                return;
            }

            var encapCommandClassBytes = commandClassBytes[2..];
            var encapCommandClassType = (CommandClassType)encapCommandClassBytes[0];
            var encapCommandClass = _client.GetCommandClass(encapCommandClassType);
            encapCommandClass.ProcessCommandClassBytes(sourceNodeId, encapCommandClassBytes);
        }

        private static short ZW_CheckCrc16(short crc, IEnumerable<byte> bytes)
        {
            const short Poly = 0x1021;

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