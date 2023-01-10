namespace CConn
{
    public interface INetworkRegister : IModule
    {
        void Register(ConfigProps configProps);

        void Unregister();
    }
}

