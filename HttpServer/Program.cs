using System.Threading;

namespace HttpServer
{
    class Program
    {
        static void Main()
        {
            var httpServer = new ZumeServer("192.168.24.54", 8090);
            var thread = new Thread(httpServer.Listen);
            thread.Start();
        }
    }
}
