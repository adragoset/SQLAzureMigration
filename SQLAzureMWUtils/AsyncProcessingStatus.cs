using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public static class AsyncProcessingStatus
    {
        public static bool FinishedAddingJobs = false;
        public static bool FinishedProcessingJobs = false;
        public static bool CancelProcessing = false;
        public static long NumberOfCommands = 0;
        public static long NumberOfCommandsExecuted = 0;
    }
}
