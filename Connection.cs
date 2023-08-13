using System;

namespace CConn
{ 
    public delegate void ConnectionStateChangedHandler(ConnectionState connectionState, Exception exception);
    public delegate void OnDataArriveListener(string topic, Method method, byte[] data);

    public enum Method
    {
        REPORT = 0,
        QUERY = 1,
        REPLY = 2,
        REQUEST = 3,
        RESPONSE = 4,
    }

    public enum ConnectionState
    {
        DISCONNECTED = 0,
        CONNECTING = 1,
        CONNECTED = 2,
        RECONNECTING = 3,
    }

    public interface IOnActionListener
    {
        void OnSuccess();

        void OnFailure(Exception exception);
    }

    public interface IConnection : IModule
    {
        event ConnectionStateChangedHandler ConnectionStateChanged; 

        bool Start(ConfigProps configProps);

        void Stop();

        ConnectionState GetState();

        void Publish(string topic, Method method, byte[] data, IOnActionListener onActionListener = null);

        void Subscribe(string topic, Method method, OnDataArriveListener onDataArriveListener, IOnActionListener onActionListener = null);

        void Unsubscribe(string topic, Method method);
    }
}
