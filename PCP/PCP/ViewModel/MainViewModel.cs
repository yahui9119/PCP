using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCP.ViewModel
{
    public class MainViewModel:PropertyChangedBase
    {
        private string _message;
        public string message {
            get { return _message; }
            set{
                _message = value;
                this.NotifyPropertyChanged(m => m.message);
            }
        }
    }
}
