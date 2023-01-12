namespace CConn
{
    internal class ServerStruct
    {
        public IServer server;
        public INetworkRegister register;

        public ServerStruct(IServer server, INetworkRegister register)
        {
            this.server = server;
            this.register = register;
        }
    }
}