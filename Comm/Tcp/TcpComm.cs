using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CConn
{
    internal class TcpComm : IComm 
    {
        private TcpClient tcpClient;

        private IPAddress address;

        private int port;

        public TcpComm(TcpClient tcpClient, IPAddress address = null, int port = 0)
        {
            this.tcpClient = tcpClient;
            this.address = address;
            this.port = port;
        }

        public Stream GetStream()
        {
            return tcpClient.GetStream();
        }

        public void Connect()
        {
            if (address != null && port != 0)
            {
                tcpClient.Connect(address, port);
            }
        }

        public void Close()
        {
            tcpClient.Close();
        }
    }
}

