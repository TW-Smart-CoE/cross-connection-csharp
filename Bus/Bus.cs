namespace CConn
{
    public interface IBus : IModule
    {
        bool Initialize();
        bool Start(
            ConnectionType connectionType,
            ConfigProps serverConfig,
            ConfigProps networkRegisterConfig
            );
        void StopAll();
    }
}
