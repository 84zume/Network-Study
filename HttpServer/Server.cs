using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HttpServer
{
    public abstract class Server
    {

        protected int Port;
        protected IPAddress IpAddress;
        private TcpListener _listener;

        protected Server(string address, int port)
        {
            IpAddress = IPAddress.Parse(address);
            Port = port;
        }

        public void Listen()
        {
            _listener = new TcpListener(IpAddress, Port);
            _listener.Start();
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var processor = new Processor(client, this);
                var thread = new Thread(processor.Process);
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void HandleGetRequest(Processor p);
        public abstract void HandlePostRequest(Processor p, StreamReader inputData);
    }
}
