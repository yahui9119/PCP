using System;
using System.Collections.Generic;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// 用户登出消息
    /// </summary>
       [Serializable]
    public class LogoutMessage : CSMessage
    {

        public LogoutMessage(string userName)
            : base(userName)
        { }

    }// end class


}
