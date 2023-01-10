namespace CConn
{
    public interface IServerCallback
    {
        void OnSubscribe(string fullTopic);
        void OnUnSubscribe(string fullTopic);
        void OnPublish(Msg msg);
    }

    public interface IServer : IModule
    {
        bool Start(ConfigProps configProps);

        void Stop();

        void HandlePublishMessage(Msg msg);

        void SetCallback(IServerCallback serverCallback);
    }
}

