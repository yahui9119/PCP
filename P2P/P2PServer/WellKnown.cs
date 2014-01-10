using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// P2PConsts 的摘要说明。
/// </summary>
public class P2PConsts
{

    /// <summary>

    /// 服务器侦听端口号

    /// </summary>

    public const int SRV_PORT = 8245;

}

/// <summary>

/// User 的摘要说明。

/// </summary>

[Serializable]

public class User
{

    protected string userName;

    protected IPEndPoint netPoint;



    public User(string UserName, IPEndPoint NetPoint)
    {

        this.userName = UserName;

        this.netPoint = NetPoint;

    }

    public string UserName
    {

        get { return userName; }

    }



    public IPEndPoint NetPoint
    {

        get { return netPoint; }

        set { netPoint = value; }

    }

}

/// <summary>

/// UserCollection 的摘要说明。

/// </summary>

[Serializable]

public class UserCollection : CollectionBase
{

    public void Add(User user)
    {

        InnerList.Add(user);

    }



    public void Remove(User user)
    {

        InnerList.Remove(user);

    }



    public User this[int index]
    {

        get { return (User)InnerList[index]; }

    }



    public User Find(string userName)
    {

        foreach (User user in this)
        {

            if (string.Compare(userName, user.UserName, true) == 0)
            {

                return user;

            }

        }

        return null;

    }

}

/// <summary>

/// FormatterHelper 序列化，反序列化消息的帮助类

/// </summary>

[Serializable]
public class FormatterHelper
{
    public static byte[] Serialize(object obj)
    {

        BinaryFormatter binaryF = new BinaryFormatter();

        MemoryStream ms = new MemoryStream(1024 * 10);

        binaryF.Serialize(ms, obj);

        ms.Seek(0, SeekOrigin.Begin);

        byte[] buffer = new byte[(int)ms.Length];

        ms.Read(buffer, 0, buffer.Length);

        ms.Close();

        return buffer;

    }

    public static object Deserialize(byte[] buffer)
    {

        BinaryFormatter binaryF = new BinaryFormatter();

        MemoryStream ms = new MemoryStream(buffer, 0, buffer.Length, false);

        object obj = binaryF.Deserialize(ms);

        ms.Close();

        return obj;

    }

}

/// <summary>

/// Message base class

/// </summary>

[System.Serializable]

public abstract class MessageBase
{

}



/// <summary>

/// 客户端发送到服务器的消息基类

/// </summary>
[System.Serializable]
public abstract class CSMessage : MessageBase
{

    private string userName;

    protected CSMessage(string anUserName)
    {

        userName = anUserName;

    }

    public string UserName
    {

        get { return userName; }

    }

}

/// <summary>

/// 用户登录消息

/// </summary>
[System.Serializable]
public class LoginMessage : CSMessage
{

    private string password;

    public LoginMessage(string userName, string password)
        : base(userName)
    {

        this.password = password;

    }

    public string Password
    {

        get { return password; }

    }

}

/// <summary>

/// 用户登出消息

/// </summary>
[System.Serializable]
public class LogoutMessage : CSMessage
{

    public LogoutMessage(string userName)
        : base(userName)

    { }

}

/// <summary>

/// 请求用户列表消息

/// </summary>
[System.Serializable]
public class GetUsersMessage : CSMessage
{

    public GetUsersMessage(string userName)
        : base(userName)

    { }

}

/// <summary>

/// 请求Purch Hole消息

/// </summary>
[System.Serializable]
public class TranslateMessage : CSMessage
{

    protected string toUserName;

    public TranslateMessage(string userName, string toUserName)
        : base(userName)
    {

        this.toUserName = toUserName;

    }

    public string ToUserName
    {

        get { return this.toUserName; }

    }

}



/// <summary>

/// 服务器发送到客户端消息基类

/// </summary>
[System.Serializable]
public abstract class SCMessage : MessageBase

{ }

/// <summary>

/// 请求用户列表应答消息

/// </summary>
[System.Serializable]
public class GetUsersResponseMessage : SCMessage
{

    private UserCollection userList;

    public GetUsersResponseMessage(UserCollection users)
    {

        this.userList = users;

    }

    public UserCollection UserList
    {

        get { return userList; }

    }

}

/// <summary>

/// 转发请求Purch Hole消息

/// </summary>
[System.Serializable]
public class SomeOneCallYouMessage : SCMessage
{

    protected System.Net.IPEndPoint remotePoint;

    public SomeOneCallYouMessage(System.Net.IPEndPoint point)
    {

        this.remotePoint = point;

    }

    public System.Net.IPEndPoint RemotePoint
    {

        get { return remotePoint; }

    }

}



/// <summary>

/// 点对点消息基类

/// </summary>
[System.Serializable]
public abstract class PPMessage : MessageBase

{ }

/// <summary>

/// 测试消息

/// </summary>
[System.Serializable]
public class WorkMessage : PPMessage
{

    private string message;

    public WorkMessage(string msg)
    {

        message = msg;

    }

    public string Message
    {

        get { return message; }

    }

}

/// <summary>

/// 测试应答消息

/// </summary>
[System.Serializable]
public class ACKMessage : PPMessage
{

}

/// <summary>

/// P2P Purch Hole Message

/// </summary>
[System.Serializable]
public class TrashMessage : PPMessage

{ }

