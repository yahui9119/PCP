using System;
using System.Collections.Generic;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// 测试消息
    /// </summary>
    public class WorkMessage : PPMessage
    {

        private string message;

        public WorkMessage(string msg)
        {
            message = msg;
        }

        public string Message
        {

            get { return message; }

        }

    }


}
