// -------------------------------------------------------------------------------------------------
// <copyright file="TransmitException.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi
{
    using System;

    public class TransmitException : Exception
    {
        public TransmitException(string message)
            : base(message)
        {
        }
    }
}