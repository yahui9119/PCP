using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PCP.ViewModel
{
    class DelegateCommand : ICommand
    {
        //ICommand 两个方法 CanExecute 和Execute，一个事件CanExecuteChanged

        public bool CanExecute(object parameter)
        {
            //throw new NotImplementedException();
            if (this.CanExecuteFunc == null)
            {
                return true;
            }
            return this.CanExecuteFunc(parameter);
        }

        public event EventHandler CanExecuteChanged;

        //Execute
        public void Execute(object parameter)
        {
            // throw new NotImplementedException();
            if (this.ExecuteAction == null)
            {
                return;
            }
            this.ExecuteAction(parameter);
        }

        public Action<object> ExecuteAction { get; set; }
        public Func<object, bool> CanExecuteFunc { get; set; }
    }
}
