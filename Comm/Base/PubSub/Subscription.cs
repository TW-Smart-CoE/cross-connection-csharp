using System;

namespace CConn
{
    internal class Subscription
    {
        public string topic;

        public OnDataArriveListener onDataArriveListener;

        public Subscription(String topic, OnDataArriveListener onDataArriveListener)
        {
            this.topic = topic;
            this.onDataArriveListener = onDataArriveListener;
        }
    }
}
