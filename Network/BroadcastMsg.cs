using System;

namespace CConn
{
    public struct BroadcastMsg 
    {
        public uint flag;
        public uint ip;
        public ushort port;
        public ushort reserved;
    }

    public static class BroadcastMsgUtils
    {
        public static void FromBytes(ref BroadcastMsg broadcastMsg, byte[] bytes)
        {
            if (bytes.Length < Constants.BROADCAST_MSG_HEADER_LEN)
            {
                throw new Exception(string.Format("byteArray size {0} smaller than BROADCAST_MSG_HEADER_LEN {1}", bytes.Length, Constants.BROADCAST_MSG_HEADER_LEN));
            }

            var index = 0;
            broadcastMsg.flag = bytes.GetUInt(index);
            index += sizeof(uint);

            broadcastMsg.ip = bytes.GetUInt(index);
            index += sizeof(uint);

            broadcastMsg.port = bytes.GetUShort(index);
            index += sizeof(ushort);

            broadcastMsg.reserved = bytes.GetUShort(index);
            index += sizeof(ushort);
        }

        public static byte[] ToBytes(ref BroadcastMsg broadcastMsg, byte[] dstBytes = null)
        {
            var bytes = dstBytes != null ? dstBytes : new byte[Constants.BROADCAST_MSG_HEADER_LEN];

            var index = 0;

            bytes.PutUInt(index, broadcastMsg.flag);
            index += sizeof(uint);

            bytes.PutUInt(index, broadcastMsg.ip);
            index += sizeof(uint);

            bytes.PutUShort(index, broadcastMsg.port);
            index += sizeof(ushort);

            bytes.PutUShort(index, broadcastMsg.reserved);
            index += sizeof(ushort);

            return bytes;
        }
    }
}
