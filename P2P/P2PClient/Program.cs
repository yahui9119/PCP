

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
        try
        {
            //Client client = new Client("202.121.197.133");
            Client client = new Client("115.238.30.123");
            Console.Write("输入用户名:");
            string myname = Console.ReadLine();
            client.ConnectToServer(myname, "mypassword");

            client.Start();

            Console.WriteLine("test arguments");

            while (true)
            {

                string str = Console.ReadLine();

                client.PaserCommand(str);

            }
        }
        catch (Exception ee)
        {
            Console.WriteLine("test arguments" + ee.Message + ee.StackTrace);
        }

    }

}

/// <summary>

/// Client 的摘要说明。

/// </summary>

public class Client : IDisposable
{

    private const int MAXRETRY = 10;

    private UdpClient client;

    private IPEndPoint hostPoint;

    private IPEndPoint remotePoint;

    private UserCollection userList;

    private string myName;

    private bool ReceivedACK;

    private Thread listenThread;



    public Client(string serverIP)
    {

        ReceivedACK = false;

        remotePoint = new IPEndPoint(IPAddress.Any, 0);

        hostPoint = new IPEndPoint(IPAddress.Parse(serverIP), P2PConsts.SRV_PORT);

        client = new UdpClient();
        //client = new TcpClient();
        userList = new UserCollection();

        listenThread = new Thread(new ThreadStart(Run));

    }



    public void Start()
    {

        if (this.listenThread.ThreadState == ThreadState.Unstarted)
        {

            this.listenThread.Start();

            Console.WriteLine("你可以输入一些指令:\n");

            Console.WriteLine("现在支持的指令:\"send\",\"exit\",\"getu\"");

            Console.WriteLine("使用示例 : send Username Message");

            Console.WriteLine("          exit");

            Console.WriteLine("          getu");

        }

    }



    public void ConnectToServer(string userName, string password)
    {

        myName = userName;

        // 发送登录消息到服务器

        LoginMessage lginMsg = new LoginMessage(userName, password);

        byte[] buffer = FormatterHelper.Serialize(lginMsg);

        client.Send(buffer, buffer.Length, hostPoint);

        // 接受服务器的登录应答消息

        buffer = client.Receive(ref remotePoint);

        GetUsersResponseMessage srvResMsg = (GetUsersResponseMessage)FormatterHelper.Deserialize(buffer);

        // 更新用户列表

        userList.Clear();

        foreach (User user in srvResMsg.UserList)
        {

            userList.Add(user);

        }

        this.DisplayUsers(userList);

    }



    /// <summary>

    /// 这是主要的函数：发送一个消息给某个用户(C)

    /// 流程：直接向某个用户的外网IP发送消息，如果此前没有联系过

    /// 那么此消息将无法发送，发送端等待超时。

    /// 超时后，发送端将发送一个请求信息到服务端，要求服务端发送

    /// 给客户C一个请求，请求C给本机发送打洞消息

    /// *以上流程将重复MAXRETRY次

    /// </summary>

    /// <param name="toUserName">对方用户名</param>

    /// <param name="message">待发送的消息</param>

    /// <returns></returns>

    private bool SendMessageTo(string toUserName, string message)
    {

        User toUser = userList.Find(toUserName);

        if (toUser == null)
        {

            return false;

        }

        for (int i = 0; i < MAXRETRY; i++)
        {

            WorkMessage workMsg = new WorkMessage(message);

            byte[] buffer = FormatterHelper.Serialize(workMsg);

            client.Send(buffer, buffer.Length, toUser.NetPoint);



            // 等待接收线程将标记修改

            for (int j = 0; j < 10; j++)
            {

                if (this.ReceivedACK)
                {

                    this.ReceivedACK = false;

                    return true;

                }

                else
                {

                    Thread.Sleep(300);

                }

            }

            // 没有接收到目标主机的回应，认为目标主机的端口映射没有

            // 打开，那么发送请求信息给服务器，要服务器告诉目标主机

            // 打开映射端口（UDP打洞）

            TranslateMessage transMsg = new TranslateMessage(myName, toUserName);

            buffer = FormatterHelper.Serialize(transMsg);

            client.Send(buffer, buffer.Length, hostPoint);

            // 等待对方先发送信息

            Thread.Sleep(100);

        }

        return false;

    }



    public void PaserCommand(string cmdstring)
    {

        cmdstring = cmdstring.Trim();

        string[] args = cmdstring.Split(new char[] { ' ' });

        if (args.Length > 0)
        {

            if (string.Compare(args[0], "exit", true) == 0)
            {

                LogoutMessage lgoutMsg = new LogoutMessage(myName);

                byte[] buffer = FormatterHelper.Serialize(lgoutMsg);

                client.Send(buffer, buffer.Length, hostPoint);

                // do clear something here

                Dispose();

                System.Environment.Exit(0);

            }

            else if (string.Compare(args[0], "send", true) == 0)
            {

                if (args.Length > 2)
                {

                    string toUserName = args[1];

                    string message = "";

                    for (int i = 2; i < args.Length; i++)
                    {

                        if (args[i] == "") message += " ";

                        else message += args[i];

                    }

                    if (this.SendMessageTo(toUserName, message))
                    {

                        Console.WriteLine("Send OK!");

                    }

                    else

                        Console.WriteLine("Send Failed!");

                }

            }

            else if (string.Compare(args[0], "getu", true) == 0)
            {

                GetUsersMessage getUserMsg = new GetUsersMessage(myName);

                byte[] buffer = FormatterHelper.Serialize(getUserMsg);

                client.Send(buffer, buffer.Length, hostPoint);

            }

            else
            {

                Console.WriteLine("Unknown command {0}", cmdstring);

            }

        }

    }



    private void DisplayUsers(UserCollection users)
    {

        foreach (User user in users)
        {

            Console.WriteLine("Username: {0}, IP:{1}, Port:{2}", user.UserName, user.NetPoint.Address.ToString(), user.NetPoint.Port);

        }

    }



    private void Run()
    {

        byte[] buffer;

        while (true)
        {

            buffer = client.Receive(ref remotePoint);

            object msgObj = FormatterHelper.Deserialize(buffer);

            Type msgType = msgObj.GetType();

            if (msgType == typeof(GetUsersResponseMessage))
            {

                // 转换消息

                GetUsersResponseMessage usersMsg = (GetUsersResponseMessage)msgObj;

                // 更新用户列表

                userList.Clear();

                foreach (User user in usersMsg.UserList)
                {

                    userList.Add(user);

                }

                this.DisplayUsers(userList);

            }

            else if (msgType == typeof(SomeOneCallYouMessage))
            {

                // 转换消息

                SomeOneCallYouMessage purchReqMsg = (SomeOneCallYouMessage)msgObj;

                // 发送打洞消息到远程主机

                TrashMessage trashMsg = new TrashMessage();

                buffer = FormatterHelper.Serialize(trashMsg);

                client.Send(buffer, buffer.Length, purchReqMsg.RemotePoint);

            }

            else if (msgType == typeof(WorkMessage))
            {

                // 转换消息

                WorkMessage workMsg = (WorkMessage)msgObj;

                Console.WriteLine("Receive a message: {0}", workMsg.Message);

                // 发送应答消息

                ACKMessage ackMsg = new ACKMessage();

                buffer = FormatterHelper.Serialize(ackMsg);

                client.Send(buffer, buffer.Length, remotePoint);

            }

            else if (msgType == typeof(ACKMessage))
            {

                this.ReceivedACK = true;

            }

            else if (msgType == typeof(TrashMessage))
            {

                Console.WriteLine("Recieve a trash message");

            }

            Thread.Sleep(100);

        }

    }

    #region IDisposable 成员



    public void Dispose()
    {

        try
        {

            this.listenThread.Abort();

            this.client.Close();

        }

        catch

        { }

    }



    #endregion

}