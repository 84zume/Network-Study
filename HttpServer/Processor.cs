using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace HttpServer
{
    public class Processor
    {
        public TcpClient Socket { get; set; }
        public Server Server { get; set; }
        private Stream _inputStream;
        public StreamWriter OutputStream { get; set; }
        public string HttpMethod { get; set; }
        public string HttpUrl { get; set; }
        public string HttpProtocolVersionString { get; set; }
        public Hashtable HttpHeaders = new Hashtable();

        private const int MaxPostSize = 10*1024*1024;

        public Processor(TcpClient client, Server httpServer)
        {
            Socket = client;
            Server = httpServer;
        }

        public void Process()
        {
            _inputStream = new BufferedStream(Socket.GetStream());
            OutputStream = new StreamWriter(new BufferedStream(Socket.GetStream()));
            try
            {
                ParseRequest();
                ReadHeaders();
                if (HttpMethod == "GET")
                {
                    HandleGetRequest();
                }
                else if (HttpMethod == "POST")
                {
                    HandlePostRequest();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("エラー：" + e);
                WriteFailuer();
            }
            OutputStream.Flush();
            _inputStream = null;
            OutputStream = null;
            Socket.Close();
        }

        private void WriteFailuer()
        {
            OutputStream.WriteLine("HTTP/1.0 404 File not found");
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }

        private const int BufSize = 4096;
        private void HandlePostRequest()
        {
            Console.WriteLine("get post data start");
            var ms = new MemoryStream();
            if (HttpHeaders.ContainsKey("Content-Length"))
            {
                var contentLength = Convert.ToInt32(HttpHeaders["Content-Length"]);
                if (contentLength > MaxPostSize)
                {
                    throw new Exception("Too big size");
                }
                var buffer = new byte[BufSize];
                var toRead = contentLength;
                while (toRead > 0)
                {
                    Console.WriteLine("starting read, toRead={0}", toRead);
                    var numRead = _inputStream.Read(buffer, 0, Math.Min(BufSize, toRead));
                    Console.WriteLine("read finished, numRead={0}", numRead);
                    if (numRead == 0)
                    {
                        if (toRead == 0)
                        {
                            break;
                        }
                        throw new Exception("client disconnected during post");
                    }
                    toRead -= numRead;
                    ms.Write(buffer, 0, numRead);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            Console.WriteLine("get post data end");
            Server.HandlePostRequest(this, new StreamReader(ms));
        }

        private void HandleGetRequest()
        {
            Server.HandleGetRequest(this);
        }

        private void ReadHeaders()
        {
            Console.WriteLine("Read Headers");
            string line;
            while ((line = streamReadLine(_inputStream)) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                var separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                var key = line.Substring(0, separator);
                var pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }
                var value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header : {0} : {1}", key, value);
                HttpHeaders[key] = value;
            }
        }

        private void ParseRequest()
        {
            var request = streamReadLine(_inputStream);
            var tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            HttpMethod = tokens[0].ToUpper();
            HttpUrl = tokens[1];
            HttpProtocolVersionString = tokens[2];
            Console.WriteLine("starting: " + request);
        }

        private string streamReadLine(Stream inputStream)
        {
            var data = "";
            while (true)
            {
                var nextChar = inputStream.ReadByte();
                if (nextChar == '\n')
                {
                    break;
                }
                if (nextChar == '\r')
                {
                    continue;
                }
                if (nextChar == -1)
                {
                    Thread.Sleep(1);
                    continue;
                }
                data += Convert.ToChar(nextChar);
            }
            return data;
        }

        public void WriteSuccess()
        {
            OutputStream.WriteLine("HTTP/1.0 200 OK");
            OutputStream.WriteLine("Content-Type: text/html");
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }
    }
}
