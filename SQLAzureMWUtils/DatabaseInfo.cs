using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace SQLAzureMWUtils
{
    public enum TypeOfConnection
    {
          SQLAzureFederation = 0
        , UsingSMO = 1
        , UsingADO = 2
    }

    public class DatabaseInfo
    {
        public TypeOfConnection ConnectedTo = TypeOfConnection.UsingSMO;
        public FederationMemberDistribution FederationMember { get; set; }
        public Database DatabaseObject { get; set; }
        public string DatabaseName { get; set; }
        public bool IsDbOwner { get; set; }

        public override string ToString()
        {
            if (ConnectedTo == TypeOfConnection.SQLAzureFederation)
            {
                return FederationMember.ToString();
            }
            else if (DatabaseObject == null)
            {
                return "[" + DatabaseName + "]";
            }
            return DatabaseObject.ToString();
        }
    }
}
