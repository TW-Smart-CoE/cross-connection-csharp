using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CConn
{
    internal class TcpServer : IServer
    {
        private const string TCP_LOCALHOST = "0.0.0.0";
        private const int PROPERTY_PORT_DEFAULT = 9100;

        private TcpListener server = null;
        private ILogger logger = new DefaultLogger();
        private int serverPort = PROPERTY_PORT_DEFAULT;
        private int recvBufferSize = Constants.DEFAULT_RECV_BUFFER_SIZE;
        private ServerCommPubSubManager serverPubSubManager;

        public TcpServer()
        {
            this.serverPubSubManager = new ServerCommPubSubManager(logger);
        }

        public void HandlePublishMessage(Msg msg)
        {
            serverPubSubManager.HandlePublishMsgSelf(msg);
        }

        public void SetCallback(IServerCallback serverCallback)
        {
            serverPubSubManager.SetServerCallback(serverCallback);
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public bool Start(ConfigProps configProps)
        {
            serverPort = configProps.Get(PropKeys.PROP_PORT, Constants.INVALID_PORT);
            if (serverPort == Constants.INVALID_PORT)
            {
                logger.Error("Parse port failed");
                return false;
            }

            recvBufferSize = configProps.Get(PropKeys.PROP_RECV_BUFFER_SIZE, Constants.DEFAULT_RECV_BUFFER_SIZE);

            Stop();

            CreateServer();

            if (server == null)
            {
                return false;
            }

            Task.Factory.StartNew(() =>
                    {
                        while (server != null)
                        {
                            System.Net.Sockets.TcpClient client;
                            try
                            {
                                client = server.AcceptTcpClient();
                            }
                            catch (InvalidOperationException e)
                            {
                                logger.Error(string.Format("TcpListener has not been started: {0}", e.Message));
                                ClearClients();
                                break;
                            }
                            catch (SocketException e)
                            {
                                logger.Error(string.Format("TcpListener accept() failed: {0}", e.Message));
                                ClearClients();
                                break;
                            }

                            var commHandler = new CommHandler(
                                false,
                                new TcpComm(client),
                                logger,
                                recvBufferSize
                                );
                            var commServerWrapper = new CommServerWrapper(commHandler);

                            commHandler.SetOnCommCloseListener((CommHandler handler, bool isForce) =>
                                    {
                                        serverPubSubManager.RemoveCommWrapper(commServerWrapper);
                                        logger.Debug($"Current client count = {serverPubSubManager.ClientCount()}");
                                    });
                            commHandler.SetOnMsgArrivedListener((msg) =>
                                    {
                                        serverPubSubManager.OnServerDataArrive(commServerWrapper, msg);
                                    });

                            serverPubSubManager.AddCommWrapper(commServerWrapper);
                            logger.Debug($"Current client count = {serverPubSubManager.ClientCount()}");

                            Task.Run(() =>
                            {
                                commHandler.Run();
                            });
                        }
                    }, TaskCreationOptions.LongRunning);

            return true;
        }

        public void Stop()
        {
            if (server == null)
            {
                return;
            }

            try
            {
                server.Stop();
            }
            catch (SocketException e)
            {
                logger.Error(string.Format("TcpListener stop failed: {0}", e.Message));
            }

            ClearClients();
            server = null;
            serverPort = PROPERTY_PORT_DEFAULT;
        }

        private void ClearClients()
        {
            serverPubSubManager.ClearAllCommWrappers();
        }

        private void CreateServer()
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(TCP_LOCALHOST);
                server = new TcpListener(localAddr, serverPort);
                server.Start();
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Tcp server socket creation failed: {0}", e.Message));
            }
        }
    }
}
