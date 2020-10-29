using System;
using System.Collections.Generic;
using System.Text;

namespace NRTA.Core.Models
{
    public class User
    {
        public string Username { get; set; }
        public string RealName { get; set; }
        public string Password { get; set; }
        public string Department { get; set; }

        public override string ToString()
        {
            return string.Format("用户名: {0} 姓名: {1} 密码: {2} 部门: {3}", Username, RealName, Password, Department);
        }
    }
}
