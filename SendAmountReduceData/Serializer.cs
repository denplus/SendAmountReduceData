using System;
using System.Linq;

namespace SendAmountReduceData
{
    public static class Serializer
    {
        private const int Mantissa = 2;

        public static byte[] SerializeToBytes(PlayerPosition pp)
        {
            // convert position from short to byte array (if can) or from float to byte array
            byte[] numberBytesX = GetBytes(pp.X);
            byte[] numberBytesY = GetBytes(pp.Y);
            byte[] numberBytesZ = GetBytes(pp.Z);

            byte[] serializeToBytes = numberBytesX.Concat(numberBytesY).Concat(numberBytesZ).ToArray();

            /* if conversion from short to byte array is successful then set bit to 1
               if not set to 0. 
               Example: 101. Here X and Z value were flagged as they were successfully 
               converted from short to byte array, and Y overfilled Min..Max value of short.
               100 - X, 010 - Y, 001 - Z, 011 - YZ
            */

            byte byteFlag = 0;
            SetBitAsFlag(numberBytesX.Length, 4, ref byteFlag);
            SetBitAsFlag(numberBytesY.Length, 2, ref byteFlag);
            SetBitAsFlag(numberBytesZ.Length, 1, ref byteFlag);
            if (byteFlag != 0)
            {
                // add flag byte to end of data set
                serializeToBytes = serializeToBytes.Concat(new[] {byteFlag}).ToArray();
            }

            return serializeToBytes;
        }

        public static PlayerPosition DeserializeFromBytes(byte[] data)
        {
            byte[] flag = null;
            if (data.Length % 2 != 0)
            {
                // separate data from flag byte
                flag = data.Skip(data.Length - 1).ToArray();
                data = data.Take(data.Length - 1).ToArray();
            }

            // put back the dot for float
            float divider = 1f;
            for (int i = 0; i < Mantissa; i++)
                divider *= 10f;

            // check bit for each axes
            bool isBitOneSetX = IsBitSet(flag, 2);
            bool isBitOneSetY = IsBitSet(flag, 1);
            bool isBitOneSetZ = IsBitSet(flag, 0);

            float finalX = isBitOneSetX
                ? BitConverter.ToInt16(data, 0) / divider
                : BitConverter.ToSingle(data, 0) / divider;
            float finalY = isBitOneSetY
                ? BitConverter.ToInt16(data, isBitOneSetX ? 2 : 4) / divider
                : BitConverter.ToSingle(data, isBitOneSetX ? 2 : 4) / divider;

            int index = GetIndexForZValue(isBitOneSetX, isBitOneSetY);

            float finalZ = isBitOneSetZ
                ? BitConverter.ToInt16(data, index) / divider
                : BitConverter.ToSingle(data, index) / divider;

            return new PlayerPosition(finalX, finalY, finalZ);
        }

        private static int GetIndexForZValue(bool isBitOneSetX, bool isBitOneSetY)
        {
            int index = 8;
            if (isBitOneSetX)
                index = 2;
            if (isBitOneSetX && isBitOneSetY)
                index = 4;
            if ((isBitOneSetX && !isBitOneSetY) || (!isBitOneSetX && isBitOneSetY))
                index = 6;

            return index;
        }

        private static bool IsBitSet(byte[] value, int index)
        {
            byte bitMask = (byte) (1 << index);
            return value != null && (value[0] & bitMask) != 0;
        }

        private static void SetBitAsFlag(int length, byte flag, ref byte byteFlag)
        {
            // set flag for byte
            if (length == 2)
                byteFlag |= flag;
        }

        private static byte[] GetBytes(float os)
        {
            // multiply for moving dot in float left
            for (int i = 0; i < Mantissa; i++)
                os *= 10f;

            // check for short limits
            if (os > short.MinValue && os < short.MaxValue)
                return BitConverter.GetBytes((short) os);

            return BitConverter.GetBytes(os);
        }
    }
}