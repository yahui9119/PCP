using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCP.Model
{
    /// <summary>
    /// 统一结果表示
    /// </summary>
    [Serializable]
    public class Result
    {
       
        public bool result { get; set; }
    
        public string message { get; set; }

    }
}
