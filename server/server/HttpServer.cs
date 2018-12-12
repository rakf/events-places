using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
namespace server
{
    
    class HttpServer
    {
        private List<Route> Routes = new List<Route>();
        public const String MSG_DIR = "/root/msg/";
        public const String WEB_DIR = "/root/web/";
        public const String VERSION = "HTTP/1.0";
        public const String SERVERNAME = "myserv/1.1";
        TcpListener listener;

        bool running = false;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        public HttpServer(int port)
        {
            listener = new TcpListener(localAddr, port);
        }
        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }
        private void Run()
        {
            listener.Start();
            running = true;
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("server is running.");
            while (running)
            {
                Console.WriteLine("waiting for connection...");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("client connected.");
                HandleClient(client);
                client.Close();
            }

            running = false;
            listener.Stop();

        }
        private void HandleClient(TcpClient client)
        {
            Stream inputStream = GetInputStream(client);
            Stream outputStream = GetOutputStream(client);
            HttpRequest request = GetRequest(inputStream, outputStream);
          
            Console.WriteLine(request.Url);
            Console.WriteLine(request.Content);
            HttpResponse response = Handlers.GetRequest(request);

            WriteResponse(outputStream, response);

            outputStream.Flush();
            outputStream.Close();
            outputStream = null;

            inputStream.Close();
            inputStream = null;
            // Request request = Request.GetRequest(msg);
            // Response response = Response.From(request);
            // response.Post(client.GetStream());
        }
        private HttpRequest GetRequest(Stream inputStream, Stream outputStream)
        {
            //Read Request Line
            string request = Readline(inputStream);

            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = Readline(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            string content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];
               
                while(bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }


            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = content
            };
        }
        private static string Readline(Stream stream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        private static void WriteResponse(Stream stream, HttpResponse response)
        {
            if (response.Content == null)
            {
                response.Content = new byte[] { };
            }

            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            Write(stream, string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            Write(stream, "\r\n\r\n");

            stream.Write(response.Content, 0, response.Content.Length);
        }
        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
