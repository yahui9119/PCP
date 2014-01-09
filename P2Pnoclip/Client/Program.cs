using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    /// <summary>
    /// UDP用户登录事件委托
    /// </summary>
    /// <param name="sender">事件源对象</param>
    /// <param name="e">事件实体</param>
    public delegate void UdpUserLogInDelegate(object sender,
              UDPSockEventArgs e);


    /// <summary>
    /// 一般UDP消息事件委托
    /// </summary>
    /// <param name="sender">事件源对象</param>
    /// <param name="e">事件实体</param>
    public delegate void UdpMessageDelegate(object sender,
              UDPSockEventArgs e);


    /// <summary>
    /// 初始化一个新连接的事件委托
    /// </summary>
    /// <param name="sender">事件源对象</param>
    /// <param name="e">事件实体</param>
    public delegate void UdpNewConnectDelegate(object sender,
             UDPSockEventArgs e);
 

    class Program
    {
        static UDPP2PSock udpSock;
        static void Main(string[] args)
        {
            //创建UDP服务器和客户端
              try
              {
                  Console.WriteLine("请输入ip地址");
                  string strServerIP = Console.ReadLine() ;
                  udpSock = new UDPP2PSock();
                  udpSock.OnUserLogInU += 
                        new UdpUserLogInDelegate(OnUserLogInU);
                  udpSock.OnNewConnectU += 
                        new UdpNewConnectDelegate(OnNewConnectU);
                  udpSock.CreateUDPSever();
                  udpSock.CreateUDPClient(strServerIP, P2PConsts.UDP_SRV_PORT);
              }
              catch (Exception ex)
              {
                  Console.WriteLine(ex.Message);
              }
              //Console.WriteLine("按回车键结束程序");
              //Console.ReadLine();
        }
        public static void OnUserLogInU(object sender, UDPSockEventArgs e)
        {
            Console.WriteLine("用户名：{0}，端口：{1} 信息：{2}",e.RemoteUserName,e.RemoteEndPoint,e.SockMessage);
        }
        public static void OnNewConnectU(object sender, UDPSockEventArgs e)
        {
            Console.WriteLine("新用户名：{0}，端口：{1} 信息：{2}", e.RemoteUserName, e.RemoteEndPoint, e.SockMessage);
        }
    }
}
