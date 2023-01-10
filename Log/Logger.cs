namespace CConn
{
    public interface ILogger
    {
        void Debug(object message);

        void Info(object message);

        void Warn(object message);

        void Error(object message);
    }
}
