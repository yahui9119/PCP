using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Client
{
    /// <summary>
      /// 保存打洞消息要发向的节点信息
      /// </summary>
      class ToEndPoint
      {
          /// <summary>
          /// 私有节点
          /// </summary>
          public IPEndPoint m_privateEndPoint;
 
 
          /// <summary>
          /// 公共节点
          /// </summary>
          public IPEndPoint m_publicEndPoint;
      } 
}
