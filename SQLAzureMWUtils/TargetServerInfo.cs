using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public class TargetServerInfo
    {
        public ServerTypes ServerType { get; set; }
        public string ServerInstance { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string RootDatabase { get; set; }
        public string TargetDatabase { get; set; }
        public bool LoginSecure { get; set; }

        public TargetServerInfo()
        {
            ServerInstance = "";
            Login = "";
            Password = "";
            RootDatabase = "";
            TargetDatabase = "";
            LoginSecure = false;
        }

        public string ConnectionStringRootDatabase
        {
            get
            {
                return CommonFunc.GetConnectionString(ServerInstance, LoginSecure, RootDatabase, Login, Password);
            }
        }

        public string ConnectionStringTargetDatabase
        {
            get
            {
                return CommonFunc.GetConnectionString(ServerInstance, LoginSecure, TargetDatabase, Login, Password);
            }
        }
    }
}
