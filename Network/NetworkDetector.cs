namespace CConn
{
    public delegate void OnFoundServiceHandler(ConfigProps configProps);

    public interface INetworkDetector : IModule
    {
        void StartDiscover(ConfigProps configProps, OnFoundServiceHandler onFoundService);

        void StopDiscover();
    }
}
