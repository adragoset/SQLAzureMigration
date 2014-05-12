using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public class AsyncQueueBCPJobArgs : EventArgs
    {
        public BCPJobInfo JobInfo { get; set; }
    }
}
