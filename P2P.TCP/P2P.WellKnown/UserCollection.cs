using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P2P.WellKnown
{
    /// <summary>
    /// 网络用户列表
    /// </summary>
    [Serializable]
    public class UserCollection:CollectionBase
    {
        public void Add(User user)
        {
            InnerList.Add(user);
        }
        public void Remove(User user)
        {
            InnerList.Remove(user);
        }
        public User this[int index]
        {
            get { return (User)InnerList[index]; }
        }
        public User Find(string username)
        {
            foreach (User item in this)
            {
                if (string.Compare(username, item.UserName, true) == 0)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
