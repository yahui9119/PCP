using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCP.Service;
using PCP.Model;

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
            User user=new User ();
            user.username="admin";
            user.password="123456";
            client.StartServer(user);
            Console.ReadLine();
        }
    }
}
