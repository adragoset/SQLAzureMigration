using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using SQLAzureMWUtils;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;

namespace SQLAzureMWBatchUpload
{
    partial class Program
    {
        private static string _FileToProcess = "";
        private static string _TargetDatabase = "";
        private static string _Collation = "";
        private static string _OutputResultsDirectory = "";
        private static string _OutputResultsFile = "";
        private static string _TargetServerName = "";
        private static string _TargetUserName = "";
        private static string _TargetPassword = "";
        private static string _TargetEdition = "";
        private static bool _bTargetConnectNTAuth;
        private static bool _bDropExistingDatabase;
        private static int _TargetDatabaseSize;

        static void Main(string[] args)
        {
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.ProgramVersion, assemName.Name, assemName.Version.ToString()));

            string message = "";
            if (!DependencyHelper.CheckDependencies(ref message))
            {
                Console.WriteLine(message);
                return;
            }

            string argValues = Environment.NewLine + Properties.Resources.ProgramArgs + Environment.NewLine;

            _TargetServerName = CommonFunc.GetAppSettingsStringValue("TargetServerName");
            _TargetUserName = CommonFunc.GetAppSettingsStringValue("TargetUserName");
            _Collation = CommonFunc.GetAppSettingsStringValue("DBCollation");
            _TargetPassword = CommonFunc.GetAppSettingsStringValue("TargetPassword");
            _FileToProcess = CommonFunc.GetAppSettingsStringValue("FileToProcess");
            _TargetDatabase = CommonFunc.GetAppSettingsStringValue("TargetDatabase");
            _TargetDatabaseSize = Convert.ToInt32(ConfigurationManager.AppSettings["TargetDatabaseSize"], CultureInfo.InvariantCulture);
            _TargetEdition = CommonFunc.GetAppSettingsStringValue("TargetDatabaseEdition");
            _OutputResultsFile = CommonFunc.GetAppSettingsStringValue("OutputResultsFile");
            _bTargetConnectNTAuth = CommonFunc.GetAppSettingsBoolValue("TargetConnectNTAuth");
            _bDropExistingDatabase = CommonFunc.GetAppSettingsBoolValue("DropOldDatabaseIfExists");

            for (int index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "-S":
                    case "/S":
                        _TargetServerName = args[++index];
                        break;

                    case "-U":
                    case "/U":
                        _TargetUserName = args[++index];
                        break;

                    case "-P":
                    case "/P":
                        _TargetPassword = args[++index];
                        break;

                    case "-D":
                    case "/D":
                        _TargetDatabase = args[++index];
                        break;

                    case "-i":
                    case "/i":
                        _FileToProcess = args[++index];
                        break;

                    case "-c":
                    case "/c":
                        _Collation = args[++index];
                        break;

                    case "-o":
                    case "/o":
                        _OutputResultsFile = args[++index];
                        try
                        {
                            File.AppendAllText(_OutputResultsFile, "SQLAzureMWBatchUpload");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.ErrorOpeningFile, _OutputResultsFile, ex.Message));
                            return;
                        }
                        break;

                    case "-e":
                    case "/e":
                        _TargetEdition = args[++index].ToLower(CultureInfo.InvariantCulture);
                        if (!_TargetEdition.Equals("web") && !_TargetEdition.Equals("business"))
                        {
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidDatabaseEdition, _TargetEdition));
                            return;
                        }
                        break;

                    case "-s":
                    case "/s":
                        try
                        {
                            int size = Convert.ToInt32(args[++index], CultureInfo.InvariantCulture);
                            if (size != 1 && size != 5 && size != 10 && size != 20 && size != 30 && size != 40 && size != 50 && size != 100 && size != 150)
                            {
                                Console.WriteLine(Properties.Resources.InvalidDatabaseSize);
                                return;
                            }
                            _TargetDatabaseSize = size;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            Console.WriteLine(CommonFunc.FormatString(Properties.Resources.InvalidDatabaseEdition, args[index]));
                            return;
                        }
                        break;

                    case "-T":
                    case "/T":
                        _bTargetConnectNTAuth = true;
                        break;

                    case "-d":
                    case "/d":
                        _bDropExistingDatabase = args[++index].Equals(Properties.Resources.True, StringComparison.CurrentCultureIgnoreCase);
                        break;

                    default:
                        Console.WriteLine(argValues);
                        return;
                }
            }
            Process();
        }

        protected static void TargetAsyncUpdateStatusHandler(AsyncNotificationEventArgs e)
        {
            int retry = 5;

            if (_OutputResultsFile.Length > 0)
            {
                while (true)
                {
                    try
                    {
                        File.AppendAllText(_OutputResultsFile, e.DisplayText);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (retry-- > 0)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        Console.WriteLine(ex.Message);
                        break;
                    }
                }
            }
            Console.Write(e.DisplayText);
        }

        public static void Process()
        {
            TargetProcessor tp = new TargetProcessor();
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(TargetAsyncUpdateStatusHandler);
            string sqlToExecute = CommonFunc.GetTextFromFile(_FileToProcess);

            _OutputResultsDirectory = _OutputResultsFile.Substring(0, _OutputResultsFile.LastIndexOf('\\') + 1);

            TargetServerInfo tsi = new TargetServerInfo();
            tsi.ServerInstance = _TargetServerName;
            tsi.TargetDatabase = _TargetDatabase;

            if (Regex.IsMatch(_TargetServerName, CommonFunc.GetAppSettingsStringValue("RegexSearchForAzureServer"), RegexOptions.IgnoreCase))
            {
                tsi.ServerType = ServerTypes.SQLAzure;
            }
            else
            {
                tsi.ServerType = ServerTypes.SQLServer;
            }

            if (_bTargetConnectNTAuth == true)
            {
                // Use Windows authentication
                tsi.LoginSecure = true;
            }
            else
            {
                // Use SQL Server authentication
                tsi.LoginSecure = false;
                tsi.Login = _TargetUserName;
                tsi.Password = _TargetPassword;
            }

            //AsyncProcessingStatus.FinishedProcessingJobs = true;
            AsyncQueueBCPJob queueBCPJob = new AsyncQueueBCPJob(AsyncQueueJobHandler);

            tp.CreateDatabase(tsi, _Collation, _TargetEdition, _TargetDatabaseSize, _bDropExistingDatabase);
            tp.ExecuteSQLonTarget(tsi, updateStatus, queueBCPJob, sqlToExecute);
        }
    }
}
