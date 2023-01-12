using System.Collections.Generic;

namespace CConn
{ 
    internal class CrossConnectionBus : IBus
    {
        private class MessageObjPublish
        {
            public Msg msg;
            public IServer excludeServer;
        }

        private class ServerCallback : IServerCallback
        {
            private CrossConnectionBus crossConnectionBus;
            private IServer server;

            public ServerCallback(CrossConnectionBus crossConnectionBus, IServer server)
            {
                this.crossConnectionBus = crossConnectionBus;
                this.server = server;
            }

            public void OnPublish(Msg msg)
            {
                crossConnectionBus.PublishMessageToBus(msg, server);
            }

            public void OnSubscribe(string fullTopic)
            {
            }

            public void OnUnSubscribe(string fullTopic)
            {
            }
        }

        private ILogger logger = new DefaultLogger();
        private Dictionary<ConnectionType, ServerStruct> serverMap = new Dictionary<ConnectionType, ServerStruct>();
        private MsgThread<MessageObjPublish> msgThread = new MsgThread<MessageObjPublish>();
        private bool isInitialized = false;

        public bool Initialize()
        {
            if (isInitialized)
            {
                return true;
            }

            CreateMessageProcessingThread();


            var tcpServer = ConnectionFactory.CreateServer(ConnectionType.TCP);
            tcpServer.SetCallback(new ServerCallback(this, tcpServer));
            tcpServer.SetLogger(logger);

            var udpRegister = ConnectionFactory.CreateNetworkRegister(NetworkDiscoveryType.UDP);
            udpRegister.SetLogger(logger);

            serverMap[ConnectionType.TCP] = new ServerStruct(
                tcpServer,
                udpRegister
            );

            isInitialized = true;
            return isInitialized;
        } 

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public bool Start(
            ConnectionType connectionType,
            ConfigProps serverConfig,
            ConfigProps networkRegisterConfig
            )
        {
            if (!isInitialized)
            {
                return false;
            }

            ServerStruct serverStruct;
            if (!serverMap.TryGetValue(connectionType, out serverStruct))
            {
                return false;
            }

            var isStarted = serverStruct.server.Start(serverConfig);
            if (isStarted)
            {
                logger.Info($"{connectionType} server started");
            }
            else
            {
                return false;
            }

            serverStruct.register.Register(networkRegisterConfig);
            return isStarted;
        }

        public void StopAll()
        {
            foreach (var entry in serverMap) 
            {
                entry.Value.register.Unregister();
                entry.Value.server.Stop();
            }

            logger.Info($"CrossConnectionBus stopped");
        }

        private void CreateMessageProcessingThread()
        {
            msgThread.SetOnMsgArrivedListener((messageObjPublish) =>
            {
                foreach (var entry in serverMap)
                {
                    if (entry.Value.server != messageObjPublish.excludeServer)
                    {
                        entry.Value.server.HandlePublishMessage(messageObjPublish.msg);
                    }
                }
            });

            msgThread.Start();
        }

        private void PublishMessageToBus(Msg msg, IServer excludeServer)
        {
            msgThread.Enqueue(new MessageObjPublish
            {
                msg = msg,
                excludeServer = excludeServer,
            });
        }
    }
}