using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCP.Model
{
    /// <summary>
    /// 用户 
    /// </summary>
    [Serializable]
    public class User
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
