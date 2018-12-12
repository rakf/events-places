using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Response
    {
        Byte[] data = null;
        String status;
        String mime;
        private Response(String status, String mime, Byte[] data)
        {
            this.data = data;
            this.status = status;
            this.mime = mime;
        }
        
        

        private static Response NotWork( String status)
        {
            return new Response(status, "text/html", null);
        }
        public void Post(NetworkStream stream)
        {
            StreamWriter writer = new StreamWriter(stream);

            Console.WriteLine(String.Format("Response:\r\n{0} {1}\r\nServer: {2}\r\nContent-Language: ru\r\nContent-Type: {3}\r\nAccept-Ranges: bytes\r\nContent-Length: {4}\r\nConnection: close\r\n",
                HttpServer.VERSION, status, HttpServer.SERVERNAME, mime, data.Length));
            Console.WriteLine(Encoding.UTF8.GetString(data, 0, data.Length));

            writer.WriteLine(String.Format("{0} {1}\r\nServer: {2}\r\nContent-Language: ru\r\nContent-Type: {3}\r\nAccept-Ranges: bytes\r\nContent-Length: {4}\r\nConnection: close\r\n",
                HttpServer.VERSION, status, HttpServer.SERVERNAME, mime, data.Length));
            writer.Flush();
            stream.Write(data, 0, data.Length);
        }
    }
}