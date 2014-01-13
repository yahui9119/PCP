using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using P2P.WellKnown;
using System.Threading;

namespace P2P.Client
{
    public class Client : IDisposable
    {
        private const int MaxRetry = 10;
        /// <summary>
        /// 用户数据服务对象
        /// </summary>
        private Socket client;
        /// <summary>
        /// 服务器端点
        /// </summary>
        private IPEndPoint hostPoint;
        /// <summary> 
        /// 客户端
        /// </summary>
        private IPEndPoint remotePoint;
        /// <summary>
        /// 用户列表，所有用户
        /// </summary>
        private UserCollection userList;
        private string myName;
        private bool ReceivedACK;
        private Thread listenThread;
        #region 随机产生未被占用的端口
        private static int RandomPort()
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
        
        #region TCP/UDP检测端口是否重复
        private static bool CheckPort(string tempPort)
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
        #endregion
        #endregion
        public Client(string serverIp)
        {
            ReceivedACK = false;
            remotePoint = new IPEndPoint(IPAddress.Any, RandomPort());
            hostPoint = new IPEndPoint(IPAddress.Parse(serverIp), P2PConsts.SEV_Port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Bind(remotePoint);
            //client.Connect(hostPoint);
            client.Connect(hostPoint);
            client.Close();
            //client.Listen(10);
            Socket p2pserver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            p2pserver.Bind(remotePoint);
            p2pserver.Listen(10);
            Socket p2pclient = p2pserver.Accept();
            Console.WriteLine("打洞："+p2pclient.RemoteEndPoint);
            userList = new UserCollection();
            listenThread = new Thread(new ThreadStart(Run));

        }
        public void Start()
        {
            if (this.listenThread.ThreadState == ThreadState.Unstarted)
            {
                this.listenThread.Start();

                Console.WriteLine("You can input you command:\n");

                Console.WriteLine("Command Type:\"send\",\"exit\",\"getu\"");

                Console.WriteLine("Example : send Username Message");

                Console.WriteLine("          exit");

                Console.WriteLine("          getu");
            }
        }
        public void ConnectToServer(string username, string password)
        {
            myName = username;
            //发送消息到服务器
            LoginMessage loginMsg = new LoginMessage(username, password);
            byte[] buffer = FormatterHelper.Serialize(loginMsg);
            int si= client.Send(buffer);
            byte[] bytes=new byte[2048];
            int ri= client.Receive(bytes);
            GetUsersResponseMessage srvResMsg = (GetUsersResponseMessage)FormatterHelper.Deserialize(bytes);
            //跟新用户列表
            userList = srvResMsg.UserList;
            this.DisplayUsers(userList);
        }
        private void DisplayUsers(P2P.WellKnown.UserCollection users)
        {

            foreach (P2P.WellKnown.User user in users)
            {

                Console.WriteLine("Username: {0}, IP:{1}, Port:{2}", user.UserName, user.NetPoint.Address.ToString(), user.NetPoint.Port);

            }

        }
        private void Run()
        {
            
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
