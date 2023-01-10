namespace CConn
{
    public class DefaultLogger : ILogger
    {
        public void Debug(object message)
        {
            System.Console.WriteLine($"[Debug] {message}");
        }

        public void Info(object message)
        {
            System.Console.WriteLine($"[Info] {message}");
        }

        public void Warn(object message)
        {
            System.Console.WriteLine($"[Warn] {message}");
        }

        public void Error(object message)
        {
            System.Console.WriteLine($"[Error] {message}");
        }
    }
}

