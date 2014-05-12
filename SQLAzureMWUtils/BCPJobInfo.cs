using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Specialized;

namespace SQLAzureMWUtils
{
    public enum CommandStatus
    {
        Failed,
        InProcess,
        Skip,
        Success,
        Waiting
    }

    public class BCPJobInfo
    {
        public AsyncUpdateStatus UpdateStatus { get; set; }
        public CommandStatus JobStatus { get; set; }
        public FederationDetails FederationInfo { get; set; }
        public string TableName { get; set; }
        public string Schema { get; set; }
        public string BCPUploadCommand { get; set; }
        public string ConnectionString { get; set; }
        public Int64 NumberOfRows { get; set; }
        public int AssignedTabIndex { get; set; }
        public int CurrentThreadIndex { get; set; }
    }
}
