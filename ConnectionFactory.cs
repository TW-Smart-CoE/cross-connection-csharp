namespace CConn
{
    public enum ConnectionType
    {
        TCP = 0
    }

    public enum NetworkDiscoveryType
    {
        UDP = 0
    }

    public static class ConnectionFactory
    {
        public static IConnection CreateConnection(ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case ConnectionType.TCP:
                    return new TcpConnection();
                default:
                    return null;
            }
        }

        public static IServer CreateServer(ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case ConnectionType.TCP:
                    return new TcpServer();
                default:
                    return null;
            }
        }

        public static INetworkRegister CreateNetworkRegister(NetworkDiscoveryType networkDiscoveryType)
        {
            switch (networkDiscoveryType)
            {
                case NetworkDiscoveryType.UDP:
                    return new UdpRegister();
                default:
                    return null;
            }
        }

        public static INetworkDetector CreateNetworkDetector(NetworkDiscoveryType networkDiscoveryType)
        {
            switch (networkDiscoveryType)
            {
                case NetworkDiscoveryType.UDP:
                    return new UdpDetector();
                default:
                    return null;
            }
        }

        public static IBus CreateBus()
        {
            return new CrossConnectionBus();
        }
    }
}
