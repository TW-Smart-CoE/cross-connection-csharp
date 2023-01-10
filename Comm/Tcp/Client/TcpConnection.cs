using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CConn
{
    public class TcpConnection : IConnection
    {
        private const int DEFAULT_PORT = 9100;
        private const int DEFAULT_MIN_RECONNECT_RETRY_TIME_DEFAULT = 4;
        private const int DEFAULT_MAX_RECONNECT_RETRY_TIME_DEFAULT = 32;
        private const int SECOND_TO_MILLISECOND = 1000;

        private ILogger logger = new DefaultLogger();
        private ClientCommPubSubManager subscribeManager;
        private ConnectionState connectionState = ConnectionState.DISCONNECTED;
        private IPAddress ipAddress;
        private int port = DEFAULT_PORT;
        private bool autoConnect = false;
        private int minReconnectRetryTime = DEFAULT_MIN_RECONNECT_RETRY_TIME_DEFAULT;
        private int maxReconnectRetryTime = DEFAULT_MAX_RECONNECT_RETRY_TIME_DEFAULT;
        private int currentReconnectRetryTime = DEFAULT_MIN_RECONNECT_RETRY_TIME_DEFAULT;
        private int recvBufferSize = Constants.DEFAULT_RECV_BUFFER_SIZE;
        private CommHandler commHandler = null;
        private bool isStarted = false;

        public event ConnectionStateChangedHandler ConnectionStateChanged;

        public TcpConnection()
        {
            this.subscribeManager = new ClientCommPubSubManager(logger);
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public ConnectionState GetState()
        {
            return connectionState;
        }

        public void Publish(string topic, Method method, byte[] data, IOnActionListener onActionListener)
        {
            if (!isStarted)
            {
                if (onActionListener != null)
                {
                    onActionListener.OnFailure(new Exception("TcpConnection not started"));
                }

                return;
            }

            if (commHandler == null)
            {
                if (onActionListener != null)
                {
                    onActionListener.OnFailure(new Exception("commHandler is null"));
                }

                return;
            }

            Task.Run(() =>
                    {
                        var fullTopic = TopicUtils.ToFullTopic(topic, method);
                        var fullTopicBytes = DataConverter.StringToBytes(fullTopic);

                        try
                        {
                            var msg = new Msg();
                            msg.header = new MsgHeader();
                            msg.header.flag = Constants.MSG_FLAG;
                            msg.header.type = (byte) MsgType.PUBLISH;
                            msg.header.method = (byte) method;
                            msg.header.topicLen = Convert.ToUInt16(fullTopicBytes.Length);
                            msg.header.dataLen = Convert.ToUInt16(data.Length);
                            msg.topic = fullTopicBytes;
                            msg.data = data;

                            commHandler.Send(msg);
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Error occurred when sending data: {0}", e.Message));
                            if (onActionListener != null)
                            {
                                onActionListener.OnFailure(e);
                            }
                        }
                    });
        }

        public bool Start(ConfigProps configProps)
        {
            if (isStarted)
            {
                logger.Warn("TcpConnetion already started");
                return true;
            }

            string propertyIp = configProps.Get(PropKeys.PROP_IP, "");
            if (!IPAddress.TryParse(propertyIp, out ipAddress))
            {
                ChangeConnectionState(ConnectionState.DISCONNECTED, new Exception(string.Format("Parse IpAddress failed: {0}", propertyIp)));
                logger.Error(string.Format("Parse IpAddress failed: {0}", propertyIp));
                return false;
            }


            port = configProps.Get(PropKeys.PROP_PORT, Constants.INVALID_PORT);
            if (port == Constants.INVALID_PORT)
            {
                ChangeConnectionState(ConnectionState.DISCONNECTED, new Exception("Parse port failed"));
                logger.Error("Parse port failed");
                return false;
            }

            autoConnect = configProps.Get(PropKeys.PROP_AUTO_RECONNECT, false);
            minReconnectRetryTime = configProps.Get(PropKeys.PROP_MIN_RECONNECT_RETRY_TIME, DEFAULT_MIN_RECONNECT_RETRY_TIME_DEFAULT);
            maxReconnectRetryTime = configProps.Get(PropKeys.PROP_MAX_RECONNECT_RETRY_TIME, DEFAULT_MAX_RECONNECT_RETRY_TIME_DEFAULT);
            currentReconnectRetryTime = Math.Min(maxReconnectRetryTime, minReconnectRetryTime);
            recvBufferSize = configProps.Get(PropKeys.PROP_RECV_BUFFER_SIZE, Constants.DEFAULT_RECV_BUFFER_SIZE);

            TcpConnect();

            isStarted = true;
            return true;
        }

        public void Stop()
        {
            Task.Run(() =>
                    {
                        if (commHandler != null)
                        {
                            commHandler.Close(true);
                            subscribeManager.Clear();

                            ipAddress = null;
                            port = DEFAULT_PORT;
                            autoConnect = false;
                            isStarted = false;
                        }
                    });
        }

        public void Subscribe(string topic, Method method, OnDataArriveListener onDataArrived, IOnActionListener onActionListener)
        {
            if (!isStarted)
            {
                if (onActionListener != null)
                {
                    onActionListener.OnFailure(new Exception("TcpConnection not started"));
                }

                return;
            }

            if (commHandler == null)
            {
                if (onActionListener != null)
                {
                    onActionListener.OnFailure(new Exception("commHandler is null"));
                }

                return;
            }

            Task.Run(() =>
                    {
                        var fullTopic = TopicUtils.ToFullTopic(topic, method);
                        var fullTopicBytes = DataConverter.StringToBytes(fullTopic);

                        try
                        {
                            var msg = new Msg();
                            msg.header = new MsgHeader();
                            msg.header.flag = Constants.MSG_FLAG;
                            msg.header.type = (byte) MsgType.SUBSCRIBE;
                            msg.header.method = (byte) method;
                            msg.header.topicLen = Convert.ToUInt16(fullTopicBytes.Length);
                            msg.header.dataLen = 0;
                            msg.topic = fullTopicBytes;

                            commHandler.Send(msg);
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Error occurred when sending data: {0}", e.Message));
                            if (onActionListener != null)
                            {
                                onActionListener.OnFailure(e);
                            }
                        }

                        subscribeManager.Subscribe(new Subscription(fullTopic, onDataArrived));
                    });
        }

        public void Unsubscribe(string topic, Method method)
        {
            if (!isStarted)
            {
                return;
            }

            if (commHandler == null)
            {
                return;
            }

            Task.Run(() =>
                    {
                        var fullTopic = TopicUtils.ToFullTopic(topic, method);
                        var fullTopicBytes = DataConverter.StringToBytes(fullTopic);

                        try
                        {
                            var msg = new Msg();
                            msg.header = new MsgHeader();
                            msg.header.flag = Constants.MSG_FLAG;
                            msg.header.type = (byte) MsgType.UNSUBSCRIBE;
                            msg.header.method = (byte) method;
                            msg.header.topicLen = Convert.ToUInt16(fullTopicBytes.Length);
                            msg.header.dataLen = 0;
                            msg.topic = fullTopicBytes;

                            commHandler.Send(msg);
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Error occurred when sending data: {0}", e.Message));
                        }

                        subscribeManager.Unsubscribe(fullTopic);
                    });
        }

        private void TcpConnect()
        {
            Task.Factory.StartNew(() =>
                    {
                        subscribeManager.Clear();

                        TcpComm tcpComm = new TcpComm(new TcpClient(), ipAddress, port);
                        commHandler = new CommHandler(
                            true,
                            tcpComm,
                            logger,
                            recvBufferSize,
                            (handler, isForce) =>
                            {
                                if (!isForce && autoConnect)
                                {
                                    ScheduleReconnect();
                                }
                            },
                            (msg) =>
                            {
                                OnMsgArrived(msg);
                            },
                            (connectionState, exception) =>
                            {
                                ChangeConnectionState(connectionState, exception);
                            });

                        commHandler.Run();
                    }, TaskCreationOptions.LongRunning);
        }

        private void ScheduleReconnect()
        {
            Task.Run(() =>
                    {
                        ChangeConnectionState(ConnectionState.CONNECTING);
                        logger.Info(string.Format("Schedule tcp reconnect attempt in {0} seconds.", currentReconnectRetryTime));

                        Task.Delay(currentReconnectRetryTime * SECOND_TO_MILLISECOND).Wait();
                        currentReconnectRetryTime = Math.Min(currentReconnectRetryTime * 2, maxReconnectRetryTime);

                        logger.Error("TcpClient reconnecting ...");
                        TcpConnect();
                    });
        }

        private void OnMsgArrived(Msg msg)
        {
            if (msg.header.type != (byte) MsgType.PUBLISH)
            {
                return;
            }

            subscribeManager.InvokeMatchedCallback(DataConverter.BytesToString(msg.topic), msg.data);
        }

        private void ChangeConnectionState(ConnectionState connectionState, Exception exception = null)
        {
            this.connectionState = connectionState;
            if (connectionState == ConnectionState.CONNECTED) {
                currentReconnectRetryTime = minReconnectRetryTime;
            }

            if (ConnectionStateChanged != null)
            {
                ConnectionStateChanged(connectionState, exception);
            }
        }
    }
}
