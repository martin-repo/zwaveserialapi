// -------------------------------------------------------------------------------------------------
// <copyright file="SetPromiscuousModeTx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions
{
    internal class SetPromiscuousModeTx : FunctionTx
    {
        private SetPromiscuousModeTx(byte[] functionArgsBytes)
            : base(functionArgsBytes)
        {
        }

        public override bool HasReturnValue => false;

        public static SetPromiscuousModeTx Create(bool enabled)
        {
            var serialCommandBytes = new byte[2];

            serialCommandBytes[0] = (byte)FunctionType.SetPromiscuousMode;
            serialCommandBytes[1] = (byte)(enabled ? 1 : 0);

            return new SetPromiscuousModeTx(serialCommandBytes);
        }
    }
}