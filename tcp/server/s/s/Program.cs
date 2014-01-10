using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

//port:   9520
//目录服务器

namespace s1
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 9520);
            TcpListener s = new TcpListener(ep);
            s.Start();

            TcpClient c1 = s.AcceptTcpClient();
            string c1ep = c1.Client.RemoteEndPoint.ToString() + "#Server";
            //在控制台中显示c1其IP地址和端口号 
            Console.WriteLine("Server Peer: " + c1ep);

            Console.WriteLine();

            TcpClient c2 = s.AcceptTcpClient();
            string c2ep = c2.Client.RemoteEndPoint.ToString() + "#Client";
            //在控制台中显示c1其IP地址和端口号 
            Console.WriteLine("Client Peer: " + c2ep);

            c1ep += "#" + c2ep;
            NetworkStream ns1 = c1.GetStream();
            byte[] sb1 = Encoding.UTF8.GetBytes(c1ep);
            //返回c1其IP地址和端口号 先连入的Client 作为Peer的Server
            ns1.Write(sb1, 0, sb1.Length);

            c2ep += "#" + c1ep;
            NetworkStream ns2 = c2.GetStream();
            byte[] sb2 = Encoding.UTF8.GetBytes(c2ep);
            //返回c2其IP地址和端口号 后连入的Client 作为Peer的Client
            ns2.Write(sb2, 0, sb2.Length);
            //在控制台中显示c2其IP地址和端口号



            //TcpClient c3 = s.AcceptTcpClient();
            //string c3ep = c3.Client.RemoteEndPoint.ToString() + "#Server";
            //NetworkStream ns3 = c3.GetStream();
            //byte[] sb3 = Encoding.UTF8.GetBytes(c3ep);
            ////返回c1其IP地址和端口号 先连入的Client 作为Peer的Server
            //ns3.Write(sb3, 0, sb3.Length);
            ////在控制台中显示c1其IP地址和端口号 
            //Console.WriteLine("Server Peer: " + c3ep);

            Console.Read();
        }
    }
}
