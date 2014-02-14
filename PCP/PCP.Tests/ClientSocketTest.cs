using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCP.Service;
using PCP.Model;

namespace PCP.Tests
{
    [TestClass]
    public class ClientSocketTest
    {

        [TestMethod]
        public void StartServer()
        {
            ClientSocket client = new ClientSocket("http://servershost.sinaapp.com");
            User user=new User ();
            user.username="admin";
            user.password="123456";
            client.StartServer(user);
        }
    }
}
