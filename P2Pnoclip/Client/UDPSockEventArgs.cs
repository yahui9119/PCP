using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Client
{
    /// <summary>
 
/// 用于承载UDPSock信息的事件类
/// </summary>
public class UDPSockEventArgs:EventArgs
{
     /// <summary>
     /// 要承载的消息
     /// </summary>
     private string m_strMsg;
     /// <summary>
     /// 用户信息
     /// </summary>
     private string m_strUserName;
           /// <summary>
           /// 触发该事件的公共终端
           /// </summary>
           private IPEndPoint m_EndPoint;
 
 
     /// <summary>
     /// 初始化UDPSock事件
     /// </summary>
     /// <param name="sMsg">用户发送的信息</param>
     public UDPSockEventArgs(string sMsg):base()
     {
      this.m_strMsg =sMsg;
     }
 
 
     /// <summary>
     /// 远端用户名
     /// </summary>
     public string RemoteUserName
     {
      get
      {
       return m_strUserName;
      }
      set
      {
       m_strUserName=value;
      }
     }
 
 
           /// <summary>
           /// 一般套接字消息
           /// </summary>
           public string SockMessage
           {
               get
               {
                   return m_strMsg;
               }
               set
               {
                   m_strMsg = value;
               }
           }
 
 
           /// <summary>
           /// 公共远端节点
           /// </summary>
           public IPEndPoint RemoteEndPoint
           {
               get
               {
                   return m_EndPoint;
               }
               set
               {
                   m_EndPoint = value;
               }
           }
     }
}
 

