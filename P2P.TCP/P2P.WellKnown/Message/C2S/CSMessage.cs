using System;
using System.Collections.Generic;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// 客户端发送到服务器的消息基类
    /// </summary>
    [Serializable]
    public abstract class CSMessage : P2P.WellKnown.MessageBase
    {

        private string userName;

        protected CSMessage(string anUserName)
        {

            userName = anUserName;

        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {

            get { return userName; }

        }

    }


}
