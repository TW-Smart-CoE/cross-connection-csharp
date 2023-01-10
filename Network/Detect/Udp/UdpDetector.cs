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

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void StartDiscover(ConfigProps configProps, OnFoundServiceHandler onFoundService)
        {
            broadcastPort = configProps.Get(PropKeys.PROP_BROADCAST_PORT, DEFAULT_BROADCAST_PORT);
            flag = configProps.Get(PropKeys.PROP_FLAG, UdpRegister.DEFAULT_BROADCAST_FLAG);

            broadcastListener = new UdpClient(broadcastPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, broadcastPort);
            isKeepReceiving = true;

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (isKeepReceiving)
                        {
                            logger.Debug(string.Format("Waiting for broadcast on port {0}", broadcastPort));
                            byte[] bytes = broadcastListener.Receive(ref groupEP);

                            logger.Debug(string.Format("Received broadcast from {0}", groupEP.ToString()));
                            if (bytes.Length == Constants.BROADCAST_MSG_HEADER_LEN)
                            {
                                var receiveMsgFlag = bytes.GetUInt(0);
                                if (receiveMsgFlag == flag)
                                {
                                    var broadcastMsg = new BroadcastMsg();
                                    BroadcastMsgUtils.FromBytes(ref broadcastMsg, bytes);

                                    ConfigProps onFoundServiceProps = new ConfigProps();
                                    onFoundServiceProps.Put(PropKeys.PROP_SERVER_IP, IpAddressUtils.UIntToIpString(broadcastMsg.ip));
                                    onFoundServiceProps.Put(PropKeys.PROP_SERVER_PORT, broadcastMsg.port);
                                    onFoundService.Invoke(onFoundServiceProps);
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
