// -------------------------------------------------------------------------------------------------
// <copyright file="SerialApiGetInitDataRx.cs" company="Martin Karlsson">
//   Copyright (c) Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace ZWaveSerialApi.Functions.SerialApi
{
    using System.Collections;
    using System.Collections.Generic;

    using ZWaveSerialApi.Utilities;

    internal class SerialApiGetInitDataRx : FunctionRx
    {
        public SerialApiGetInitDataRx(byte[] returnValueBytes)
            : base(FunctionType.SerialApiGetInitData, returnValueBytes)
        {
            var serialApiVersion = returnValueBytes[1];
            var (isControllerApi, isTimerFunctionsSupported, isPrimaryController, isSuc) = DeconstructMetadataByte(returnValueBytes[2]);
            IsStaticUpdateController = isSuc;

            var nodeBytesLength = returnValueBytes[3];
            var nodeBytes = returnValueBytes[4..(4 + nodeBytesLength)];
            DeviceNodeIds = GetDeviceNodeIds(nodeBytes).ToArray();

            var chipType = returnValueBytes[^2];
            var chipVersion = returnValueBytes[^1];
        }

        public byte[] DeviceNodeIds { get; }

        public bool IsStaticUpdateController { get; set; }

        private (bool IsControllerApi, bool IsTimerFunctionsSupported, bool IsPrimaryController, bool IsSuc) DeconstructMetadataByte(
            byte metadataByte)
        {
            //  7 6 5 4 3 2 1 0
            // |-------| reserved
            //         |-| is static update controller
            //           |-| is primary controller
            //             |-| is timer functions supported
            //               |-| is controller api (vs slave api)
            var isControllerApi = BitHelper.IsSet(metadataByte, 0);
            var isTimerFunctionsSupported = BitHelper.IsSet(metadataByte, 1);
            var isPrimaryController = BitHelper.IsSet(metadataByte, 2);
            var isSuc = BitHelper.IsSet(metadataByte, 3);

            return (isControllerApi, isTimerFunctionsSupported, isPrimaryController, isSuc);
        }

        private List<byte> GetDeviceNodeIds(byte[] nodeBytes)
        {
            var deviceNodeIds = new List<byte>();

            var nodeBits = new BitArray(nodeBytes);
            for (var nodeIndex = 0; nodeIndex < nodeBits.Length; nodeIndex++)
            {
                if (!nodeBits[nodeIndex])
                {
                    continue;
                }

                var nodeId = (byte)(nodeIndex + 1);
                deviceNodeIds.Add(nodeId);
            }

            return deviceNodeIds;
        }
    }
}