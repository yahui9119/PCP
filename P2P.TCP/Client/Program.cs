using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace P2P.Client
{
    class Program
    {
        //static TcpListener c = new TcpListener(10086);
        static void Main(string[] args)
        {
            Console.WriteLine("请输入服务器ip：");
            Client client = new Client(Console.ReadLine());
            Console.WriteLine("请输入用户名：");
            client.ConnectToServer(Console.ReadLine(), "123456");
            client.Start();
            //IPEndPoint endPoint;
            //IPAddress IpList;
            //IpList = Dns.GetHostAddresses("www.cnblogs.com")[0];
            // StringBuilder sendString=new StringBuilder(200);
            //sendString.Append("POST "+ "/" + " HTTP/1.1\r\n");
            //sendString.Append("Accept: */*\r\n");
            //sendString.Append("Host: www.cnblogs.com\r\n");
            //sendString.Append("User-Agent: Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.72 Safari/537.36\r\n");
            //sendString.Append("Content-Type: application/x-www-form-urlencoded\r\n");
            ////sendString.Append("Content-Length: 10086\r\n");
            //sendString.Append("Connection: keep-alive\r\n\r\n");
            //var socket = c.Server;
            //byte[] sendBytes = Encoding.Default.GetBytes(sendString.ToString());
            //int httpPoint = 80;
            //endPoint = new IPEndPoint(IPAddress.Parse("42.121.252.58"), httpPoint);
            ////socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(endPoint);
            //socket.Send(sendBytes, sendBytes.Length, 0);
            //Byte[] byteReceive = new Byte[1024];
            //Int32 bytes = socket.Receive(byteReceive);
            //string str = Encoding.Default.GetString(byteReceive, 0, bytes);
            //c.Start();
            Console.ReadLine();
        }
    }
}
