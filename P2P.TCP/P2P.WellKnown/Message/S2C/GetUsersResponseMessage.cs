using System;
using System.Collections.Generic;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// 请求用户列表应答消息
    /// </summary>
    public class GetUsersResponseMessage : SCMessage
    {

        private P2P.WellKnown.UserCollection userList;

        public GetUsersResponseMessage(P2P.WellKnown.UserCollection users)
        {

            this.userList = users;

        }

        public P2P.WellKnown.UserCollection UserList
        {

            get { return userList; }

        }

    }// end class


}
