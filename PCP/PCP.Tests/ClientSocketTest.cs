using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCP.Service;
using PCP.Model;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using System.Threading;

namespace PCP.Tests
{
    [TestClass]
    public class ClientSocketTest
    {
        //服务器先发socket到客户端，有一个维持双方通信的socket，
        //然后使用websocket  对于client 和服务端位置的socket和本地websocket的socket是一个socket
        //需要实现socket的多功能性
        //需要修改fleck的底层实现
        [TestMethod]
        public void StartServer()
        {
            ClientSocket client = new ClientSocket("http://servershost.sinaapp.com");
            User user = new User();
            user.username = "admin";
            user.password = "123456";
            client.StartServer(user);
            Console.ReadLine();
        }
        Thread threadServer;
        Socket httpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        [TestMethod]
        public void P2PTest()
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 35286);
            httpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            httpSocket.Bind(localEP);
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            ServerSocket.Bind(localEP);
            ServerSocket.Listen(10);
            threadServer = new Thread(new ParameterizedThreadStart(ClientConnectAsy));
            threadServer.Start();
            //var result = HttpRequest("http://servershost.sinaapp.com/Pcp/NeedleStart?username=admin", "get", string.Empty, httpSocket);
            var result = HttpRequest("http://query.ferrygame.com/Validate/Login.aspx", "post", "UserName=yhwang&Password=loveaways", httpSocket);
            Console.ReadLine();
        }
        private void ClientConnectAsy(object state)
        {
            while (true)
            {
                Socket serversocket = ServerSocket.Accept();
                byte[] sendbuffbytes = UTF8Encoding.ASCII.GetBytes("hello world");
                serversocket.Send(sendbuffbytes);
                while (true)
                {
                    byte[] buffbytes = new byte[1024];
                    if (serversocket.Receive(buffbytes) > 0)
                    {
                        string test = UTF8Encoding.ASCII.GetString(buffbytes);
                    }
                }
               
                Thread.Sleep(50);
            }
        }
        public string HttpRequest(string url, string method, string data, Socket socket)
        {
            Regex reg = new Regex(@"(?<=://)([\w-]+\.)+[\w-]+(?<=/?)");

            string server = reg.Match(url, 0).Value.Replace("/", string.Empty);
            if (string.IsNullOrEmpty(server))
            {
                server = "localhost";
            }
            string urlPath = url.Replace(string.Format("http://{0}", server), string.Empty).Replace(":800",string.Empty);
            string gethtml = GetHtml(server, urlPath, method.ToUpper(), data, "", socket);
            return gethtml;
        }
        #region httprequest method
        /// <summary>
        /// Socket获取页面HTML同时返回头信息
        /// </summary>
        /// <param name="server">服务器地址或主机名</param>
        /// <param name="url">请求的页面</param>
        /// <param name="method">post or get</param>
        /// <param name="data">提交的数据</param>
        /// <param name="Cookies">Cookies</param>
        /// <returns>返回页面的HTML</returns>
        public string GetHtml(string server, string url, string method, string data, string Cookies, Socket socket)
        {
            string _method = method.ToUpper();
            string _url = string.Empty;
            if (url == "")
            {
                _url = "/";
            }
            else if (url.Substring(0, 1) != "/")
            {
                _url = "/" + url;
            }
            else
            {
                _url = url;
            }
            StringBuilder formatString = new StringBuilder(); ;
            string sendString = string.Empty;
            Encoding ASCII = Encoding.Default;
            //以下是拼接的HTTP头信息
            if (_method == "GET")
            {
                formatString.Append("");
                formatString.Append("{0} {1} HTTP/1.1\r\n");
                formatString.Append("Host: {2}\r\n");
                formatString.Append("User-Agent:Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.7) Gecko/20091221 Firefox/3.5.7\r\n");
                formatString.Append("Accept: text/html\r\n");
                //formatString.Append("Keep-Alive: 300\r\n");
                formatString.Append("Cookie:{3}\r\n");
                formatString.Append("Connection: keep-alive\r\n\r\n");
                sendString = string.Format(formatString.ToString(), _method, string.Format("{0}?{1}", url, data), server, Cookies);
            }
            else
            {
                formatString.Append("");
                formatString.Append("{0} {1} HTTP/1.1\r\n");
                formatString.Append("Host: {2}\r\n");
                formatString.Append("User-Agent:Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.7) Gecko/20091221 Firefox/3.5.7\r\n");
                formatString.Append("Accept:text/html\r\n");
                formatString.Append("Content-Type:application/x-www-form-urlencoded\r\n");
                formatString.Append("Content-Length:{3}\r\n");
                formatString.Append("Referer:{2}");
                //formatString.Append("Keep-Alive:300\r\n");
                formatString.Append("Cookie:{4}\r\n");
                formatString.Append("Connection: keep-alive\r\n\r\n");
                formatString.Append("{5}\r\n");
                sendString = string.Format(formatString.ToString(), _method, _url, server, Encoding.Default.GetByteCount(data), Cookies, data);
            }
            Byte[] ByteGet = ASCII.GetBytes(sendString);
            Byte[] RecvBytes = new Byte[1024];
            String strRetPage = null;
            IPAddress hostadd = Dns.Resolve(server).AddressList[0];
            IPEndPoint EPhost = new IPEndPoint(hostadd, 80);
            //IPEndPoint EPhost = new IPEndPoint(hostadd, 800);
            try
            {
                socket.Connect(EPhost);
            }
            catch (Exception ex)
            {
                return strRetPage;
            }

            if (!socket.Connected)
            {
                //strRetPage = "链接主机失败";
                return strRetPage;
            }
            socket.Send(ByteGet, ByteGet.Length, SocketFlags.None);
            strRetPage = Recv(socket, ASCII);
            return strRetPage;
        }
        public String Recv(Socket sock, Encoding encode)
        {
            Byte[] buffer = new Byte[8192];
            StringBuilder sb = new StringBuilder();
            Thread.Sleep(50);//根据页面响应时间进行微调
            Int32 len = sock.Receive(buffer);
            sb.Append(encode.GetString(buffer, 0, len));
            while (sock.Available > 0)
            {
                Thread.Sleep(300);//也可以视情况微调
                Array.Clear(buffer, 0, buffer.Length);
                len = sock.Receive(buffer);
                sb.Append(encode.GetString(buffer, 0, len));
                string ss = encode.GetString(buffer, 0, len);
            }
            return sb.ToString();
        }
        #endregion
    }
}
