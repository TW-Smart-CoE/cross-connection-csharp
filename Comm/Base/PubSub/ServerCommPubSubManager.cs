using System.Collections.Generic;
using System.Collections.Concurrent;

namespace CConn
{
    public class ServerCommPubSubManager
    {
        private ILogger logger;

        private IDictionary<int, CommServerWrapper> commServerWrapperDict = new ConcurrentDictionary<int, CommServerWrapper>();

        private IServerCallback serverCallback = null;

        internal void OnServerDataArrive(CommServerWrapper commServerWrapper, Msg msg)
        {
            switch (msg.header.type)
            {
                case (byte)MsgType.PUBLISH:
                    HandlePublish(msg);
                    break;
                case (byte)MsgType.SUBSCRIBE:
                    HandleSubscribe(commServerWrapper, msg);
                    break;
                case (byte)MsgType.UNSUBSCRIBE:
                    HandleUnsubscribe(commServerWrapper, msg);
                    break;
                default:
                    break;
            }
        }

        public ServerCommPubSubManager(ILogger logger)
        {
            this.logger = logger;
        }

        internal void SetServerCallback(IServerCallback serverCallback)
        {
            this.serverCallback = serverCallback;
        }

        internal void AddCommWrapper(CommServerWrapper commServerWrapper)
        {
            commServerWrapperDict.Add(commServerWrapper.GetHashCode(), commServerWrapper);
        }

        internal void RemoveCommWrapper(CommServerWrapper commServerWrapper)
        {
            commServerWrapperDict.Remove(commServerWrapper.GetHashCode());
        }

        internal int ClientCount()
        {
            return commServerWrapperDict.Count;
        }

        internal void ClearAllCommWrappers()
        {
            foreach (var pair in commServerWrapperDict)
            {
                var commServerWrapper = pair.Value;
                commServerWrapper.Clear();
                commServerWrapper.commHandler.Close(true);
            }

            commServerWrapperDict.Clear();
        }

        internal void HandlePublishMsgSelf(Msg msg)
        {
            var fullTopic = DataConverter.BytesToString(msg.topic);

            foreach (var pair in commServerWrapperDict)
            {
                var commServerWrapper = pair.Value;
                if (commServerWrapper.IsSubscribed(fullTopic))
                {

                    commServerWrapper.commHandler.Send(msg);
                }
            }
        }

        private void HandlePublish(Msg msg)
        {
            HandlePublishMsgSelf(msg);

            // publish msg to bus
            if (serverCallback != null)
            {
                serverCallback.OnPublish(msg);
            }
        }

        private void HandleSubscribe(CommServerWrapper commServerWrapper, Msg msg)
        {
            var fullTopic = DataConverter.BytesToString(msg.topic);
            // subscribe topic self
            commServerWrapper.Subscribe(fullTopic);

            // subscribe topic to bus
            if (serverCallback != null)
            {
                serverCallback.OnSubscribe(fullTopic);
            }
        }

        private void HandleUnsubscribe(CommServerWrapper commServerWrapper, Msg msg)
        {
            var fullTopic = DataConverter.BytesToString(msg.topic);

            // unsubscribe topic self
            commServerWrapper.Unsubscribe(fullTopic);

            // unsubscribe topic from bus
            if (serverCallback != null)
            {
                serverCallback.OnUnSubscribe(fullTopic);
            }
        }
    }
}
