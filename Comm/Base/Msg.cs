using System;

namespace CConn
{
    public enum MsgType
    {
        PUBLISH = 0,
        SUBSCRIBE = 1,
        UNSUBSCRIBE = 2
    }

    public struct MsgHeader
    {
        public uint flag;
        public byte type;
        public byte method;
        public ushort topicLen;
        public ushort dataLen;
        public uint checkSum;
        public ushort reserved;
    }

    public class Msg
    {
        public MsgHeader header;
        public byte[] topic;
        public byte[] data;
    }

    public static class MsgHeaderUtils
    {
        public static void FromBytes(ref MsgHeader msgHeader, byte[] bytes)
        {
            if (bytes.Length < Constants.MSG_HEADER_LEN)
            {
                throw new Exception(string.Format("byteArray size {0} smaller than MSG_HEADER_LEN {1}", bytes.Length, Constants.MSG_HEADER_LEN));
            }

            var index = 0;
            msgHeader.flag = bytes.GetUInt(index);
            index += sizeof(uint);

            msgHeader.type = bytes.GetByte(index);
            index += sizeof(byte);

            msgHeader.method = bytes.GetByte(index);
            index += sizeof(byte);

            msgHeader.topicLen = bytes.GetUShort(index);
            index += sizeof(ushort);

            msgHeader.dataLen = bytes.GetUShort(index);
            index += sizeof(ushort);

            msgHeader.checkSum = bytes.GetUInt(index);
            index += sizeof(uint);

            msgHeader.reserved = bytes.GetUShort(index);
            index += sizeof(ushort);
        }

        public static byte[] ToBytes(ref MsgHeader msgHeader, byte[] dstBytes = null)
        {
            var bytes = dstBytes != null ? dstBytes : new byte[Constants.MSG_HEADER_LEN];

            var index = 0;

            bytes.PutUInt(index, Constants.MSG_FLAG);
            index += sizeof(uint);

            bytes.PutByte(index, msgHeader.type);
            index += sizeof(byte);

            bytes.PutByte(index, msgHeader.method);
            index += sizeof(byte);

            bytes.PutUShort(index, msgHeader.topicLen);
            index += sizeof(ushort);

            bytes.PutUShort(index, msgHeader.dataLen);
            index += sizeof(ushort);

            bytes.PutUInt(index, msgHeader.checkSum);
            index += sizeof(uint);

            bytes.PutUShort(index, msgHeader.reserved);
            index += sizeof(ushort);

            return bytes;
        }
    }

    public static class MsgExtension
    {
        public static int Length(this Msg msg)
        {
            return Constants.MSG_HEADER_LEN + msg.topic.Length + msg.data.Length;
        }

        public static byte[] ToBytes(this Msg msg)
        {
            var bytes = new byte[Constants.MSG_HEADER_LEN + msg.header.topicLen + msg.header.dataLen];
            var index = 0;

            MsgHeaderUtils.ToBytes(ref msg.header, bytes);
            index += Constants.MSG_HEADER_LEN;

            Buffer.BlockCopy(msg.topic, 0, bytes, index, msg.topic.Length);
            index += msg.topic.Length;

            if (msg.header.dataLen > 0)
            {
                Buffer.BlockCopy(msg.data, 0, bytes, index, msg.header.dataLen);
                index += msg.data.Length;
            }

            return bytes;
        }

        public static Msg CopyOf(this Msg msg)
        {
            Msg copy = new Msg();
            copy.header = msg.header;

            copy.topic = new byte[msg.topic.Length];
            Buffer.BlockCopy(msg.topic, 0, copy.topic, 0, msg.topic.Length);

            if (msg.header.dataLen > 0)
            {
                copy.data = new byte[msg.header.dataLen];
                Buffer.BlockCopy(msg.data, 0, copy.data, 0, msg.header.dataLen);
            }

            return copy;
        }

        public static uint CalcCheckSum(this Msg msg)
        {
            uint checkSum = 0;
            MsgHeader headerCopy = msg.header;
            headerCopy.checkSum = 0;
            byte[] headerBytes = MsgHeaderUtils.ToBytes(ref headerCopy);

            for (int i = 0; i < headerBytes.Length; i++)
            {
                checkSum += headerBytes[i];
            }

            if (msg.topic != null)
            {
                for (int i = 0; i < msg.topic.Length; i++)
                {
                    checkSum += msg.topic[i];
                }
            }

            if (msg.data != null)
            {
                for (int i = 0; i < msg.data.Length; i++)
                {
                    checkSum += msg.data[i];
                }
            }

            return checkSum;
        }
    }
}
