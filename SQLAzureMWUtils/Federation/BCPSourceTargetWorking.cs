using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public class BCPSourceTargetWorking
    {
        public FederationTableInfo TableInformation { get; set; }
        public string TargetDatabase { get; set; }
        public string BCPCommand { get; set; }
        public string OutputFileName { get; set; }
        public long NumberOfRows { get; set; }

        public override string ToString()
        {
            return BCPCommand;
        }
    }
}
