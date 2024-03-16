using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CConn
{
    internal class UdpDetector : INetworkDetector
    {
        private const int DEFAULT_BROADCAST_PORT = 12000;

        private ILogger logger = new DefaultLogger();
        private UdpClient broadcastListener;
        private bool isKeepReceiving = false;
        private int broadcastPort;
        private uint flag;
        private bool debugMode = false;

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void StartDiscover(ConfigProps configProps, OnFoundServiceHandler onFoundService)
        {
            broadcastPort = configProps.Get(PropKeys.PROP_BROADCAST_PORT, DEFAULT_BROADCAST_PORT);
            flag = configProps.Get(PropKeys.PROP_FLAG, UdpRegister.DEFAULT_BROADCAST_FLAG);
            debugMode = configProps.Get(PropKeys.PROP_BROADCAST_DEBUG_MODE, false);

            broadcastListener = new UdpClient(broadcastPort);
            IPEndPoint groupEP = new(IPAddress.Any, broadcastPort);
            isKeepReceiving = true;

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (isKeepReceiving)
                        {
                            logger.Debug(string.Format("Waiting for broadcast on port {0}", broadcastPort));
                            byte[] bytes = broadcastListener.Receive(ref groupEP);

                            if (debugMode)
                            {
                                logger.Debug($"Received broadcast (len={bytes.Length}): {bytes.ToHexString()}");
                            }

                            if (bytes.Length >= Constants.BROADCAST_MSG_HEADER_LEN)
                            {
                                var receiveMsgFlag = bytes.GetUInt(0);
                                if (receiveMsgFlag == flag)
                                {
                                    var broadcastHeader = new BroadcastHeader();
                                    BroadcastHeaderUtils.FromBytes(ref broadcastHeader, bytes);

                                    if (broadcastHeader.dataLen == bytes.Length - Constants.BROADCAST_MSG_HEADER_LEN)
                                    {
                                        ConfigProps onFoundServiceProps = new();
                                        onFoundServiceProps.Put(PropKeys.PROP_SERVER_IP, IpAddressUtils.UIntToIpString(broadcastHeader.ip));
                                        onFoundServiceProps.Put(PropKeys.PROP_SERVER_PORT, broadcastHeader.port);

                                        if (broadcastHeader.dataLen > 0)
                                        {
                                            onFoundServiceProps.Put(PropKeys.PROP_BROADCAST_DATA, bytes.Skip(Constants.BROADCAST_MSG_HEADER_LEN).ToArray());
                                        }
                                        
                                        onFoundService.Invoke(onFoundServiceProps);
                                    }
                                    else
                                    {
                                        if (debugMode)
                                        {
                                            logger.Error($"Invalid broadcast msg data len {bytes.Length - Constants.BROADCAST_MSG_HEADER_LEN}, but broadcast_header.data_len is {broadcastHeader.dataLen}");
                                        }
                                    }
                                }
                                else
                                {
                                    if (debugMode)
                                    {
                                        logger.Error($"Received broadcast flag does not match");
                                    }
                                }
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        logger.Debug(e.ToString());
                    }
                    finally
                    {
                        CloseBroadcastListener();
                    }
                }, TaskCreationOptions.None);
        }

        public void StopDiscover()
        {
            isKeepReceiving = false;
            CloseBroadcastListener();
        }

        private void CloseBroadcastListener()
        {
            if (broadcastListener == null)
            {
                return;
            }

            try
            {
                broadcastListener.Close();
            }
            catch (SocketException)
            {
                broadcastListener = null;
            }
        }
    }
}
