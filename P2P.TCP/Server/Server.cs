using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using P2P.WellKnown;
using System.Threading;
namespace P2P.Server
{
    /// <summary>
    /// p2p服务器
    /// </summary>
    public class Server
    {
        /// <summary>
        /// 用户数据服务对像
        /// </summary>
        private Socket server;

        /// <summary>
        /// 用户列表
        /// </summary>
        private UserCollection userList;
        /// <summary>
        /// 服务器线程
        /// </summary>
        private Thread serverThread;
        /// <summary>
        /// 远程的网络端点
        /// </summary>
        private IPEndPoint remotePoint;

        public Server()
        {
            userList = new UserCollection();
            remotePoint = new IPEndPoint(IPAddress.Any, P2PConsts.SEV_Port);
            serverThread = new Thread(new ThreadStart(Run));
        }
        private void Run()
        {
            Console.WriteLine("开始监听本地端口：" + P2PConsts.SEV_Port);
            while (server.IsBound)
            {
                Thread clientThread = new Thread(new ParameterizedThreadStart(AcceptClient));
                Socket clientSocket = server.Accept();
                clientThread.Start(clientSocket);

            }
        }

        private void AcceptClient(object obj)
        {
            Socket client = obj as Socket;
            Console.WriteLine("客户端连接："+client.RemoteEndPoint.ToString());
            while (client.Connected)
            {
                try
                {
                byte[] bytes = new byte[1024];
                int ri = client.Receive(bytes);
                if (ri == 0)
                {

                    Socket p2pcosket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.Disconnect(true);
                    p2pcosket.Connect(client.RemoteEndPoint);
                }
                object msgObj = FormatterHelper.Deserialize(bytes);
                Type msgType = msgObj.GetType();
                IPEndPoint clientIPEndPoint = client.RemoteEndPoint as IPEndPoint;
                if (msgType == typeof(LoginMessage))
                {
                    //用户登入
                    LoginMessage loginMsg = (LoginMessage)msgObj;
                    Console.WriteLine("用户登录：" + loginMsg.UserName);
                    //添加用户到列表
                    userList.Add(new User(loginMsg.UserName, clientIPEndPoint));
                    //发送应答消息
                    GetUsersResponseMessage usersMsg = new GetUsersResponseMessage(userList);
                    byte[] buffer = FormatterHelper.Serialize(usersMsg);
                    int si = client.Send(buffer);
                }
                else
                    if (msgType == typeof(LogoutMessage))
                    {
                        //用户登出
                        LogoutMessage logoutMsg = (LogoutMessage)msgObj;
                        Console.WriteLine("用户注销：" + logoutMsg.UserName);
                        //添加用户到列表
                        User user = userList.Find(logoutMsg.UserName);
                        if (user != null)
                        {
                            userList.Remove(user);
                        }
                        //发送应答消息
                        //GetUsersResponseMessage usersMsg = new GetUsersResponseMessage(userList);
                        //byte[] buffer = FormatterHelper.Serialize(usersMsg);
                        //int si = server.Send(buffer);
                    }
                    else
                        if (msgType == typeof(TranslateMessage))
                        {
                            ///客户端间建立连接
                            //转换接收的消息
                            TranslateMessage transMsg = (TranslateMessage)msgObj;
                            Console.WriteLine("{0}:{1} 想要P2P {2}", clientIPEndPoint.ToString(), transMsg.UserName, transMsg.ToUserName);
                            //获取目标用户
                            User toUser = userList.Find(transMsg.ToUserName);
                            //转发消息
                            if (toUser == null)
                            {
                                Console.WriteLine("此用户不存在：" + transMsg.ToUserName);
                            }
                            else
                            {
                                SomeOneCallYouMessage transMsg2 = new SomeOneCallYouMessage(toUser.NetPoint);
                                int si = client.Send(FormatterHelper.Serialize(transMsg2));
                            }
                        }
                        else 
                            if (msgType == typeof(GetUsersMessage))
                        {
                            GetUsersResponseMessage srvResMsg = new GetUsersResponseMessage(userList);
                            foreach (User item in userList)
                            {
                                client.Send(FormatterHelper.Serialize(srvResMsg));
                            }
                        }
                Thread.Sleep(500);


                }
                catch (Exception ex)
                {
                    Console.WriteLine("信息处理出错："+ex.Message);
                    Thread.Sleep(500);
                }
            }
        }
        public void Start()
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(remotePoint);
                server.Listen(99);
                serverThread.Start();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("开启服务器出错：" + ex.Message);
            }

        }
        public void Stop()
        {
            Console.WriteLine("服务关闭。。。");
            try
            {
                serverThread.Abort();
                server.Close();
                Console.WriteLine("关闭完成"); 
            }
            catch (Exception ex)
            {

                Console.WriteLine("关闭出错："+ex.Message); 
            }
        }
    }
}
