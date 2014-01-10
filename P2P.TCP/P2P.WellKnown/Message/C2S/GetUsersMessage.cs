using System;
using System.Collections.Generic;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// 请求用户列表消息
    /// </summary>
    public class GetUsersMessage : CSMessage
    {

        public GetUsersMessage(string userName)
            : base(userName)

        { }

    }


}
