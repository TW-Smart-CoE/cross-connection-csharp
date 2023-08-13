using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CConn
{
    internal class ClientCommPubSubManager
    {
        private ILogger logger;
        private IDictionary<string, Subscription> subscriptionMap = new Dictionary<string, Subscription>();

        public ClientCommPubSubManager(ILogger logger)
        {
            this.logger = logger;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Subscribe(Subscription subscription)
        {
            subscriptionMap[subscription.topic] = subscription;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Unsubscribe(string topic) {
            subscriptionMap.Remove(topic);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear() {
            subscriptionMap.Clear();
        }

        public void InvokeMatchedCallback(string fullTopic, byte[] data)
        {
            foreach (var element in subscriptionMap)
            {
                if (TopicUtils.IsTopicMatch(element.Key, fullTopic))
                {
                    if (element.Value.onDataArriveListener != null)
                    {
                        try
                        {
                            var result = TopicUtils.ToAppTopic(fullTopic);
                            element.Value.onDataArriveListener.Invoke(result.Item1, result.Item2, data);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.Message);
                        }
                    }
                }
            }
        }
    }
}
