using System;
using System.Collections.Generic;
using System.Text;

namespace SQLAzureMWUtils
{
    public class FederationTableInfo
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string FederatedColumn { get; set; }
        public string BCPArgs { get; set; }
        public string InputFileName { get; set; }
        public string HastableKey { get; set; }

        public override string ToString()
        {
            return "[" + Schema + "].[" + Table + "]";
        }
    }
}
