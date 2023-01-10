using System.IO;

namespace CConn
{
    internal interface IComm
    {
        Stream GetStream();

        void Connect();

        void Close();
    }
}
