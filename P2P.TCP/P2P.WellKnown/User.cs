using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace P2P.WellKnown
{
    /// <summary>
    /// 网络端点用户
    /// </summary>
    [Serializable]
    public class User
    {
        protected string userName;
        protected IPEndPoint netPoint;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="NetPoint"></param>
        public User(string UserName, IPEndPoint NetPoint)
        {
            this.userName = UserName;
            this.netPoint = NetPoint;
        }
        /// <summary>
        /// 获取用户名
        /// </summary>
        public string UserName { get { return userName;} }
        public IPEndPoint NetPoint
        {
            get { return netPoint; }
            set { netPoint = value; }
        }
    }
}
