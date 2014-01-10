using System;
using System.Collections.Generic;
using System.Text;

namespace P2P.WellKnown
{

    /// <summary>
    /// 转发请求Purch Hole消息
    /// </summary>
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


}
