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
        bool ResetRegister(
            ConnectionType connectionType,
            ConfigProps networkRegisterConfig
        );
        void StopAll();
        void Cleanup();
    }
}
