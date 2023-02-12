using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CConn
{
    internal class UdpRegister : INetworkRegister
    {
        private const int DEFAULT_BROADCAST_PORT = 12000;
        private const int DEFAULT_BROADCAST_INTERVAL = 10000;
        internal const uint DEFAULT_BROADCAST_FLAG = 0xFFFEC1E5;

        private ILogger logger = new DefaultLogger();
        private bool isSendBroadcast;
        private int broadcastPort;
        private int broadcastInterval;
        private uint flag;
        private uint serverIp;
        private int serverPort;

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void Register(ConfigProps configProps)
        {
            broadcastPort = configProps.Get(PropKeys.PROP_BROADCAST_PORT, DEFAULT_BROADCAST_PORT);
            broadcastInterval = configProps.Get(PropKeys.PROP_BROADCAST_INTERVAL, DEFAULT_BROADCAST_INTERVAL);
            flag = configProps.Get(PropKeys.PROP_FLAG, DEFAULT_BROADCAST_FLAG);

            var strIp = configProps.Get(PropKeys.PROP_SERVER_IP, IpAddressUtils.GetLocalIpAddress().ToString());
            serverIp = IpAddressUtils.IpStringToUInt(strIp);

            serverPort = configProps.Get(PropKeys.PROP_SERVER_PORT, 0);

            StartUdpBroadCast();
        }

        public void Unregister()
        {
            isSendBroadcast = false;
        }

        private void StartUdpBroadCast()
        {
            var broadcaster = new UdpClient();
            broadcaster.EnableBroadcast = true;
            isSendBroadcast = true;

            Task.Factory.StartNew(() =>
                    {
                        while (isSendBroadcast)
                        {
                            byte[] bytes = BuildBroadcastMsg();
                            broadcaster.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, broadcastPort));

                            Task.Delay(broadcastInterval).Wait();
                        }
                    }, TaskCreationOptions.LongRunning);

            broadcaster.Close();
        }

        private byte[] BuildBroadcastMsg()
        {
            var broadcastMsg = new BroadcastMsg();
            broadcastMsg.flag = flag;
            broadcastMsg.ip = serverIp;
            broadcastMsg.port = (ushort) serverPort;

            return BroadcastMsgUtils.ToBytes(ref broadcastMsg);
        } 
    }
}
