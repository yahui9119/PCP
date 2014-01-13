using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

//P2P节点

namespace c1
{
    class Program
    {
        #region TCP/UDP检测端口是否重复
        public static bool CheckPort(string tempPort)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("netstat", "-an");
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
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
        public static int RandomPort()
        {
            while (true)
            {
                int second = DateTime.Now.Second;
                Random ran = new Random(second);
                int RandPort = ran.Next(8000, 10000);
                if (CheckPort(RandPort.ToString()) == false)
                {
                    return RandPort;
                }
            }
        }
        #endregion

        #region 获取本地IPv4地址
        private static string[] GetHostIPv4()
        {

            int j = 0;
            string strHostName = Dns.GetHostName();  //得到本机的主机名
            IPHostEntry ipEntry = Dns.GetHostByName(strHostName); //取得本机IP
            string[] ip = new string[ipEntry.AddressList.Length];
            foreach (IPAddress i in ipEntry.AddressList)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                    ip.SetValue(i.ToString(), j++);
            }
            return ip;
        }
        #endregion

        #region 域名解析函数
        public static string Domain2Ip(string str)
        {
            string _return = "";
            try
            {
                IPHostEntry hostinfo = Dns.GetHostEntry(str);
                IPAddress[] aryIP = hostinfo.AddressList;
                _return = aryIP[0].ToString();
            }
            catch (Exception e)
            {
                _return = e.Message;
            }
            return _return;
        } 
        #endregion

        #region 判断是否为IPv4地址
        public static bool IsIPv4(string str)
        {
            //单独的一个数字（表示0-9）
            //一个非零数字后紧跟着另外一个数字（表示10-99）
            //"1"后面跟着两个数字（表示100-199）
            //"2"后面跟着一个"0"到"4"间的数字，后面又跟着一个数字（表示200-249）
            //"25"后面跟着一个"0"到"5"间的数字（表示250-255）
            //    根据上面的分析，来试着写匹配0-255的正则表达式：
            //^\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]$
            //将前两项合并，上式变成：
            //^[1-9]?\d|1\d\d|2[0-4]\d|25[0-5]$
            //然后，将上面这个式子重复4次，中间以点号隔开即可，如下所示：
            string regipv4 = "^([1-9]?\\d|1\\d\\d|2[0-4]\\d|25[0-5])\\.([1-9]?\\d|1\\d\\d|2[0-4]\\d|25[0-5])\\.([1-9]?\\d|1\\d\\d|2[0-4]\\d|25[0-5])\\.([1-9]?\\d|1\\d\\d|2[0-4]\\d|25[0-5])$";
            return Regex.IsMatch(str, regipv4);
        }
        #endregion

        #region PeerClient监听线程ClientListen
        public static void ClientListen(object s)
        {
            byte[] rb = new byte[1024];
            Socket socket1 = (Socket)s;
            socket1.Listen(20);
            Socket Peer = socket1.Accept();
            //发送PeerClient欢迎消息
            Console.WriteLine("\nPeer Server尝试连接成功,表明你们在同一NAT下,Peer Client已将欢迎消息发送给Peer Server");
            byte[] sb1 = Encoding.UTF8.GetBytes("Hello, I'm Peer Client.");//无论如何此Peer一直叫Client
            Peer.Send(sb1, sb1.Length, 0);
            //接收PeerClient的消息
            Peer.Receive(rb, 1024, 0);
            Console.WriteLine("接收到来自Peer Server的消息：");
            Console.WriteLine(Encoding.UTF8.GetString(rb).Replace("\0", ""));
            //Console.Read();
        }
        #endregion

        #region 线程间传递参数DataStruck
        class DataStruck
        {
            public Socket s;
            public Thread t;
            public string ip;
            public int port;
            public IPEndPoint localEP;
        }
        #endregion

        #region 用于关闭PeerClient的监听线程, 开启ClientConnect连接线程的时间触发线程
        public static void KillClientListen(object ds)
        {
            DataStruck DS = (DataStruck)ds;
            if (DS.t.IsAlive)
            {
                Console.WriteLine("在10s秒之内没有接到PeerServer的连接请求, 或者请求被NAT拒绝, 正在关闭Socket监听线程(ClientListen)");
                DS.t.Abort();
                DS.s.Close();
                //循环线程连接peerserver报第几次连接失败
                Console.WriteLine("Peer Client 开启连接Peer Server 线程--ClientConnect");
                Thread t = new Thread(new ParameterizedThreadStart(ClientConnect));
                t.Start(DS);
            }
        }
        #endregion

        #region PeerClient连接PeerServer的打洞线程ClientConnect
        public static void ClientConnect(object ds)
        {
            byte[] rb = new byte[1024];
            DataStruck DS = (DataStruck)ds;
            Socket PeerC = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            PeerC.Bind(DS.localEP);
            //创建PeerServerEP
            Console.WriteLine("尝试连接Peer server(打洞过程)");
            Console.WriteLine("PeerServer的地址：" + DS.ip);
            string c2 = DS.ip;
            Console.WriteLine("端口号：" + DS.port);
            int port = Convert.ToInt32(DS.port);
            IPEndPoint PeerEP = new IPEndPoint(IPAddress.Parse(c2), port);

            //连入PeerServer
            try
            {
                PeerC.Connect(PeerEP);
                Console.WriteLine("尝试连接Peer server成功(打洞成功)");
                //接收PeerServer的消息
                Console.WriteLine("接收到来自Peer server的消息：");
                PeerC.Receive(rb, 1024, 0);
                Console.WriteLine(Encoding.UTF8.GetString(rb).Replace("\0", ""));
                //发送PeerServer欢迎消息
                Console.WriteLine("发送给Peer server的问候消息：");
                byte[] sb = Encoding.UTF8.GetBytes("I'm Peer Client，Nace to meet you！");
                PeerC.Send(sb, sb.Length, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("尝试连接Peer server失败,打洞失败...");
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        static void Main(string[] args)
        {
            Socket socket1;
            //构建本地Peer EP 分配随机未被利用的端口号
            int LocalPort = RandomPort();
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, LocalPort);
            Console.WriteLine("构建本地EP成功,localEP：" + localEP.ToString());
            //构建构建目录服务器server EP
            Console.WriteLine("输入Directory Server的IP地址(请不要输入域名,默认为shiwei012.eicp.net),Directory Server的开放端口为：9520");
            string ServerIP = Console.ReadLine();
            if (ServerIP == "")
            {
                ServerIP = Domain2Ip("shiwei012.eicp.net");
                if (!IsIPv4(ServerIP))
                {
                    Console.WriteLine("shiwei012.eicp.net服务器未开启\n请尝试手动输入Directory Server IP,程序结束");
                    Console.Read();
                    return;
                }
            }
            else
            {
                if (!IsIPv4(ServerIP))
                {
                    Console.WriteLine("输入地址错误,程序结束");
                    Console.Read();
                    return;
                }
            }
            IPEndPoint ServerEP = new IPEndPoint(IPAddress.Parse(ServerIP), 9520);//192.168.4.182
            socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket1.Bind(localEP);

            //连接目录服务器server
            byte[] rb = new byte[1024];
            try
            {
                socket1.Connect(ServerEP);
            }
            catch (Exception ex)
            {
                Console.WriteLine("连接Directory Server失败,ERROR如下：");
                Console.WriteLine(ex.ToString());
                Console.Read();
                return;
            }

            Console.WriteLine("连接Directory Server成功");

            //输出目录服务器server返回的本地EP
            socket1.Receive(rb, 1024, 0);
            string[] RecP = Encoding.UTF8.GetString(rb).Replace("\0", "").Split(new char[] { '#' });
            Console.WriteLine("Directory server 返回你的EP为：" + RecP[0] + "\nYou're " + RecP[1] + ".");

            //继续使用此socket但是关闭与目录服务器的连接
            Console.WriteLine("正在关闭你与Directory Server的连接(此时已得知两个Peer在公网中的EP,关闭连接是为了复用此端口)");
            socket1.Close();

            //建立EP和socket1相同的socket2
            Socket socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket2.Bind(localEP);

            if ("Server" == RecP[1])
            {
                string[] CEP = RecP[2].Split(new char[] { ':' });
                //连接client
                //取得peer client server 的ep
                Console.WriteLine("\nPeerServer尝试连接PeerClient\nPeerClient的EP：" + CEP[0]);
                string c2 = CEP[0];
                Console.WriteLine("端口号：" + CEP[1]+"\n");
                int port = Convert.ToInt32(CEP[1]);
                IPEndPoint PeerEP = new IPEndPoint(IPAddress.Parse(c2), port);

                //尝试连接
                try
                {
                    socket2.Connect(PeerEP);

                    //连接成功
                    Console.WriteLine("\n尝试连接成功,表明你们在同一NAT下,Peer Server已将欢迎消息发送给Peer Client");
                    byte[] sb2 = Encoding.UTF8.GetBytes("Hello, I'm Peer Server.");
                    socket2.Send(sb2, sb2.Length, 0);
                    //接收PeerClient的消息
                    Array.Clear(rb, 0, rb.Length);
                    socket2.Receive(rb, 1024, 0);
                    Console.WriteLine("接收到来自Peer Client的消息：");
                    Console.WriteLine(Encoding.UTF8.GetString(rb).Replace("\0", ""));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Peer Server 连接 Peer Client Server 失败(打洞过程)");
                    //连接失败,开启accept
                    Console.WriteLine("Peer Server 开启 Socket ... 等待 Peer Client 连入");
                    //等待PeerClient
                    socket2.Listen(20);
                    Socket Peer = socket2.Accept();
                    //发送PeerClient欢迎消息
                    Console.WriteLine("Peer Server 接收到Peer Client连入(打洞成功),Peer Server已将欢迎消息发送给Peer Client");
                    byte[] sb1 = Encoding.UTF8.GetBytes("Hello, I'm Peer Server.");//无论如何此Peer一直叫Server
                    Peer.Send(sb1, sb1.Length, 0);
                    //接收PeerClient的消息
                    Array.Clear(rb, 0, rb.Length);
                    Peer.Receive(rb, 1024, 0);
                    Console.WriteLine("接收到来自Peer Client的消息：");
                    Console.WriteLine(Encoding.UTF8.GetString(rb).Replace("\0", ""));
                }
            }

            if ("Client" == RecP[1])
            {
                string[] SEP = RecP[2].Split(new char[] { ':' });
                //开启server线程 在指定时间10s内未有Peer 连入则关闭accept 表明不能直接通信
                Console.WriteLine("Peer Client已开启Socket监听线程ClientListen, 等待Peer Server的连入");
                Thread tcl = new Thread(new ParameterizedThreadStart(ClientListen));
                tcl.Start(socket2);

                DataStruck ds = new DataStruck();
                ds.s = socket2;
                ds.t = tcl;
                ds.ip = SEP[0];
                ds.port = Convert.ToInt32(SEP[1]);
                ds.localEP = localEP;

                Timer aTimer = new Timer(new TimerCallback(KillClientListen), ds, 10000, 0);
            }

            //防止异常关闭线程
            Console.Read();
        }
    }
}
