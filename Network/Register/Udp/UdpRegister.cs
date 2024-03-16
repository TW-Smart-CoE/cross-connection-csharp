using System.Linq;
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
        private byte[] data = null;
        private bool debugMode = false;

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
            data = configProps.GetBytes(PropKeys.PROP_BROADCAST_DATA, null);
            debugMode = configProps.Get(PropKeys.PROP_BROADCAST_DEBUG_MODE, false);


            StartUdpBroadCast();
        }

        public void Unregister()
        {
            isSendBroadcast = false;
        }

        private void StartUdpBroadCast()
        {
            var broadcaster = new UdpClient
            {
                EnableBroadcast = true
            };
            isSendBroadcast = true;

            Task.Factory.StartNew(() =>
                    {
                        while (isSendBroadcast)
                        {
                            byte[] bytes = BuildBroadcastHeader();
                            if (data != null)
                            {
                                bytes = bytes.Concat(data).ToArray();
                            }

                            if (debugMode)
                            {
                                logger.Debug($"Send broadcast (len={bytes.Length}): {bytes.ToHexString()}");
                            }

                            broadcaster.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, broadcastPort));

                            Task.Delay(broadcastInterval).Wait();
                        }

                        broadcaster.Close();
                    }, TaskCreationOptions.LongRunning);

        }

        private byte[] BuildBroadcastHeader()
        {
            var broadcastHeader = new BroadcastHeader
            {
                flag = flag,
                ip = serverIp,
                port = (ushort)serverPort,
                dataLen = (ushort)(data == null ? 0 : data.Length),
            };

            return BroadcastHeaderUtils.ToBytes(ref broadcastHeader);
        } 
    }
}
