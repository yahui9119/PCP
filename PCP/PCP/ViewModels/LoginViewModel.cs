using PCP.Core;
using PCP.Model;
using PCP.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PCP.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        /// <summary>
        /// 用户名
        /// </summary>
        string _username;
        public string username
        {
            get
            {
                return _username;
            }
            set
            {
                if (value == _username)
                {
                    return;
                }
                _username = username;
                base.RaisePropertyChanged("username");

            }
        }
        /// <summary>
        /// 用户名
        /// </summary>
        string _password;
        public string password
        {
            get
            {
                return _password;
            }
            set
            {
                if (value == _password)
                {
                    return;
                }
                _password = password;  
                base.RaisePropertyChanged("password");
            }
        }
        RelayCommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new RelayCommand(() =>
                {
                    User user = new User() { password = password, username = username };
                    ClientSocket client = new ClientSocket("");
                    client.StartServer(user);
                }));
            }
        }
    }
}
