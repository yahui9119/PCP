using System;
using System.Collections.Generic;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// 请求Purch Hole消息
    /// </summary>
    public class TranslateMessage : CSMessage
    {

        protected string toUserName;

        public TranslateMessage(string userName, string toUserName) : base(userName)

        {

            this.toUserName = toUserName;

        }

        public string ToUserName

        {

            get { return this.toUserName; }

        }

    }


}
