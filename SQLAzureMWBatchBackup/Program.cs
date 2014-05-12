using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLAzureMWUtils;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Globalization;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Reflection;
using SQLAzureMWBatchBackup.SQLObjectFilter;

namespace SQLAzureMWBatchBackup
{
    class Program
    {
        private static StringBuilder _outputDir;
        private static string _SourceDatabase = "";
        private static string _OutputResultsFile = "";
        private static string _SourceServerName = "";
        private static string _SourceUserName = "";
        private static string _SourcePassword = "";
        private static bool _bSourceConnectNTAuth;
        private static bool _bAddDateTimeFolder;
        private static ObjectSelector _ObjectSelector;

        static int Main(string[] args)
        {
            try
            {
                Assembly assem = Assembly.GetEntryAssembly();
                AssemblyName assemName = assem.GetName();
                Console.WriteLine(CommonFunc.FormatString(Properties.Resources.ProgramVersion, assemName.Name, assemName.Version.ToString()));

                string message = "";
                if (!DependencyHelper.CheckDependencies(ref message))
                {
                    Console.WriteLine(message);
                    return -1;
                }

                try
                {
                    string txt = CommonFunc.GetTextFromFile(CommonFunc.GetAppSettingsStringValue("ObjectSelector"));
                    _ObjectSelector = (ObjectSelector)CommonFunc.DeserializeXmlString(txt, typeof(ObjectSelector));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _ObjectSelector = GenerateDefaultObjectSelector();
                }
                finally
                {
                    if (_ObjectSelector == null)
                    {
                        _ObjectSelector = GenerateDefaultObjectSelector();
                    }
                }

                string argValues = Environment.NewLine + Properties.Resources.ProgramArgs + Environment.NewLine;

                _outputDir = new StringBuilder(CommonFunc.GetAppSettingsStringValue("OutputDirectory"));
                _SourceServerName = ConfigurationManager.AppSettings["SourceServerName"];
                _SourceUserName = ConfigurationManager.AppSettings["SourceUserName"];
                _SourcePassword = ConfigurationManager.AppSettings["SourcePassword"];
                _SourceDatabase = ConfigurationManager.AppSettings["SourceDatabase"];
                _bSourceConnectNTAuth = CommonFunc.GetAppSettingsBoolValue("SourceConnectNTAuth");
                _OutputResultsFile = ConfigurationManager.AppSettings["OutputResultsFile"];
                _bAddDateTimeFolder = CommonFunc.GetAppSettingsBoolValue("AppendDateTimeFolder");

                for (int index = 0; index < args.Length; index++)
                {
                    switch (args[index])
                    {
                        case "-a":
                        case "/a":
                            _bAddDateTimeFolder = args[++index].Equals(Properties.Resources.True, StringComparison.CurrentCultureIgnoreCase);
                            break;

                        case "-S":
                        case "/S":
                            _SourceServerName = args[++index];
                            break;

                        case "-U":
                        case "/U":
                            _SourceUserName = args[++index];
                            break;

                        case "-P":
                        case "/P":
                            _SourcePassword = args[++index];
                            break;

                        case "-D":
                        case "/D":
                            _SourceDatabase = args[++index];
                            break;

                        case "-o":
                        case "/o":
                            _OutputResultsFile = args[++index];
                            break;

                        case "-O":
                        case "/O":
                            _outputDir.Remove(0, _outputDir.Length);
                            _outputDir.Append(args[++index]);
                             break;

                        case "-T":
                        case "/T":
                            _bSourceConnectNTAuth = true;
                            break;

                        default:
                            Console.WriteLine(argValues);
                            return -1;
                    }
                }
                int retVal = Process();
                return retVal;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        private static ObjectSelector GenerateDefaultObjectSelector()
        {
            ObjectSelector gos = new ObjectSelector();
            gos.Roles.Script = true;
            gos.Views.Script = true;
            gos.UserDefinedFunctions.Script = true;
            gos.UserDefinedDataTypes.Script = true;
            gos.UserDefinedTableTypes.Script = true;
            gos.StoredProcedures.Script = true;
            gos.Triggers.Script = true;
            gos.Schemas.Script = true;
            gos.SchemaCollections.Script = false;
            gos.Tables.Script = true;

            //SQLObject obj = new SQLObject();
            //obj.Script = true;
            //obj.RegexExpression = true;
            //obj.Schema = ".*";
            //obj.Name = ".*";
            //gos.Tables.SQLObjects.Add(obj);
            //string ttt = CommonFunc.SerializeToXmlString(gos);
            //System.Diagnostics.Debug.WriteLine(ttt);
            return gos;
        }

        protected static void SourceAsyncUpdateStatusHandler(AsyncNotificationEventArgs args)
        {
            if (_OutputResultsFile != null && _OutputResultsFile.Length > 0)
            {
                File.AppendAllText(_OutputResultsFile, args.DisplayText);
            }
            Console.Write(args.DisplayText);
        }

        public static int Process()
        {
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            StreamWriter swTSQL = null;

            if (_bAddDateTimeFolder)
            {
                if (!_outputDir.ToString().EndsWith("\\"))
                {
                    _outputDir.Append("\\");
                }
                _outputDir.Append(DateTime.Now.ToString("dd-MMMM-yyyy HHmm"));
            }

            DirectoryInfo di = Directory.CreateDirectory(_outputDir.ToString());
            if (di.Exists)
            {
                string file = CommonFunc.FormatString("{0}\\{1}.sql", di.FullName, _SourceDatabase);
                swTSQL = new StreamWriter(File.Create(file));
            }
            else
            {
                Console.WriteLine(CommonFunc.FormatString(Properties.Resources.BCPOutputDirectoryNotFound, ConfigurationManager.AppSettings["BCPFileDir"]));
                return -1;
            }

            if (_outputDir.ToString().EndsWith("\\"))
            {
                _OutputResultsFile = _outputDir.ToString() + _OutputResultsFile;
            }
            else
            {
                _OutputResultsFile = _outputDir.ToString() + "\\" + _OutputResultsFile;
            }

            try
            {
                File.AppendAllText(_OutputResultsFile, CommonFunc.FormatString(Properties.Resources.ProgramVersion, assemName.Name, assemName.Version.ToString()) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(CommonFunc.FormatString(Properties.Resources.ErrorOpeningFile, _OutputResultsFile, ex.Message));
                return -1;
            }

            // Fill in necessary information
            ServerConnection conn = new ServerConnection();
            conn.ServerInstance = _SourceServerName;
            //conn.DatabaseName = _SourceDatabase;

            // Setup capture and execute to be able to display script
            conn.SqlExecutionModes = SqlExecutionModes.ExecuteSql;

            // Set connection timeout
            conn.ConnectTimeout = 30;
            if (_bSourceConnectNTAuth == true)
            {
                // Use Windows authentication
                conn.LoginSecure = true;
            }
            else
            {
                // Use SQL Server authentication
                conn.LoginSecure = false;
                conn.Login = _SourceUserName;
                conn.Password = _SourcePassword;
            }

            // Go ahead and connect
            conn.Connect();
            Server svr = CommonFunc.GetSmoServer(conn);
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(SourceAsyncUpdateStatusHandler);
            SourceServer tp = new SourceServer();

            foreach (Database sdb in svr.Databases)
            {
                if (sdb.Name.Equals(_SourceDatabase, StringComparison.CurrentCultureIgnoreCase))
                {
                    tp.GenerateScriptFromSourceServer(svr, sdb, swTSQL, updateStatus, _outputDir.ToString(), _ObjectSelector);
                    break;
                }
            }
            return 0;
        }
    }
}
