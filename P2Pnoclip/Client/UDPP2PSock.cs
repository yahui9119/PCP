using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    /// <summary>
    /// UDPP2P套接字管理类
    /// </summary>
    public class UDPP2PSock
    {
        /// <summary>
        /// 用户登录事件
        /// </summary>
        public event UdpUserLogInDelegate OnUserLogInU;


        /// <summary>
        /// 一般UDP消息事件
        /// </summary>
        public event UdpMessageDelegate OnSockMessageU;


        /// <summary>
        /// 初始化一个新连接事件
        /// </summary>
        public event UdpNewConnectDelegate OnNewConnectU;


        /// <summary>
        /// UDP服务器
        /// </summary>
        private UdpClient m_udpServer;


        /// <summary>
        /// UDP客户端
        /// </summary>
        private UdpClient m_udpClient;


        /// <summary>
        /// 服务器实际上在本地机器上监听的
        /// 端口，用于当一台计算机上同时启
        /// 动两个可两以上服务器进程时，标
        /// 识不同的服务器进程
        /// </summary>
        private int m_iMyServerPort;


        /// <summary>
        /// 客户端在本地机器上实际使用的端口，
        /// 用于当一台计算机上同时有两个或两
        /// 个以上客户端进程在运行时，标识不
        /// 同的客户端进程
        /// </summary>
        private int m_iMyClientPort;


        /// <summary>
        /// 标识是否已成功创服务器
        /// </summary>
        private bool m_bServerCreated;


        /// <summary>
        /// 标识是否已成功创建客户端
        /// </summary>
        private bool m_bClientCreated;


        /// <summary>
        /// 服务器使用的线程
        /// </summary>
        private Thread m_serverThread;


        /// <summary>
        /// 客户端使用的线程
        /// </summary>
        private Thread m_clientThread;


        /// <summary>
        /// 打洞线程
        /// </summary>
        //private Thread m_burrowThread;


        /// <summary>
        /// 远端节点
        /// </summary>
        private IPEndPoint m_remotePoint;


        /// <summary>
        /// 当前进程作为客户端的公共终端
        /// </summary>
        private string m_strMyPublicEndPoint;


        /// <summary>
        /// 当前进程作为客户端的私有终端
        /// </summary>
        private string m_strMyPrivateEndPoint;


        /// <summary>
        /// 用于接受信息的StringBuilder实例
        /// </summary>
        private StringBuilder m_sbResponse = new StringBuilder();


        /// <summary>
        /// P2P打洞时标识是否收到回应消息
        /// </summary>
        private bool m_bRecvAck = false;


        /// <summary>
        /// 请求向其方向打洞的私有终端
        /// </summary>
        private IPEndPoint m_requestPrivateEndPoint;


        /// <summary>
        /// 请求向其方向打洞的公共终端
        /// </summary>
        private IPEndPoint m_requestPublicEndPoint;


        /// <summary>
        /// 打洞消息要发向的节点
        /// </summary>
        private ToEndPoint m_toEndPoint;


        /// <summary>
        /// 用于标识是否已经和请求客户端建立点对连接
        /// </summary>
        //private bool m_bHasConnected=false ;


        /// <summary>
        /// 创建服务器或客户端的最大尝试
        /// 次数，为（65536-60000），防止
        /// 因不能创建而限入死循环或使用
        /// 无效端口
        /// </summary>
        private const int MAX_CREATE_TRY = 5536;


        /// <summary>
        /// 打洞时尝试连接的最大尝试次数
        /// </summary>
        private const int MAX_CONNECT_TRY = 10;





        /// <summary>
        /// 构造函数，初始化UDPP2P实例
        /// </summary>
        public UDPP2PSock()
        {
            m_iMyServerPort = P2PConsts.UDP_SRV_PORT;
            m_iMyClientPort = 60000;
            m_bClientCreated = false;
            m_bServerCreated = false;
            m_toEndPoint = new ToEndPoint();
            m_serverThread = new Thread(new ThreadStart(RunUDPServer));
            m_clientThread = new Thread(new ThreadStart(RunUDPClient));
            //m_burrowThread = new Thread(new ThreadStart(BurrowProc));
        }


        /// <summary>
        /// 创建UDP服务器
        /// </summary>
        public void CreateUDPSever()
        {
            int iTryNum = 0;
            //开始尝试创建服务器
            while (!m_bServerCreated && iTryNum < MAX_CREATE_TRY)
            {
                try
                {
                    m_udpServer = new UdpClient(m_iMyServerPort);
                    m_bServerCreated = true;
                }
                catch
                {
                    m_iMyServerPort++;
                    iTryNum++;
                }
            }
            //创建失败，抛出异常
            if (!m_bServerCreated && iTryNum == MAX_CREATE_TRY)
            {
                throw new Exception("创建服务器尝试失败！");
            }
            m_serverThread.Start();

        }


        /// <summary>
        /// 创建UDP客户端
        /// </summary>
        /// <param name="strServerIP">服务器IP</param>
        /// <param name="iServerPort">服务器端口</param>
        public void CreateUDPClient(string strServerIP, int iServerPort)
        {
            int iTryNum = 0;


            //开始尝试创建服务器
            while (!m_bClientCreated && iTryNum < MAX_CREATE_TRY)
            {
                try
                {
                    m_udpClient = new UdpClient(m_iMyClientPort);
                    m_bClientCreated = true;
                    string strIPAddress =
                       (System.Net.Dns.GetHostAddresses("localhost")[0]).
                         ToString();
                    m_strMyPrivateEndPoint = strIPAddress + ":" +
                        m_iMyClientPort.ToString();
                }
                catch
                {
                    m_iMyClientPort++;
                    iTryNum++;
                }
            }


            //创建失败，抛出异常
            if (!m_bClientCreated && iTryNum == MAX_CREATE_TRY)
            {
                throw new Exception("创建客户端尝试失败！");
            }


            IPEndPoint hostPoint =
                new IPEndPoint(IPAddress.Parse(strServerIP), iServerPort);

            string strLocalIP =
               (System.Net.Dns.GetHostAddresses("localhost"))[0].ToString();
            SendLocalPoint(strLocalIP, m_iMyClientPort, hostPoint);
            m_clientThread.Start();
        }





        /// <summary>
        /// 运行UDP服务器
        /// </summary>
        private void RunUDPServer()
        {
            while (true)
            {
                byte[] msgBuffer = m_udpServer.Receive(ref m_remotePoint);
                m_sbResponse.Append(System.Text.Encoding.
                       Default.GetString(msgBuffer));
                CheckCommand();
                Thread.Sleep(10);
            }
        }





        /// <summary>
        /// 运行UDP客户端
        /// </summary>
        private void RunUDPClient()
        {
            while (true)
            {
                byte[] msgBuffer = m_udpClient.Receive(ref m_remotePoint);
                m_sbResponse.Append(System.Text.Encoding.
                        Default.GetString(msgBuffer));
                CheckCommand();
                Thread.Sleep(10);
            }
        }





        /// <summary>
        /// 销毁UDP服务器
        /// </summary>
        public void DisposeUDPServer()
        {
            m_serverThread.Abort();
            m_udpServer.Close();
        }


        /// <summary>
        /// 销毁UDP客房端
        /// </summary>
        public void DisposeUDPClient()
        {
            m_clientThread.Abort();
            m_udpClient.Close();
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="strMsg">消息内容</param>
        /// <param name="REP">接收节点</param>
        public void SendData(string strMsg, IPEndPoint REP)
        {
            byte[] byMsg =
               System.Text.Encoding.Default.GetBytes(strMsg.ToCharArray());
            m_udpClient.Send(byMsg, byMsg.Length, REP);
        }





        /// <summary>
        /// 发送消息，服务器专用
        /// </summary>
        /// <param name="strMsg">消息内容</param>
        /// <param name="REP">接收节点</param>
        private void ServerSendData(string strMsg, IPEndPoint REP)
        {
            byte[] byMsg =
               System.Text.Encoding.Default.GetBytes(strMsg.ToCharArray());
            m_udpServer.Send(byMsg, byMsg.Length, REP);
        }





        /// <summary>
        /// 发送本地节点信息
        /// </summary>
        /// <param name="strLocalIP">本地IP</param>
        /// <param name="iLocalPort">本地端口</param>
        public void SendLocalPoint(string strLocalIP,
             int iLocalPort, IPEndPoint REP)
        {
            string strLocalPoint = "\x01\x02" + strLocalIP + ":" +
                     iLocalPort.ToString() + "\x02\x01";
            SendData(strLocalPoint, REP);
        }


        /// <summary>

        /// 同时向指定的终端（包括公共终端和私有终端）打洞
        /// </summary>
        /// <param name="pubEndPoint">公共终端</param>
        /// <param name="prEndPoint">私有终端</param>
        /// <returns>打洞成功返回true,否则返回false</returns>
        public void StartBurrowTo(IPEndPoint pubEndPoint,
             IPEndPoint prEndPoint)
        {
            Thread burrowThread = new Thread(new ThreadStart(BurrowProc));
            m_toEndPoint.m_privateEndPoint = prEndPoint;
            m_toEndPoint.m_publicEndPoint = pubEndPoint;
            burrowThread.Start();
        }




        /// <summary>
        /// 打洞线程
        /// </summary>
        private void BurrowProc()
        {
            IPEndPoint prEndPoint = m_toEndPoint.m_privateEndPoint;
            IPEndPoint pubEndPoint = m_toEndPoint.m_publicEndPoint;
            int j = 0;
            for (int i = 0; i < MAX_CONNECT_TRY; i++)
            {
                SendData("\x01\x07\x07\x01", prEndPoint);
                SendData("\x01\x07\x07\x01", pubEndPoint);


                //等待接收线程标记修改
                for (j = 0; j < MAX_CONNECT_TRY; j++)
                {
                    if (m_bRecvAck)
                    {
                        m_bRecvAck = false;
                        SendData("\x01\x07\x07\x01", prEndPoint);
                        Thread.Sleep(50);
                        SendData("\x01\x07\x07\x01", pubEndPoint);


                        UDPSockEventArgs args = new UDPSockEventArgs("");

                        args.RemoteEndPoint = pubEndPoint;
                        if (OnNewConnectU != null)
                        {
                            OnNewConnectU(this, args);
                        }
                        //Thread .Sleep (System .Threading.Timeout .Infinite );
                        return;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }


                //如果没有收到目标主机的回应，表明本次打
                //洞尝试失败,等待100毫秒后尝试下一次打洞
                Thread.Sleep(100);
            }


            //MAX_CONNECT_TRY尝试都失败，表明打洞失败，抛出异常
            //throw new Exception("打洞失败！");
            Console.WriteLine("打洞失败！");////////////
        }



        /// <summary>
        /// 转发打洞请求消息，在服务器端使用
        /// </summary>
        /// <param name="strSrcPrEndpoint">请求转发的源私有终端</param>
        /// <param name="strSrcPubEndPoint">请求转发的源公共终端</param>
        /// <param name="REP">转发消息到达的目的终端</param>
        public void SendBurrowRequest(string strSrcPrEndpoint,
                 string strSrcPubEndPoint, IPEndPoint REP)
        {
            string strBurrowMsg = "\x04\x07" + strSrcPrEndpoint + " " +
                 strSrcPubEndPoint + "\x07\x04";
            ServerSendData(strBurrowMsg, REP);
        }





        /// <summary>
        /// 检查字符串中的命令
        /// </summary>
        private void CheckCommand()
        {
            int nPos;
            string strCmd = m_sbResponse.ToString();


            //如果接收远端用户名
            if ((nPos = strCmd.IndexOf("\x01\x02")) > -1)
            {
                ReceiveName(strCmd, nPos);

                //反馈公共终给端远端主机
                string strPubEPMsg = "\x03\x07" + m_remotePoint.ToString() +
                   "\x07\x03";
                SendData(strPubEPMsg, m_remotePoint);


                return;

            }


            //如果接收我的公共终端
            if ((nPos = strCmd.IndexOf("\x03\x07")) > -1)
            {
                ReceiveMyPublicEndPoint(strCmd, nPos);
                return;
            }


            //如果是打洞请求消息
            if ((nPos = strCmd.IndexOf("\x04\x07")) > -1)
            {
                ReceiveAndSendAck(strCmd, nPos);
                return;
            }


            //如果是打洞回应消息
            if ((nPos = strCmd.IndexOf("\x01\x07")) > -1)
            {
                m_bRecvAck = true;
                int nPos2 = strCmd.IndexOf("\x07\x01");
                if (nPos2 > -1)
                {
                    m_sbResponse.Remove(nPos, nPos2 - nPos + 2);
                }


                /*
                if (m_requestPublicEndPoint != null)
                {
                    if (!m_bHasConnected)
                    {
                        m_bHasConnected = true;
                        UDPSockEventArgs args = new UDPSockEventArgs("");
                        args.RemoteEndPoint = m_requestPublicEndPoint;
                        if (OnNewConnectU != null)
                        {
                            OnNewConnectU(this, args);
                        }
                        m_requestPublicEndPoint = null;
                    }
                }*/


                return;

            }


            //一般聊天消息
            m_sbResponse.Remove(0, strCmd.Length);
            RaiseMessageEvent(strCmd);
        }





        /// <summary>
        /// 接收远端用户名
        /// </summary>
        /// <param name="strCmd">包含用户名的控制信息</param>
        /// <param name="nPos"></param>
        private void ReceiveName(string strCmd, int nPos)
        {
            int nPos2 = strCmd.IndexOf("\x02\x01");
            if (nPos2 == -1)
            {
                return;
            }
            m_sbResponse.Remove(nPos, nPos2 - nPos + 2);


            string strUserName = strCmd.Substring(nPos + 2, nPos2 -
                 nPos - 2);

            UDPSockEventArgs e = new UDPSockEventArgs("");
            e.RemoteUserName = strUserName;
            e.RemoteEndPoint = m_remotePoint;


            //触发用户登录事件
            if (OnUserLogInU != null)
            {
                OnUserLogInU(this, e);
            }
        }





        /// <summary>
        /// 接收打洞请求的消息并发送回应
        /// </summary>
        /// <param name="strCmd"></param>
        /// <param name="nPos"></param>
        private void ReceiveAndSendAck(string strCmd, int nPos)
        {
            int nPos2 = strCmd.IndexOf("\x07\x04");
            if (nPos2 == -1)
            {
                return;
            }
            m_sbResponse.Remove(nPos, nPos2 - nPos + 2);


            string strBurrowMsg = strCmd.Substring(nPos + 2, nPos2 -
                  nPos - 2);




            string[] strSrcPoint = strBurrowMsg.Split(' ');




            //分析控制字符串包含的节点信息
            string[] strPrEndPoint = strSrcPoint[0].Split(':');
            string[] strPubEndPoint = strSrcPoint[1].Split(':');
            m_requestPrivateEndPoint =
                new IPEndPoint(IPAddress.Parse(strPrEndPoint[0]),
                int.Parse(strPrEndPoint[1]));
            m_requestPublicEndPoint =
                new IPEndPoint(IPAddress.Parse(strPubEndPoint[0]),
                int.Parse(strPubEndPoint[1]));


            //向请求打洞终端的方向打洞
            StartBurrowTo(m_requestPublicEndPoint, m_requestPrivateEndPoint);
        }





        /// <summary>
        /// 接收我的公共终端
        /// </summary>
        /// <param name="strCmd">包含公共终端的控制信息</param>
        /// <param name="nPos">控制字符串的起始位置</param>
        private void ReceiveMyPublicEndPoint(string strCmd, int nPos)
        {
            int nPos2 = strCmd.IndexOf("\x07\x03");
            if (nPos2 == -1)
            {
                return;
            }
            m_sbResponse.Remove(nPos, nPos2 - nPos + 2);


            m_strMyPublicEndPoint = strCmd.Substring(nPos + 2,
                 nPos2 - nPos - 2);

        }





        /// <summary>
        /// 触发一般UDP消息事件
        /// </summary>
        /// <param name="strMsg">消息内容</param>
        private void RaiseMessageEvent(string strMsg)
        {
            UDPSockEventArgs args = new UDPSockEventArgs("");
            args.SockMessage = strMsg;
            args.RemoteEndPoint = m_remotePoint;
            if (OnSockMessageU != null)
            {
                OnSockMessageU(this, args);
            }
        }





        /// <summary>
        /// 获取当前进程作为客户端的公共终端
        /// </summary>
        public string MyPublicEndPoint
        {
            get
            {
                return m_strMyPublicEndPoint;
            }
        }





        /// <summary>
        /// 获取当前进程作为客户端的私有终端
        /// </summary>
        public string MyPrivateEndPoint
        {
            get
            {
                return m_strMyPrivateEndPoint;
            }
        }
    }

}
