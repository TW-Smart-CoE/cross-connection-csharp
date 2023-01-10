using System;
using System.Text;

namespace CConn
{
    public static class ByteUtils
    {
        public static void PutUShort(byte[] bytes, int index, ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                bytes[index] = (byte)(value >> 8);
                bytes[index + 1] = (byte)(value);
            }
            else
            {
                bytes[index] = (byte)(value);
                bytes[index + 1] = (byte)(value >> 8);
            }
        }

        public static ushort GetUShort(byte[] bytes, int index)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)(((bytes[index] << 8) | bytes[index + 1] & 0xff));
            }
            else
            {
                return (ushort)(((bytes[index + 1] << 8) | bytes[index] & 0xff));
            }
        }

        public static void PutUInt(byte[] bytes, int index, uint value) {
            if (BitConverter.IsLittleEndian) {
                bytes[index] = (byte) (value >> 24);
                bytes[index + 1] = (byte) (value >> 16);
                bytes[index + 2] = (byte) (value >> 8);
                bytes[index + 3] = (byte) (value);
            } else {
                bytes[index] = (byte) (value);
                bytes[index + 1] = (byte) (value >> 8);
                bytes[index + 2] = (byte) (value >> 16);
                bytes[index + 3] = (byte) (value >> 24);
            }
        }

        public static uint GetUInt(byte[] bytes, int index) {
            if (BitConverter.IsLittleEndian) {
                return (uint)(((bytes[index] & 0xff) << 24)
                        | ((bytes[index + 1] & 0xff) << 16)
                        | ((bytes[index + 2] & 0xff) << 8)
                        | ((bytes[index + 3] & 0xff)));
            } else {
                return (uint)(((bytes[index] & 0xff))
                    | ((bytes[index + 1] & 0xff) << 8)
                    | ((bytes[index + 2] & 0xff) << 16)
                    | ((bytes[index + 3] & 0xff)) << 24);
            }
        }
    }

    public static class ByteExtension
    {
        public static byte GetByte(this byte[] buffer, int index)
        {
            return buffer[index];
        }

        public static void PutByte(this byte[] buffer, int index, byte value)
        {
            buffer[index] = value;
        }

        public static ushort GetUShort(this byte[] buffer, int index)
        {
            return ByteUtils.GetUShort(buffer, index);
        }

        public static void PutUShort(this byte[] buffer, int index, ushort value)
        {
            ByteUtils.PutUShort(buffer, index, value);
        }

        public static uint GetUInt(this byte[] buffer, int index)
        {
            return ByteUtils.GetUInt(buffer, index);
        }

        public static void PutUInt(this byte[] buffer, int index, uint value)
        {
            ByteUtils.PutUInt(buffer, index, value);
        }

        public static string ToHexString(this byte[] buffer, int index, int len)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = index; i < len - index; i++)
            {
                sb.Append(string.Format("{0:X2}", buffer[i]));
            }

            return sb.ToString();
        }

        public static string ToHexString(this byte[] buffer, int index = 0)
        {
            return buffer.ToHexString(index, buffer.Length);
        }
    }
}
