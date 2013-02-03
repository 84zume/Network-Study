using System;
using System.Globalization;
using System.IO;

namespace HttpServer
{
    public class ZumeServer : Server
    {
        public ZumeServer(string address, int port) : base(address, port) { }

        public override void HandleGetRequest(Processor p)
        {
            Console.WriteLine("request: {0}", p.HttpUrl);
            p.WriteSuccess();
            p.OutputStream.WriteLine("<html><body><h1>test server</h1>");
            p.OutputStream.WriteLine("Current Time: " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            p.OutputStream.WriteLine("url : {0}", p.HttpUrl);

            p.OutputStream.WriteLine("<form method=post action=/form>");
            p.OutputStream.WriteLine("<input type=text name=foo value=foovalue>");
            p.OutputStream.WriteLine("<input type=submit name=bar value=barvalue>");
            p.OutputStream.WriteLine("</form>");
        }

        public override void HandlePostRequest(Processor p, StreamReader inputData)
        {
            Console.WriteLine("POST request: {0}", p.HttpUrl);
            string data = inputData.ReadToEnd();

            p.OutputStream.WriteLine("<html><body><h1>test server</h1>");
            p.OutputStream.WriteLine("<a href=/test>return</a><p>");
            p.OutputStream.WriteLine("postbody: <pre>{0}</pre>", data);
        }
    }
}
