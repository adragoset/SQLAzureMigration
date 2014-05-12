using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Globalization;
using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Management.Common;
using System.Xml.Serialization;

namespace SQLAzureMWUtils
{
    /// <summary>
    /// General functions.
    /// </summary>
    /// <devdoc>
    /// This class is used for general functions that are used within SQLAzureMW
    /// </devdoc>
    /// <author name="George Huey" />
    /// <history>
    ///     <change date="9/9/2009" user="George Huey">
    ///         Cleaned up comments, added headers, etc.
    ///     </change>
    /// </history>
    public delegate void AsyncUpdateStatus(AsyncNotificationEventArgs e);
    public delegate void AsyncBCPJobUpdateStatus(AsyncBCPJobEventArgs e);
    public delegate void AsyncQueueBCPJob(AsyncQueueBCPJobArgs e);
    public delegate void AsyncUpdateTab(AsyncTabPageEventArgs e);

    public static class CommonFunc
    {
        public static string SerializeToXmlString(object obj)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            XmlSerializer ser = new XmlSerializer(obj.GetType());

            ser.Serialize(ms, obj);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            System.IO.StreamReader tr = new System.IO.StreamReader(ms);

            return tr.ReadToEnd();
        }

        public static object DeserializeXmlString(string xmlString, Type toTypeOf)
        {
            XmlSerializer mySerializer = new XmlSerializer(toTypeOf);
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);

            sw.Write(xmlString);
            sw.Flush();
            ms.Position = 0;
            return mySerializer.Deserialize(ms);
        }

        public static string ConverByteArrayToString(SqlBinary sqlValue)
        {
            string value = "0x" + BitConverter.ToString((byte[])sqlValue).Replace("-", "");
            return value;
        }

        public static string GetLowestException(Exception ex)
        {
            Exception innerExcept = ex;
            while (innerExcept.InnerException != null)
            {
                innerExcept = innerExcept.InnerException;
            }
            return innerExcept.Message;
        }

        public static string FormatString(string str, params object[] args)
        {
            return string.Format(System.Globalization.CultureInfo.CurrentUICulture, str, args);
        }

        public static string GetTextFromFile(string fileToProcess)
        {
            StreamReader srFileToProcess = new StreamReader(fileToProcess, true);
            string txt = srFileToProcess.ReadToEnd();
            srFileToProcess.Close();
            return txt;
        }

        public static Boolean GetAppSettingsBoolValue(string settingName)
        {
            string settingValue = ConfigurationManager.AppSettings[settingName];
            if (settingValue != null)
            {
                if (settingValue.Equals(Properties.Resources.True, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }

                if (settingValue.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetAppSettingsStringValue(string settingName)
        {
            string settingValue = ConfigurationManager.AppSettings[settingName];
            if (settingValue != null)
            {
                return settingValue;
            }
            return "";
        }

        public static string GetConnectionString(Server targetServer, string targetDatabase)
        {
            return GetConnectionString(targetServer.ConnectionContext.ServerInstance, targetServer.ConnectionContext.LoginSecure, targetDatabase, targetServer.ConnectionContext.Login, targetServer.ConnectionContext.Password);
        }

        public static string GetConnectionString(string ServerInstance, bool LoginSecure, string TargetDatabase, string Login, string Password)
        {
            string connectionStr;

            if (LoginSecure)
            {
                connectionStr = FormatString(ConfigurationManager.AppSettings["ConnectionStringTrusted"], ServerInstance, TargetDatabase);
            }
            else
            {
                connectionStr = FormatString(ConfigurationManager.AppSettings["ConnectionString"], ServerInstance, TargetDatabase, Login, "'" + Password.Replace("'", "''") + "'");
            }
            return connectionStr;
        }

        public static string GetBcpOutputFileName(string directory, string tableName, int cnt, string ext)
        {
            string file = "";
            string slash = directory.EndsWith("\\", StringComparison.Ordinal) ? "" : "\\";

            tableName = tableName.Replace("/", "_SLASH_");

            if (cnt < 1)
            {
                file = directory + slash + tableName + "." + ext;
            }
            else
            {
                file = directory + slash + tableName + "_" + cnt.ToString(CultureInfo.InvariantCulture) + "." + ext;
            }

            if (!File.Exists(file))
            {
                return file;
            }

            if (CommonFunc.GetAppSettingsBoolValue("DelOldBCPFiles"))
            {
                File.Delete(file);
                return file;
            }

            return GetBcpOutputFileName(directory, tableName, ++cnt, ext);
        }

        public static Server GetSmoServer(ServerConnection connection)
        {
            Server dbServer = new Server(connection);

            dbServer.SetDefaultInitFields(typeof(Database), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(DatabaseDdlTrigger), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(Schema), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(DatabaseRole), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(StoredProcedure), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(Table), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedDataType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedFunction), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedTableType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.View), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(XmlSchemaCollection), new String[] { "Name" });

            return dbServer;
        }

        public static Server GetSmoServer(string name)
        {
            Server dbServer = new Server(name);

            dbServer.SetDefaultInitFields(typeof(Database), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(DatabaseDdlTrigger), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(Schema), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(DatabaseRole), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(StoredProcedure), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(Table), new String[] { "Name", "IsSystemObject" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedDataType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedFunction), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(UserDefinedTableType), new String[] { "Name" });
            dbServer.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.View), new String[] { "Name", "IsSystemObject", "IsEncrypted" });
            dbServer.SetDefaultInitFields(typeof(XmlSchemaCollection), new String[] { "Name" });

            return dbServer;
        }

        public static ServerTypes GetEnumServerType(string serverType)
        {
            switch (serverType.ToLower())
            {
                case "servertype_sqlazure":
                    return ServerTypes.SQLAzure;

                case "servertype_sqlazurefed":
                    return ServerTypes.SQLAzureFed;

                default:
                    return ServerTypes.SQLServer;
            }
        }

        public static string[] GetSchemaAndTableFromBCPArgs(string bcpArgs)
        {
            string[] arr = {"", ""};

            int start = bcpArgs.IndexOf("[", 0) + 1;
            if (start > 0)
            {
                int end = bcpArgs.IndexOf("].", start + 1);

                if (end > start)
                {
                    arr[0] = bcpArgs.Substring(start, end - start);

                    start = bcpArgs.IndexOf(".[", end) + 2;
                    if (start > 2)
                    {
                        end = bcpArgs.IndexOf("] ", start + 1);
                        if (end > start)
                        {
                            arr[1] = bcpArgs.Substring(start, end - start);
                            return arr;
                        }
                    }
                }
            }
            throw new Exception(Properties.Resources.ErrorParsingBCPArgs + " { " + bcpArgs + " }");
        }

        public static string BuildBCPUploadCommand(TargetServerInfo targetServer, string bcpArgs)
        {
            // bcpArgs example: ":128425:[dbo].[Categories] in "C:\Users\ghuey\AppData\Local\Temp\tmp1CD8.tmp" -n -E

            string[] schema_table = GetSchemaAndTableFromBCPArgs(bcpArgs);
            string schema = schema_table[0];
            string table = schema_table[1];

            StringBuilder args = new StringBuilder(200);

            if (bcpArgs.EndsWith("\r", StringComparison.Ordinal))
            {
                bcpArgs = bcpArgs.Remove(bcpArgs.Length - 1);
            }
            string filteredCommands = bcpArgs.Substring(bcpArgs.IndexOf(" in ", StringComparison.Ordinal));

            /*
            ** Note that BCP currently does not handle databases, schemas, and tables with "." in them very well.  Thus,
            ** this next section of code.  If there are no "." in any of these, then we can just "" the string and put the -q (quoted) flag
            ** on the BCP command and BCP will be happy.  If there is just a period in the database name, then we can split
            ** out the database name (using the -d argument) and then "" everything out with the -q flag.  If there is a "."
            ** in more than one of the three, then we will have to [ ] the string out and hope for the best (without the -q).
            **
            ** The problem is that if you [ ] the string out, then you can't use the -q flag and if your table has computed column
            ** with an index, then BCP requires the -q flag (which you can't do with [ ].  Anyway, it is kind of a catch 22 problem.
            */

            if (targetServer.TargetDatabase.IndexOf('.') < 1 && schema.IndexOf('.') < 1 && table.IndexOf('.') < 1)
            {
                args.Append("\"" + targetServer.TargetDatabase + "." + schema + "." + table + "\"" + filteredCommands + " -q -S " + targetServer.ServerInstance);
            }
            else if (targetServer.TargetDatabase.IndexOf('.') > 0 && schema.IndexOf('.') < 1 && table.IndexOf('.') < 1)
            {
                args.Append("\"" + schema + "." + table + "\"" + filteredCommands + " -d \"" + targetServer.TargetDatabase + "\" -q -S " + targetServer.ServerInstance);
            }
            else
            {
                args.Append("[" + targetServer.TargetDatabase + "].[" + schema + "].[" + table + "]" + filteredCommands + " -S " + targetServer.ServerInstance);
            }

            if (targetServer.LoginSecure)
            {
                args.Append(" -T");
            }
            else
            {
                string login = targetServer.Login[0] == '"' ? targetServer.Login : "\"" + targetServer.Login + "\"";
                string password = targetServer.Password[0] == '"' ? targetServer.Password : "\"" + targetServer.Password + "\"";
                args.Append(" -U " + login + " -P " + password);
            }
            return args.ToString();
        }
    }

    public class TemporaryFileStream : FileStream
    {
        private string _fileName = string.Empty;
        private bool _deleteFile = true;

        public TemporaryFileStream()
            : this(Path.GetTempFileName())
        {
        }

        public bool DeleteFile
        {
            get { return _deleteFile; }
            set { _deleteFile = value; }
        }

        public TemporaryFileStream(string fileName)
            : base(fileName, FileMode.OpenOrCreate)
        {
            _deleteFile = false;
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_deleteFile && FileName.Length > 1)
            {
                File.Delete(FileName);
            }
        }
    }
}
