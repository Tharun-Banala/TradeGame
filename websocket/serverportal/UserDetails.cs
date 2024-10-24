using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serverportal
{
    public class UserDetails
    {
        public int userId { get; }
        public string userName { get; }
        public string passWord { get;  }
        public UserDetails(int userid, string username,string password)
        {
            userId = userid;
            userName = username;
            passWord = password;
        }
    }
}
