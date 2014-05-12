using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SQLAzureMWUtils;
using System.Threading;
using System.Drawing;
using System.Globalization;
using System.Configuration;
using System.IO;

namespace SQLAzureMWBatchUpload
{
    partial class Program
    {
        public static Thread[] _ta;
        public static BCPCommandCtrl[] _bcpWorkingCommand;
        public static Thread _ThreadManager;
        public static List<BCPJobInfo> BCPJobs = new List<BCPJobInfo>();
        private static List<string> _OutputFiles = new List<string>();

        private void Initialize_BCPJobs()
        {
        }

        protected static void BCPAsyncUploadProcessDone()
        {
            foreach (string file in _OutputFiles)
            {
                if (File.Exists(file))
                {
                    File.AppendAllText(_OutputResultsFile, File.ReadAllText(file));
                }
            }
        }

        private static void AsyncQueueJobHandler(AsyncQueueBCPJobArgs args)
        {
            BCPJobs.Add(args.JobInfo);
            if (_ThreadManager == null)
            {
                int threadCount = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfBCPThreads"]);
                _ta = new Thread[threadCount];
                _bcpWorkingCommand = new BCPCommandCtrl[threadCount];

                ThreadStart ts = new System.Threading.ThreadStart(ThreadManager);
                _ThreadManager = new System.Threading.Thread(ts);
                _ThreadManager.CurrentCulture = CultureInfo.CurrentCulture;
                _ThreadManager.CurrentUICulture = CultureInfo.CurrentUICulture;
                _ThreadManager.Start();
            }
        }

        private static bool BCPJobsDone()
        {
            for (int index = 0; index < BCPJobs.Count(); index++)
            {
                if (BCPJobs[index].JobStatus == CommandStatus.InProcess || BCPJobs[index].JobStatus == CommandStatus.Waiting)
                {
                    return false;
                }
            }
            return true;
        }

        private static BCPJobInfo GetNextWaitingJob()
        {
            for (int index = 0; index < BCPJobs.Count(); index++)
            {
                if (BCPJobs[index].JobStatus == CommandStatus.Waiting)
                {
                    string outputFile = _OutputResultsDirectory + BCPJobs[index].Schema + "." + BCPJobs[index].TableName + ".tmp";
                    _OutputFiles.Add(outputFile);
                    BCPJobs[index].JobStatus = CommandStatus.InProcess;
                    return BCPJobs[index];
                }
            }
            return null;
        }

        public static void ThreadManager()
        {
            AsyncTabPageEventArgs args = new AsyncTabPageEventArgs(0, "");
            while (true)
            {
                for (int index = 0; index < _ta.Count(); index++)
                {
                    args.CurrentTabIndex = index;

                    if (_ta[index] == null || _ta[index].ThreadState == ThreadState.Stopped)
                    {
                        BCPJobInfo jobInfo = GetNextWaitingJob();
                        if (jobInfo != null)
                        {
                            args.DisplayText = jobInfo.TableName;
                            args.CurrentTabIndex = index;
                            jobInfo.CurrentThreadIndex = index;

                            BCPCommandCtrl bcc = new BCPCommandCtrl();
                            bcc.UpdateBCPJobStatus = new AsyncBCPJobUpdateStatus(BCPAsyncUpdateResultsHandler);
                            bcc.CurrentJobInfo = jobInfo;
                            _bcpWorkingCommand[index] = bcc;

                            ThreadStart ts = new System.Threading.ThreadStart(delegate() { StartUpload(bcc); });
                            _ta[index] = new Thread(ts);
                            _ta[index].CurrentCulture = CultureInfo.CurrentCulture;
                            _ta[index].CurrentUICulture = CultureInfo.CurrentUICulture;
                            _ta[index].Start();
                        }
                    }
                }
                Thread.Sleep(500);    // Sleep for awhile and then look for more data to process
                if (AsyncProcessingStatus.FinishedAddingJobs && BCPJobsDone()) break;
            }

            BCPAsyncUploadProcessDone();
            AsyncProcessingStatus.FinishedProcessingJobs = true;
        }

        public static void StartUpload(object obj)
        {
            BCPCommandCtrl bcc = (BCPCommandCtrl)obj;
            bcc.ExecuteBCPCommand();
        }

        private static void BCPAsyncUpdateResultsHandler(AsyncBCPJobEventArgs args)
        {
            // Here is where we append to file

            string outputFile = _OutputResultsDirectory + _bcpWorkingCommand[args.CurrentThreadIndex].CurrentJobInfo.Schema + "." + _bcpWorkingCommand[args.CurrentThreadIndex].CurrentJobInfo.TableName + ".tmp";
            File.AppendAllText(outputFile, args.DisplayText);
            Console.Write(args.DisplayText);
        }
    }
}
