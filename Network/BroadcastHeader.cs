using System;

namespace CConn
{
    public struct BroadcastHeader 
    {
        public uint flag;
        public uint ip;
        public ushort port;
        public ushort dataLen;
    }

    public static class BroadcastHeaderUtils
    {
        public static void FromBytes(ref BroadcastHeader broadcastHeader, byte[] bytes)
        {
            if (bytes.Length < Constants.BROADCAST_MSG_HEADER_LEN)
            {
                throw new Exception(string.Format("byteArray size {0} smaller than BROADCAST_MSG_HEADER_LEN {1}", bytes.Length, Constants.BROADCAST_MSG_HEADER_LEN));
            }

            var index = 0;
            broadcastHeader.flag = bytes.GetUInt(index);
            index += sizeof(uint);

            broadcastHeader.ip = bytes.GetUInt(index);
            index += sizeof(uint);

            broadcastHeader.port = bytes.GetUShort(index);
            index += sizeof(ushort);

            broadcastHeader.dataLen = bytes.GetUShort(index);
            index += sizeof(ushort);
        }

        public static byte[] ToBytes(ref BroadcastHeader broadcastHeader, byte[] dstBytes = null)
        {
            var bytes = dstBytes != null ? dstBytes : new byte[Constants.BROADCAST_MSG_HEADER_LEN];

            var index = 0;

            bytes.PutUInt(index, broadcastHeader.flag);
            index += sizeof(uint);

            bytes.PutUInt(index, broadcastHeader.ip);
            index += sizeof(uint);

            bytes.PutUShort(index, broadcastHeader.port);
            index += sizeof(ushort);

            bytes.PutUShort(index, broadcastHeader.dataLen);
            index += sizeof(ushort);

            return bytes;
        }
    }
}
