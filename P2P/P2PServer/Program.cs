

using System;

using System.Net;

using System.Net.Sockets;

using System.Threading;



/// <summary>

/// AppClass 的摘要说明。

/// </summary>

public class AppClass
{

    public static void Main()
    {

        Server server = new Server();

        try
        {

            server.Start();

            Console.ReadLine();

            server.Stop();

        }

        catch
        {

        }

    }

}

/// <summary>

/// Server 的摘要说明。

/// </summary>

public class Server
{

    private UdpClient server;

    private static UserCollection userList;

    private Thread serverThread;

    private IPEndPoint remotePoint;



    public Server()
    {

        userList = new UserCollection();

        remotePoint = new IPEndPoint(IPAddress.Any, 0);

        serverThread = new Thread(new ThreadStart(Run));

    }



    public void Start()
    {

        try
        {

            server = new UdpClient(P2PConsts.SRV_PORT);

            serverThread.Start();

            Console.WriteLine("P2P Server started, waiting client connect...");

        }

        catch (Exception exp)
        {

            Console.WriteLine("Start P2P Server error: " + exp.Message);

            throw exp;

        }

    }



    public void Stop()
    {

        Console.WriteLine("P2P Server stopping...");

        try
        {

            serverThread.Abort();

            server.Close();

            Console.WriteLine("Stop OK.");

        }

        catch (Exception exp)
        {

            Console.WriteLine("Stop error: " + exp.Message);

            throw exp;

        }



    }



    private void Run()
    {

        byte[] buffer = null;

        while (true)
        {

            byte[] msgBuffer = server.Receive(ref remotePoint);

            try
            {

                object msgObj = FormatterHelper.Deserialize(msgBuffer);

                Type msgType = msgObj.GetType();

                if (msgType == typeof(LoginMessage))
                {

                    // 转换接受的消息

                    LoginMessage lginMsg = (LoginMessage)msgObj;

                    Console.WriteLine("has an user login: {0}", lginMsg.UserName);

                    // 添加用户到列表

                    IPEndPoint userEndPoint = new IPEndPoint(remotePoint.Address, remotePoint.Port);

                    User user = new User(lginMsg.UserName, userEndPoint);

                    userList.Add(user);

                    // 发送应答消息

                    GetUsersResponseMessage usersMsg = new GetUsersResponseMessage(userList);

                    buffer = FormatterHelper.Serialize(usersMsg);
                    for (int i = 0; i < usersMsg.UserList.Count; i++)
                    {
                        remotePoint = usersMsg.UserList[i].NetPoint;
                        server.Send(buffer, buffer.Length, remotePoint);
                    }

                }

                else if (msgType == typeof(LogoutMessage))
                {

                    // 转换接受的消息

                    LogoutMessage lgoutMsg = (LogoutMessage)msgObj;

                    Console.WriteLine("has an user logout: {0}", lgoutMsg.UserName);

                    // 从列表中删除用户

                    User lgoutUser = userList.Find(lgoutMsg.UserName);

                    if (lgoutUser != null)
                    {

                        userList.Remove(lgoutUser);

                    }

                }

                else if (msgType == typeof(TranslateMessage))
                {

                    // 转换接受的消息

                    TranslateMessage transMsg = (TranslateMessage)msgObj;

                    Console.WriteLine("{0}(1) wants to p2p {2}", remotePoint.Address.ToString(), transMsg.UserName, transMsg.ToUserName);

                    // 获取目标用户

                    User toUser = userList.Find(transMsg.ToUserName);

                    // 转发Purch Hole请求消息

                    if (toUser == null)
                    {

                        Console.WriteLine("Remote host {0} cannot be found at index server", transMsg.ToUserName);

                    }

                    else
                    {

                        SomeOneCallYouMessage transMsg2 = new SomeOneCallYouMessage(remotePoint);

                        buffer = FormatterHelper.Serialize(transMsg);

                        server.Send(buffer, buffer.Length, toUser.NetPoint);

                    }

                }

                else if (msgType == typeof(GetUsersMessage))
                {

                    // 发送当前用户信息到所有登录客户

                    GetUsersResponseMessage srvResMsg = new GetUsersResponseMessage(userList);

                    buffer = FormatterHelper.Serialize(srvResMsg);

                    foreach (User user in userList)
                    {

                        server.Send(buffer, buffer.Length, user.NetPoint);

                    }

                }

                Thread.Sleep(500);

            }

            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}