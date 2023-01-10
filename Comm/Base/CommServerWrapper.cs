using System.Collections.Generic;

namespace CConn
{
    internal class CommServerWrapper
    {
        internal CommHandler commHandler;

        private ISet<string> subscribeTopics = new HashSet<string>();

        public CommServerWrapper(CommHandler commHandler)
        {
            this.commHandler = commHandler;
        }

        public void Subscribe(string topic)
        {
            if (!subscribeTopics.Contains(topic))
            {
                subscribeTopics.Add(topic);
            }
        }

        public void Unsubscribe(string topic)
        {
            subscribeTopics.Remove(topic);
        }

        public bool IsSubscribed(string topic)
        {
            foreach (var element in subscribeTopics)
            {
                if (TopicUtils.IsTopicMatch(element, topic))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            subscribeTopics.Clear();
        }
    }
}
