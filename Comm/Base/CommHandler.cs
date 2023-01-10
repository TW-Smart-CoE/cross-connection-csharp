using System;
using System.IO;

namespace CConn
{
    internal delegate void OnMsgArrivedListener(Msg msg);
    internal delegate void OnCommCloseListener(CommHandler commHandler, bool isForce);
    internal delegate void OnConnectionStateListener(ConnectionState connectionState, Exception e);

    internal class CommHandler
    {
        private const int MSG_COMPLETENESS_NONE = 0;
        private const int MSG_COMPLETENESS_FLAG = 1;
        private const int MSG_COMPLETENESS_HEADER = 2;

        private bool isClient;
        private IComm comm;
        private ILogger logger;
        private OnCommCloseListener onCommCloseListener;
        private OnMsgArrivedListener onMsgArrivedListener;
        private OnConnectionStateListener onConnectionStateListener;
        private Stream stream;
        private bool isClose = false;
        private byte[] buffer = null;
        private int bufferDataStartOffset = 0;
        private int bufferDataLen = 0;
        private int msgCompleteness = MSG_COMPLETENESS_NONE;
        private MsgHeader currentHeader = new MsgHeader();
        private Msg currentMsg = new Msg();
        private int recvBufferSize = Constants.DEFAULT_RECV_BUFFER_SIZE;

        public CommHandler(
                bool isClient,
                IComm comm,
                ILogger logger,
                int recvBufferSize = Constants.DEFAULT_RECV_BUFFER_SIZE,
                OnCommCloseListener onCommCloseListener = null,
                OnMsgArrivedListener onMsgArrivedListener = null,
                OnConnectionStateListener onConnectionStateListener = null
                )
        {
            this.isClient = isClient;
            this.comm = comm;
            this.logger = logger;
            this.onCommCloseListener = onCommCloseListener;
            this.onMsgArrivedListener = onMsgArrivedListener;
            this.onConnectionStateListener = onConnectionStateListener;
            this.recvBufferSize = recvBufferSize;
            this.buffer = new byte[recvBufferSize];
        }

        internal void SetOnCommCloseListener(OnCommCloseListener onCommCloseListener) 
        {
            this.onCommCloseListener = onCommCloseListener;
        }

        internal void SetOnMsgArrivedListener(OnMsgArrivedListener onMsgArrivedListener)
        {
            this.onMsgArrivedListener = onMsgArrivedListener;
        }

        internal void SetOnConnectionStateListener(OnConnectionStateListener onConnectionStateListener)
        {
            this.onConnectionStateListener = onConnectionStateListener;
        }

        public void Run()
        {
            if (isClient)
            {
                try
                {
                    ConnectionStateChange(ConnectionState.CONNECTING);
                    comm.Connect();
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Connect failed: {0}", e.Message));
                    Close(false);

                    return;
                } 
            }

            try
            {
                this.stream = comm.GetStream();
            }
            catch (Exception e)
            {
                logger.Error(string.Format("GetStream failed: {0}", e.Message));
                Close(false);

                return;
            }

            ConnectionStateChange(ConnectionState.CONNECTED);

            isClose = false;
            while (!isClose)
            {
                var msg = ReadMsgFlagFromBuffer();
                if (msg == null)
                {
                    logger.Warn("Connection is lost");
                    Close(false);
                    isClose = true;
                }
                else
                {
                    if (msg.CalcCheckSum() == msg.header.checkSum)
                    {
                        MsgArrive(msg);
                    }

                    msgCompleteness = MSG_COMPLETENESS_NONE;
                }
            }
        }

        public void Send(Msg msg)
        {
            if (stream == null)
            {
                return;
            }

            msg.header.checkSum = msg.CalcCheckSum();
            byte[] bytes = msg.ToBytes();
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public void Close(bool isForce)
        {
            if (isForce)
            {
                this.onMsgArrivedListener = null;
                this.onCommCloseListener = null;
            }

            isClose = true;

            try
            {
                comm.Close();
                ConnectionStateChange(ConnectionState.DISCONNECTED);
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Could not close the connection: {0}", e.Message));
                ConnectionStateChange(ConnectionState.DISCONNECTED, e);
            }
            finally
            {
                CommClose(isForce);
            }
        }

        private Msg ReadMsgFlagFromBuffer()
        {
            var found = false;
            for (int i = bufferDataStartOffset; i < bufferDataStartOffset + bufferDataLen - sizeof(int); i++)
            {
                if (buffer.GetUInt(bufferDataStartOffset) == Constants.MSG_FLAG)
                {
                    found = true;
                    if (i != 0)
                    {
                        Buffer.BlockCopy(buffer, i, buffer, 0, bufferDataLen);
                        bufferDataStartOffset = 0;
                        bufferDataLen -= i;
                    }

                    break;
                }
            }

            if (found)
            {
                msgCompleteness = MSG_COMPLETENESS_FLAG;
                return ReadMsgHeaderFromBuffer();
            }
            else
            {
                // cannot find flag, reset buffer position and receive data again
                msgCompleteness = MSG_COMPLETENESS_NONE;
                bufferDataStartOffset = 0;
                bufferDataLen = 0;
                return ReadFromInputStream();
            }
        }

        private Msg ReadFromInputStream()
        {
            if (stream == null)
            {
                return null;
            }

            int len = 0;
            try
            {
                len = stream.Read(buffer, bufferDataStartOffset + bufferDataLen, GetBufferLeftSize());
            }
            catch (Exception e)
            {
                logger.Warn(string.Format("Stream.Read Exception: {0}", e.Message));
                return null;
            }

            if (len <= 0)
            {
                logger.Warn(string.Format("stream.Read len == {0}", len));
                return null;
            }

            bufferDataLen += len;

            switch (msgCompleteness)
            {
                case MSG_COMPLETENESS_NONE:
                    return ReadMsgFlagFromBuffer();
                case MSG_COMPLETENESS_FLAG:
                    return ReadMsgHeaderFromBuffer();
                case MSG_COMPLETENESS_HEADER:
                    return ReadMsgBodyFromBuffer();
                default:
                    return null;
            }

        }

        private Msg ReadMsgHeaderFromBuffer()
        {
            if (bufferDataLen < Constants.MSG_HEADER_LEN)
            {
                return ReadFromInputStream();
            }

            var headerBuffer = new byte[Constants.MSG_HEADER_LEN];
            Buffer.BlockCopy(buffer, bufferDataStartOffset, headerBuffer, 0, Constants.MSG_HEADER_LEN);
            MsgHeaderUtils.FromBytes(ref currentHeader, headerBuffer);
            msgCompleteness = MSG_COMPLETENESS_HEADER;

            return ReadMsgBodyFromBuffer();
        }

        private Msg ReadMsgBodyFromBuffer()
        {
            if (bufferDataLen < Constants.MSG_HEADER_LEN + currentHeader.topicLen + currentHeader.dataLen)
            {
                return ReadFromInputStream();
            }

            var topicBuffer = new byte[currentHeader.topicLen];
            Buffer.BlockCopy(buffer, bufferDataStartOffset + Constants.MSG_HEADER_LEN, topicBuffer, 0, currentHeader.topicLen);

            var dataBuffer = new byte[currentHeader.dataLen];
            Buffer.BlockCopy(buffer, bufferDataStartOffset + Constants.MSG_HEADER_LEN + currentHeader.topicLen, dataBuffer, 0, currentHeader.dataLen);

            currentMsg.header = currentHeader;
            currentMsg.topic = topicBuffer;
            currentMsg.data = dataBuffer;

            var leftDataLen = (bufferDataLen - currentMsg.Length());
            Buffer.BlockCopy(buffer, bufferDataStartOffset + currentMsg.Length(), buffer, 0, leftDataLen);

            bufferDataStartOffset = 0;
            bufferDataLen = leftDataLen;

            var copy = currentMsg.CopyOf();

            return copy;
        }

        private int GetBufferLeftSize()
        {
            return recvBufferSize - (bufferDataStartOffset + bufferDataLen);
        }

        private void MsgArrive(Msg msg)
        {
            if (onMsgArrivedListener != null)
            {
                onMsgArrivedListener.Invoke(msg);
            }
        }

        private void ConnectionStateChange(ConnectionState connectionState, Exception exception = null)
        {
            if (onConnectionStateListener != null)
            {
                onConnectionStateListener.Invoke(connectionState, exception);
            }
        }

        private void CommClose(bool isForce)
        {
            if (onCommCloseListener != null)
            {
                onCommCloseListener.Invoke(this, isForce);
            }
        }
    }
}
