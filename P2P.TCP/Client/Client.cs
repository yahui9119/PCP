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
        public Client(string serverIp)
        {
            ReceivedACK = false;
            remotePoint = new IPEndPoint(IPAddress.Any, 0);
            hostPoint = new IPEndPoint(IPAddress.Parse(serverIp), P2PConsts.SEV_Port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //client.Connect(hostPoint);
            client.Connect(hostPoint);
            client.Bind(remotePoint);
            client.Listen(10);
            
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
