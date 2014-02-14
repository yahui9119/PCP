using PCP.Common;
using PCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PCP.Service
{
    public  class ClientSocket
    {
        private string HostDoMain { get; set; }
        IPEndPoint localEP;
        /// <summary>
        /// 初始化 同时绑定端口
        /// </summary>
        public ClientSocket(string HostDomain)
        {
            this.HostDoMain = HostDomain;
            ClientPort = RandomPort();
            localEP = new IPEndPoint(IPAddress.Any, ClientPort);
            httpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            httpSocket.Bind(localEP);

            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //请注意这一句。ReuseAddress选项设置为True将允许将套接字绑定到已在使用中的地址。 
            ServerSocket.Bind(localEP);
        }
        /// <summary>
        /// 客户端可用端口
        /// </summary>
        private  int ClientPort;
        #region TCP/UDP检测端口是否重复
        public  bool CheckPort(string tempPort)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo("netstat", "-an");
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            string result = p.StandardOutput.ReadToEnd().ToLower();//最后都转换成小写字母
            string[] addressList = GetHostIPv4();
            List<string> ipList = new List<string>();
            ipList.Add("127.0.0.1");
            ipList.Add("0.0.0.0");
            for (int i = 0; i < addressList.Length; i++)
            {
                ipList.Add(addressList[i].ToString());
            }
            bool use = false;
            for (int i = 0; i < ipList.Count; i++)
            {
                if (result.IndexOf("tcp    " + ipList[i] + ":" + tempPort) >= 0 || result.IndexOf("udp    " + ipList[i] + ":" + tempPort) >= 0)
                {
                    use = true;
                    break;
                }
            }
            p.Close();
            return use;
        }
        #endregion

        #region 随机产生未被占用的端口
        public  int RandomPort()
        {
            while (true)
            {
                int second = DateTime.Now.Second;
                Random ran = new Random(second);
                int RandPort = ran.Next(1024, 65535);
                if (CheckPort(RandPort.ToString()) == false)
                {
                    return RandPort;
                }
            }
        }
        #endregion
        #region 获取本地IPv4地址
        public  string[] GetHostIPv4()
        {

            int j = 0;
            string strHostName = System.Net.Dns.GetHostName();  //得到本机的主机名
            System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostByName(strHostName); //取得本机IP
            string[] ip = new string[ipEntry.AddressList.Length];
            foreach (IPAddress i in ipEntry.AddressList)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                    ip.SetValue(i.ToString(), j++);
            }
            return ip;
        }
        #endregion

        #region socket模拟http
        //static string strPath;
        //static string strServer;
        /// <summary>
        /// 从返回的源代码中提取cookies
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private  string GetCookies(string s)
        {
            StringBuilder sbCookies = new StringBuilder();

            string[] arr = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in arr)
            {
                if (str.StartsWith("Set-Cookie: "))
                {
                    int intStart = str.IndexOf(";");
                    string strCookie = str.Substring(12, intStart - 11);
                    sbCookies.Append(strCookie);
                }
            }
            return sbCookies.ToString();
        }
        private  string GetLocationURL(string s)
        {

            string RtnString = string.Empty;
            StringBuilder sbCookies = new StringBuilder();

            string[] arr = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in arr)
            {
                if (str.StartsWith("Location: "))
                {
                    RtnString = str.Substring(11, str.Length - 11);
                }
            }
            return RtnString;
        }
        //private static void getServerAndPath(string strURL)
        //{
        //    if (strURL != "" && strURL.IndexOf("/") > 0)
        //    {
        //        int SlashPos = strURL.Substring(7).IndexOf("/");
        //        strServer = strURL.Substring(7, SlashPos);
        //        strPath = strURL.Substring(SlashPos, 7);
        //    }
        //    else
        //        return;
        //}
        /// <summary>
        /// 模拟http请求
        /// </summary>
        /// <param name="url">http链接，网站地址 http://开头</param>
        /// <param name="method">请求方式：支持GET或POST</param>
        /// <param name="data">请求数据</param>
        /// <param name="socket">使用的socket，默认随机实例化</param>
        /// <returns></returns>
        public  Result HttpRequest(string url, string method, string data)
        {
            Regex reg = new Regex(@"(?<=://)([\w-]+\.)+[\w-]+(?<=/?)");

            string server = reg.Match(url, 0).Value.Replace("/", string.Empty);
            string urlPath = url.Replace(string.Format("http://{0}",server), string.Empty);
            httpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            httpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            httpSocket.Bind(localEP);
            string gethtml = GetHtml(server, urlPath, method.ToUpper(), data, cookies, httpSocket);
            cookies= GetCookies(gethtml);
            int startjson=gethtml.IndexOf('{');
            int endjson=gethtml.LastIndexOf('}');
            string jsonresult = gethtml.Substring(startjson, endjson-startjson+1);
            Result result= Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(jsonresult);
            return result;
        }
        /// <summary>
        /// Socket获取页面HTML同时返回头信息
        /// </summary>
        /// <param name="server">服务器地址或主机名</param>
        /// <param name="url">请求的页面</param>
        /// <param name="method">post or get</param>
        /// <param name="data">提交的数据</param>
        /// <param name="Cookies">Cookies</param>
        /// <returns>返回页面的HTML</returns>
        public  string GetHtml(string server, string url, string method, string data, string Cookies,Socket socket)
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
                sendString = string.Format(formatString.ToString(), _method,string.Format("{0}?{1}",url,data), server, Cookies);
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
                formatString.Append( "Referer:{2}");
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
            Socket s = socket==null?new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp):socket;
            s.Connect(EPhost);
            if (!s.Connected)
            {
                strRetPage = "链接主机失败";
                return strRetPage;
            }
            s.Send(ByteGet, ByteGet.Length, SocketFlags.None);
            strRetPage = Recv(s, ASCII);
            return strRetPage;
        }
        public  String Recv(Socket sock, Encoding encode)
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
            sock.Close();
            return sb.ToString();
        }
        #endregion
        private  string cookies = string.Empty;
        private  Socket httpSocket =  new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        /// <summary>
        /// http针头是否开启
        /// </summary>
        private  bool IsHttpNeedleStart;
        /// <summary>
        /// http线程
        /// </summary>
        private  Thread HttpThread;
        /// <summary>
        /// 本地服务器线程
        /// </summary>
        private  Thread ServerTread;
        /// <summary>
        /// 开启本地服务
        /// </summary>
        public  void StartServer(User user)
        {
            //Timer timer = new Timer(HttpNeedle,user,0,11000);
            HttpNeedle(user);
        }
        public void HttpNeedle(object state)
        {
            User user = (User)state;
            if (user == null)
            {
                return;
            }
            Result gethtml = HttpRequest(string.Format("{0}/Pcp/Login", HostDoMain), "GET", string.Format("username={0}&password={1}", user.username, user.password));
            Result getgetOnlinehtml = HttpRequest(string.Format("{0}/Pcp/Online", HostDoMain), "GET", string.Empty);
        }
    }
}
