using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections.Specialized;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Common;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;

namespace SQLAzureMWUtils
{
    /// <summary>
    /// This is the script that is used to script the local SQL Server database.
    /// </summary>
    /// <devdoc>
    /// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    /// PARTICULAR PURPOSE.
    /// </devdoc>
    /// <author name="George Huey" />
    /// <history>
    ///     <change date="9/9/2009" user="George Huey">
    ///         Added headers, etc.
    ///     </change>
    /// </history>

    public class ScriptDatabase
    {
        private AsyncNotificationEventArgs _eventArgs = new AsyncNotificationEventArgs(NotificationEventFunctionCode.AnalysisOutput, 0, "", "" + Environment.NewLine, Color.DarkBlue);
        private TSQLNotSupportedByAzure _nsByAzure = null;
        private SMOScriptOptions _options;
        private StreamWriter _swTSQL;
        private Database _sourceDB;
        private Scripter _scrptr = null;
        private Server _SqlServerSelection = null;
        private string _ServerInstance;
        private string _Login;
        private string _Password;
        private bool _LoginSecure;
        private AsyncUpdateStatus _updateStatus = null;
        private static StringBuilder _bcpOutput = new StringBuilder(3000);
        private static CommentAreaHelper _CommentAreaHelper = new CommentAreaHelper();
        private bool _bCheckCompatibilityOnly = false;

        FederationDetails _federationDetails = null;
        TargetServerInfo _serverInfo = null;
        FederationMemberDistribution _memberDistribution = null;

        public ScriptDatabase()
        {
        }

        public ScriptDatabase(FederationDetails fed, TargetServerInfo serverInfo, FederationMemberDistribution member)
        {
            _federationDetails = fed;
            _serverInfo = serverInfo;
            _memberDistribution = member;
        }

        public void Initialize(AsyncUpdateStatus updateStatus, SMOScriptOptions options, bool bCheckOnly)
        {
            _nsByAzure = ReadNotSupportedFile(ConfigurationManager.AppSettings["NotSupportedByAzureFileName"]);
            _updateStatus = updateStatus;
            _SqlServerSelection = null;
            _options = options;
            _scrptr = null;
            _swTSQL = null;
            _bCheckCompatibilityOnly = bCheckOnly;
        }

        public void Initialize(Server svr, Database sourceDB, AsyncUpdateStatus updateStatus, SMOScriptOptions options, StreamWriter swTSQL)
        {
            _nsByAzure = ReadNotSupportedFile(ConfigurationManager.AppSettings["NotSupportedByAzureFileName"]);
            _SqlServerSelection = svr;
            _ServerInstance = _SqlServerSelection.ConnectionContext.ServerInstance;
            _Login = _SqlServerSelection.ConnectionContext.Login;
            _Password = _SqlServerSelection.ConnectionContext.Password;
            _LoginSecure = _SqlServerSelection.ConnectionContext.LoginSecure;

            _sourceDB = sourceDB;
            _options = options;
            _swTSQL = swTSQL;
            _updateStatus = updateStatus;
            _bCheckCompatibilityOnly = false;
            if (_options != null)
            {
                _scrptr = GetScripter(_options);
            }
        }

        private Color GetSeverityColor(bool bSQL, int severity)
        {
            Color msgColor = bSQL ? Color.DarkGreen : Color.Black;

            switch (severity)
            {
                case 0:
                    break;

                case 1:
                    msgColor = Color.Brown;
                    break;

                case 2:
                    msgColor = Color.Red;
                    break;

                case 3:
                    msgColor = Color.DarkCyan;
                    break;
            }
            return msgColor;
        }

        public void OutputSQLString(string str, int severity)
        {
            OutputSQLString(str, GetSeverityColor(true, severity));
        }

        public void OutputSQLString(string str, Color fgColor)
        {
            if (_swTSQL != null)
            {
                _swTSQL.WriteLine(str);
            }
            else
            {
                _eventArgs.FunctionCode = NotificationEventFunctionCode.SqlOutput;
                _eventArgs.DisplayColor = fgColor;
                _eventArgs.DisplayText = str.Substring(str.Length -2).Equals(Environment.NewLine) ? str : str  + Environment.NewLine;
                _updateStatus(_eventArgs);
            }
        }

        public void OutputAnalysisLine(string str, int severity)
        {
            OutputAnalysisLine(str, GetSeverityColor(false, severity));
        }

        public void OutputAnalysisLine(string str, Color msgColor)
        {
            OutputAnalysis(str + Environment.NewLine, msgColor);
        }

        public void OutputAnalysis(string str, Color msgColor)
        {
            _eventArgs.FunctionCode = NotificationEventFunctionCode.AnalysisOutput;
            _eventArgs.DisplayColor = msgColor;
            _eventArgs.DisplayText = str.TrimStart(' ', '\t');
            _updateStatus(_eventArgs);
        }

        private void OutputMessage(string msg, SqlSmoObject obj, int severity)
        {
            string message = "";

            if (obj != null)
            {
                try
                {
                    string objType = obj.GetType().ToString();
                    message = objType.Substring(1 + objType.LastIndexOfAny(".".ToCharArray())) + " " + obj.ToString() + " -- " + msg;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    message = msg;
                }
            }
            else
            {
                message = msg;
            }

            OutputAnalysisLine(message + Environment.NewLine, severity);
            OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveComment) + " " + message.Replace(Environment.NewLine, "").Replace("\n", ""), severity);
        }

        private TSQLNotSupportedByAzure ReadNotSupportedFile(string nsFile)
        {
            if (nsFile == null) return null;

            XmlSerializer xmls = new XmlSerializer(typeof(TSQLNotSupportedByAzure));
            TSQLNotSupportedByAzure nsByAzure;
            using (FileStream fs = new FileStream(nsFile, FileMode.Open, FileAccess.Read))
            {
                nsByAzure = (TSQLNotSupportedByAzure)xmls.Deserialize(fs);
            }
            return nsByAzure;
        }

        public bool SkipCheck(string sqlString)
        {
            foreach (SupportedStatement ss in _nsByAzure.Skip)
            {
                if (ss.Text.Equals(sqlString, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public string TSQLChecks(string sqlStr, SqlSmoObject obj, int traceFileRow)
        {
            string output = sqlStr;
            _CommentAreaHelper.CommentContinuedFromLastCommand = _CommentAreaHelper.CommentContinued;
            _CommentAreaHelper.CommentNestedLevelFromLastCommand = _CommentAreaHelper.CommentNestedLevel;

            if (SkipCheck(sqlStr)) return output;

            if (_options.GeneralTSQL && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.GeneralTSQL, output, obj, traceFileRow);
            }

            if (_options.DistributedQueriesSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.DistributedQueriesSP, output, obj, traceFileRow);
            }

            if (_options.ProfilerSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.ProfilerSP, output, obj, traceFileRow);
            }

            if (_options.ActiveDirectorySP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.ActiveDirectorySP, output, obj, traceFileRow);
            }

            if (_options.BackupandRestoreTable && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.BackupandRestoreTable, output, obj, traceFileRow);
            }

            if (_options.ChangeDataCapture && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.ChangeDataCapture, output, obj, traceFileRow);
            }

            if (_options.ChangeDataCaptureTable && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.ChangeDataCaptureTable, output, obj, traceFileRow);
            }

            if (_options.DatabaseEngineSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.DatabaseEngineSP, output, obj, traceFileRow);
            }

            if (_options.DatabaseMailSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.DatabaseMailSP, output, obj, traceFileRow);
            }

            if (_options.DatabaseMaintenancePlan && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.DatabaseMaintenancePlan, output, obj, traceFileRow);
            }

            if (_options.DataControl && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.DataControl, output, obj, traceFileRow);
            }

            if (_options.FullTextSearchSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.FullTextSearchSP, output, obj, traceFileRow);
            }

            if (_options.GeneralExtendedSPs && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.GeneralExtendedSPs, output, obj, traceFileRow);
            }

            if (_options.IntegrationServicesTable && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.IntegrationServicesTable, output, obj, traceFileRow);
            }

            if (_options.LogShipping && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.LogShipping, output, obj, traceFileRow);
            }

            if (_options.MetadataFunction && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.MetadataFunction, output, obj, traceFileRow);
            }

            if (_options.OLEAutomationSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.OLEAutomationSP, output, obj, traceFileRow);
            }

            if (_options.OLEDBTable && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.OLEDBTable, output, obj, traceFileRow);
            }

            if (_options.ReplicationSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.ReplicationSP, output, obj, traceFileRow);
            }

            if (_options.ReplicationTable && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.ReplicationTable, output, obj, traceFileRow);
            }

            if (_options.RowsetFunction && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.RowsetFunction, output, obj, traceFileRow);
            }

            if (_options.SecurityFunction && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SecurityFunction, output, obj, traceFileRow);
            }

            if (_options.SecuritySP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SecuritySP, output, obj, traceFileRow);
            }

            if (_options.SQLMailSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SQLMailSP, output, obj, traceFileRow);
            }

            if (_options.SQLServerAgentSP && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SQLServerAgentSP, output, obj, traceFileRow);
            }

            if (_options.SQLServerAgentTable && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SQLServerAgentTable, output, obj, traceFileRow);
            }

            if (_options.SystemCatalogView && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SystemCatalogView, output, obj, traceFileRow);
            }

            if (_options.SystemFunction && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SystemFunction, output, obj, traceFileRow);
            }

            if (_options.SystemStatisticalFunction && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.SystemStatisticalFunction, output, obj, traceFileRow);
            }

            if (_options.Unclassified && _options.CheckCompatibility() == 2 || _options.CheckCompatibility() == 0)
            {
                output = RemoveUnsupportedOptions(_nsByAzure.Unclassified, output, obj, traceFileRow);
            }
            return output;
        }

        private string ReplaceAndDiaplayWarningIfRequired(NotSupported ns, string sqlStr, SqlSmoObject obj, int traceFileRow)
        {
            if (sqlStr == null || sqlStr.Length == 0) return "";

            Match exp = Regex.Match(sqlStr, ns.Text, RegexOptions.IgnoreCase);
            if (exp.Success)
            {
                while (exp != null && exp.Value.Length > 0)
                {
                    // Ok, we found a match, but was it in a comment area
                    if (_CommentAreaHelper.IsMatchInComments(exp))
                    {
                        // Yes, match found was in comments area.  We can ingor it and move on to the next match.
                        exp = exp.NextMatch();
                        continue;
                    }

                    if (ns.NotSubStr != null && ns.NotSubStr.Length > 0)
                    {
                        // Ok, this is here because AFAIK, Regex does not have a NOT string.  In otherwords, when I do the
                        // first regex search, I might get a long string that is a positive match yet it contains a word
                        // that negates the match.  So, we have a second search to look for something that will invalidate
                        // the match.

                        Match exp2 = Regex.Match(exp.Value, ns.NotSubStr, RegexOptions.IgnoreCase);
                        if (exp2.Success)
                        {
                            // Yes, we found a string that invalidates the match.  Get out of here.
                            exp = exp.NextMatch();
                            continue;
                        }
                    }

                    if (ns.DisplayWarning)
                    {
                        StringBuilder message = new StringBuilder();

                        if (_bCheckCompatibilityOnly && traceFileRow > 0)
                        {
                            message.Append(CommonFunc.FormatString(Properties.Resources.TraceFileLine, traceFileRow.ToString(CultureInfo.CurrentCulture)));
                        }

                        if (ns.DefaultMessage)
                        {
                            message.Append(CommonFunc.FormatString(_nsByAzure.DefaultMessage, exp.Value));
                        }
                        else
                        {
                            message.Append(CommonFunc.FormatString(ns.WarningMessage, exp.Value));
                        }
                        OutputMessage(message.ToString(), obj, ns.SeverityLevel);
                    }

                    if (ns.RemoveCommand && _bCheckCompatibilityOnly == false)
                    {
                        sqlStr = string.Empty;
                        break;
                    }
                    else if (ns.ReplaceString && _bCheckCompatibilityOnly == false)
                    {
                        if (ns.SearchReplace != null && ns.SearchReplace.Length > 0)
                        {
                            int start = sqlStr.IndexOf(exp.Value);
                            int end = start + exp.Value.Length;

                            sqlStr = sqlStr.Substring(0, start) + Regex.Replace(sqlStr.Substring(start, exp.Value.Length), ns.SearchReplace, ns.ReplaceWith, RegexOptions.IgnoreCase) + sqlStr.Substring(end);
                        }
                        else
                        {
                            sqlStr = Regex.Replace(sqlStr, ns.Text, ns.ReplaceWith, RegexOptions.IgnoreCase);
                        }

                        try
                        {
                            if (sqlStr == null || sqlStr.Length == 0) return "";

                            _CommentAreaHelper.FindCommentAreas(sqlStr);
                        }
                        catch (Exception ex)
                        {
                            OutputAnalysis("Encountered a problem parsing string: " + sqlStr + Environment.NewLine, Color.Red);
                            OutputAnalysis("Exception: " + ex.ToString() + Environment.NewLine, Color.Red);
                            return sqlStr;
                        }
                    }
                    exp = exp.NextMatch();
                }
            }
            return sqlStr;
        }

        private string RemoveUnsupportedIndexOptions(string sqlStr, SqlSmoObject obj, int traceFileRow)
        {
            if (_options.CheckCompatibility() == 1)
            {
                return sqlStr;
            }

            //")WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\r\n) ON [PRIMARY]\r\n"

            string output = RemoveUnsupportedOptions(_nsByAzure.Index.IndexOptions, sqlStr, obj, traceFileRow);
            output = Regex.Replace(output, "\\(, ", "(");
            return Regex.Replace(output, "WITH, ", "WITH (", RegexOptions.IgnoreCase);
        }

        private string RemoveUnsupportedOptions(NotSupportedList nsl, string sqlStr, SqlSmoObject obj, int traceFileRow)
        {
            string output = sqlStr;

            try
            {
                _CommentAreaHelper.FindCommentAreas(sqlStr);
                if (_CommentAreaHelper.CommentAreas.Count == 1)
                {
                    if (_CommentAreaHelper.CommentAreas[0].Start == 0 && (_CommentAreaHelper.CommentAreas[0].End - sqlStr.Length - _CommentAreaHelper.CrLf - 1) < 3)
                    {
                        return sqlStr;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!AsyncProcessingStatus.CancelProcessing)
                {
                    OutputAnalysis("Encountered a problem parsing string: " + sqlStr + Environment.NewLine, Color.Red);
                    OutputAnalysis("Exception: " + ex.ToString() + Environment.NewLine, Color.Red);
                }
                return sqlStr;
            }

            foreach (NotSupported ns in nsl)
            {
                output = ReplaceAndDiaplayWarningIfRequired(ns, output, obj, traceFileRow);
            }
            return output;
        }

        private string RemoveUnsupportedTableStatement(string sqlStr, SqlSmoObject obj)
        {
            int traceFileRow = 0;

            if (_options.CheckCompatibility() == 1)
            {
                return sqlStr;
            }

            string working = "";

            int withIdx = sqlStr.IndexOf("WITH (", StringComparison.OrdinalIgnoreCase);

            if (withIdx > 0)
            {
                working = sqlStr.Substring(0, withIdx) + RemoveUnsupportedIndexOptions(sqlStr.Substring(withIdx), obj, traceFileRow);
            }
            else
            {
                working = sqlStr;
            }

            if (Regex.IsMatch(working, "(CREATE|ALTER)\\sTABLE", RegexOptions.IgnoreCase))
            {
                if (_options.TargetServer.Equals(Properties.Resources.ServerType_SQLAzureFed))
                {
                    working = RemoveUnsupportedOptions(_nsByAzure.Table.FedColumnTypes, working, obj, traceFileRow);
                }
                working = RemoveUnsupportedOptions(_nsByAzure.Table.TableStatement, working, obj, traceFileRow);
            }
            else if (Regex.IsMatch(working, "CREATE.*INDEX", RegexOptions.IgnoreCase))
            {
                // Ok, there is one issue that we need to look for when converting a non clustered index into a clustered index
                // and that is having a non clustered index created with an INCLUDE parameter.
                if (Regex.IsMatch(working, @"CREATE\sCLUSTERED\sINDEX", RegexOptions.IgnoreCase))
                {
                    working = Regex.Replace(working, @"\r\nINCLUDE\s[\w\W]*\)\s", "");
                }

                working = RemoveUnsupportedOptions(_nsByAzure.Index.IndexOptions, working, obj, traceFileRow);
            }
            else
            {
                working = RemoveUnsupportedOptions(_nsByAzure.GeneralTSQL, working, obj, traceFileRow);
            }
            return working;
        }

        private Scripter GetScripter(SMOScriptOptions options)
        {
            Scripter scrptr = new Scripter(_SqlServerSelection);

            if (_SqlServerSelection.ConnectionContext.DatabaseEngineType == DatabaseEngineType.SqlAzureDatabase)
            {
                //scrptr.Options.SetTargetDatabaseEngineType(DatabaseEngineType.Standalone);
                scrptr.Options.SetTargetServerVersion(new ServerVersion(10, 0));
            }

            scrptr.Options.AppendToFile = false;
            scrptr.Options.AnsiPadding = false;
            scrptr.Options.ClusteredIndexes = true;
            scrptr.Options.Default = options.ScriptDefaults; // ***
            scrptr.Options.IncludeHeaders = options.ScriptHeaders; // ***
            scrptr.Options.IncludeIfNotExists = options.IncludeIfNotExists; // ***
            scrptr.Options.SchemaQualifyForeignKeysReferences = true;
            scrptr.Options.ScriptDrops = false;
            scrptr.Options.ScriptSchema = true;
            scrptr.Options.WithDependencies = false;
            scrptr.Options.NoCollation = !options.ScriptCollation;

            scrptr.Options.DriAllConstraints = options.ScriptCheckConstraints; // ***
            scrptr.Options.DriPrimaryKey = options.ScriptPrimaryKeys; // ***
            scrptr.Options.DriForeignKeys = false; // options.ScriptForeignKeys; // ***
            scrptr.Options.DriUniqueKeys = options.ScriptUniqueKeys; // ***
            scrptr.Options.Indexes = options.ScriptIndexes; // ***
            scrptr.Options.NoTablePartitioningSchemes = true;
            scrptr.Options.NoFileGroup = true;
            scrptr.Options.NoAssemblies = true;
            scrptr.Options.NoFileStream = true;
            scrptr.Options.NoFileStreamColumn = true;
            scrptr.Options.NoTablePartitioningSchemes = false;
            scrptr.Options.Triggers = options.ScriptTableTriggers; // ***

            if (_options.TargetServer.Equals(Properties.Resources.ServerType_SQLServer, StringComparison.Ordinal))
            {
                scrptr.Options.ExtendedProperties = true;
            }
            else
            {
                scrptr.Options.ExtendedProperties = false;
            }

            return scrptr;
        }

        public void ScriptObjectsOneByOneWithCheck(SqlSmoObject[] smoObjects)
        {
            SqlSmoObject[] objs = new SqlSmoObject[1];
            string clean;
            StringCollection strColl;

            foreach (SqlSmoObject obj in smoObjects)
            {
                if (AsyncProcessingStatus.CancelProcessing) return;

                try
                {
                    objs[0] = obj;
                    strColl = _scrptr.Script(objs);

                    foreach (String str in strColl)
                    {
                        clean = TSQLChecks(str, obj, 0);
                        if (clean.Length > 0)
                        {
                            if (obj.Urn.Type == "DdlTrigger")
                            {
                                if (Regex.IsMatch(clean, "^(DISABLE|ENABLE)", RegexOptions.IgnoreCase))
                                {
                                    OutputSQLString(Properties.Resources.Go, Color.Blue);
                                }
                            }
                            OutputSQLString(clean, Color.Black); // + Environment.NewLine
                        }
                    }
                    OutputSQLString(Properties.Resources.Go, Color.Blue);
                }
                catch (SmoException ex)
                {
                    if (ex.InnerException != null)
                    {
                        OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.SMOScriptingError, obj.ToString(), ex.InnerException.Message) + Environment.NewLine, Color.Red);
                    }
                    else
                    {
                        OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.SMOScriptingError, obj.ToString(), ex.Message) + Environment.NewLine, Color.Red);
                    }
                    _scrptr.Server.ConnectionContext.Disconnect();
                }
            }
        }

        public void ScriptObjectsNoCheck(SqlSmoObject[] smoObjects)
        {
            StringCollection strColl;

            strColl = _scrptr.Script(smoObjects);

            foreach (String str in strColl)
            {
                if (AsyncProcessingStatus.CancelProcessing) return;

                OutputSQLString(str, Color.Black);
            }
            OutputSQLString(Properties.Resources.Go, Color.Blue);
        }

        //public void FlagDrops(SqlSmoObject[] smoObjects)
        //{
        //    SqlSmoObject[] sso = new SqlSmoObject[1];

        //    _scrptr.Options.ScriptDrops = true;
        //    // "-- CREATE DROP FOR:"
        //    foreach (SqlSmoObject obj in smoObjects)
        //    {
        //        sso[0] = obj;
        //        switch (obj.Urn.Type)
        //        {
        //            case "Table":
        //                OutputSQLString("-- Create drop for:" + ((Table)obj).ToString(), Color.DarkGreen);
        //                OutputSQLString(Properties.Resources.Go, Color.Blue);
        //                break;

        //            default:
        //                ScriptObjectsNoCheck(sso);
        //                break;
        //        }
        //    }
        //    _scrptr.Options.ScriptDrops = false;
        //}

        public void ScriptDrops(SqlSmoObject[] smoObjects)
        {
            _scrptr.Options.ScriptDrops = true;
            ScriptObjectsNoCheck(smoObjects);
            _scrptr.Options.ScriptDrops = false;
        }

        public void ScriptSchemas(SqlSmoObject[] smoObjects)
        {
            SqlSmoObject[] objs = new SqlSmoObject[1];
            string clean;
            StringCollection strColl;

            try
            {
                foreach (SqlSmoObject obj in smoObjects)
                {
                    if (AsyncProcessingStatus.CancelProcessing) return;

                    objs[0] = obj;
                    strColl = _scrptr.Script(objs);

                    foreach (String sqlStr in strColl)
                    {
                        if (_options.CheckCompatibility() == 1)
                        {
                            OutputSQLString(sqlStr, Color.Black); // + Environment.NewLine;
                        }
                        else
                        {
                            clean = RemoveUnsupportedOptions(_nsByAzure.Schema.SchemaChecks, sqlStr, obj, 0);
                            if (clean.Length > 0)
                            {
                                OutputSQLString(clean, Color.Black); // + Environment.NewLine
                            }
                        }
                    }
                    OutputSQLString(Properties.Resources.Go, Color.Blue);
                }
            }
            catch (SmoException ex)
            {
                if (ex.InnerException != null)
                {
                    OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.SMOScriptingError, objs[0].ToString(), ex.InnerException.Message) + Environment.NewLine, Color.Red);
                }
                else
                {
                    OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.SMOScriptingError, objs[0].ToString(), ex.Message) + Environment.NewLine, Color.Red);
                }
                _scrptr.Server.ConnectionContext.Disconnect();
            }
        }

        public void ScriptXMLSchemaCollections(SqlSmoObject[] smoObjects)
        {
            if (_options.CheckCompatibility() != 1)
            {
                OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentStart), Color.DarkGreen);
                OutputSQLString(CommonFunc.FormatString(Properties.Resources.XMLSchemaCollectionNotSupported) + Environment.NewLine, Color.Red);
                OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.XMLSchemaCollectionNotSupported2) + Environment.NewLine, Color.Red);

                foreach (SqlSmoObject xsc in smoObjects)
                {
                    if (AsyncProcessingStatus.CancelProcessing) return;

                    OutputAnalysisLine("    " + xsc.ToString(), Color.Red);
                }

                OutputAnalysisLine("", Color.Black);

                // CREATE XML SCHEMA COLLECTION [HumanResources].[HRResumeSchemaCollection] AS
                ScriptObjectsNoCheck(smoObjects);

                OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentEnd) + Environment.NewLine, Color.DarkGreen);
            }
            else
            {
                ScriptObjectsNoCheck(smoObjects);
            }
            OutputSQLString(Properties.Resources.Go, Color.Blue);
        }

        public void ScriptUDT(SqlSmoObject[] smoObjects)
        {
            ScriptObjectsNoCheck(smoObjects);
        }

        public void ScriptUDTT(SqlSmoObject[] smoObjects)
        {
            ScriptObjectsNoCheck(smoObjects);
        }

        public void ScriptUDF(SqlSmoObject[] smoObjects)
        {
            ScriptObjectsOneByOneWithCheck(smoObjects);
        }

        public void ScriptRoles(SqlSmoObject[] smoObjects)
        {
            ScriptObjectsNoCheck(smoObjects);
        }

        public void ScriptTriggers(SqlSmoObject[] smoObjects)
        {
            ScriptObjectsOneByOneWithCheck(smoObjects);
        }

        private static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                _bcpOutput.Append(outLine.Data + Environment.NewLine);
            }
        }

        public long DownloadTableData(string table, string bcpArgsOut, string file)
        {
            long numberOfRowsDownloaded = 0;
            NameValueCollection englishLanguage = (NameValueCollection)ConfigurationManager.GetSection("en-US");
            NameValueCollection defaultLanguage = (NameValueCollection)ConfigurationManager.GetSection(Thread.CurrentThread.CurrentCulture.Name);
            if (defaultLanguage == null)
            {
                defaultLanguage = englishLanguage;
                if (defaultLanguage == null)
                {
                    OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.LocalizedBCPCanNotFind, Thread.CurrentThread.CurrentCulture.Name, Application.ExecutablePath), Color.Red);
                    return numberOfRowsDownloaded;
                }
            }
            OutputAnalysis(CommonFunc.FormatString(Properties.Resources.BCPUsing) + table + Environment.NewLine, Color.DarkSlateBlue);
            OutputAnalysis(ConfigurationManager.AppSettings["BCPExe"] + " " + bcpArgsOut + Environment.NewLine, Color.DarkSlateBlue);

            using (Process p = new Process())
            {
                int cnt = 0;
                Color curCol = Color.DarkBlue;

                p.StartInfo.FileName = ConfigurationManager.AppSettings["BCPExe"];
                p.StartInfo.Arguments = bcpArgsOut;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(defaultLanguage["BCPCodePage"]);
                p.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputHandler);
                try
                {
                    p.Start();
                    p.BeginOutputReadLine();
                }
                catch (Exception ex)
                {
                    OutputAnalysisLine(ex.ToString() + Environment.NewLine, Color.Red);
                    return 0;
                }

                while (!p.HasExited)
                {
                    if (AsyncProcessingStatus.CancelProcessing)
                    {
                        p.Kill();
                        p.Close();
                        return numberOfRowsDownloaded;
                    }

                    OutputAnalysis("*", curCol);
                    curCol = (curCol == Color.DarkBlue) ? Color.DarkRed : Color.DarkBlue;

                    if (cnt++ > 60)
                    {
                        OutputAnalysis(Environment.NewLine, Color.DarkBlue);
                        cnt = 0;
                    }

                    p.WaitForExit(1000);
                }
                OutputAnalysisLine("", Color.Black);
                p.Close();
            }

            // Expecting output from bcp.  If _bcpOutput is empty, then sleep for a second and let async thread ProcessOutputHandler catch up
            // if after a few seconds, it is still empty, output an error message and give up.

            if (_bcpOutput.Length == 0)
            {
                Debug.WriteLine("Missing output from BCP");
                int cnt = 0;
                for (int loop = 0; loop < 3; loop++)
                {
                    Thread.Sleep(1000);
                    if (_bcpOutput.Length > 0 && cnt == _bcpOutput.Length) break;
                    if (_bcpOutput.Length > cnt)
                    {
                        loop = 0;
                        Debug.WriteLine("Still getting data.  Check for more.");
                    }
                    cnt = _bcpOutput.Length;
                }
            }

            if (_bcpOutput.Length == 0)
            {
                OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.BCPMissingOutput, "BCP " + bcpArgsOut, table) + Environment.NewLine, Color.DarkRed);
            }
            else if ((Regex.IsMatch(_bcpOutput.ToString(), defaultLanguage["BCPError"]) || Regex.IsMatch(_bcpOutput.ToString(), englishLanguage["BCPError"])) &&
                    !(Regex.IsMatch(_bcpOutput.ToString(), defaultLanguage["BCPWarning"]) || Regex.IsMatch(_bcpOutput.ToString(), englishLanguage["BCPWarning"])))
            {
                OutputAnalysisLine(_bcpOutput.ToString(), Color.Red);
            }
            else
            {
                Match regMatch = Regex.Match(_bcpOutput.ToString(), defaultLanguage["BCPRowsCopied"]);
                if (regMatch.Success)
                {
                    Match rows = Regex.Match(regMatch.Value, defaultLanguage["BCPNumber"]);
                    numberOfRowsDownloaded = Convert.ToInt64(rows.Value, CultureInfo.InvariantCulture);

                    OutputAnalysisLine(_bcpOutput.ToString(), Color.DarkSlateBlue);
                    OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.BCPOutputFile) + file + Environment.NewLine, Color.DarkSlateBlue);
                }
                else
                {
                    regMatch = Regex.Match(_bcpOutput.ToString(), englishLanguage["BCPRowsCopied"]);
                    if (regMatch.Success)
                    {
                        Match rows = Regex.Match(regMatch.Value, englishLanguage["BCPNumber"]);
                        numberOfRowsDownloaded = Convert.ToInt64(rows.Value, CultureInfo.InvariantCulture);

                        OutputAnalysisLine(_bcpOutput.ToString(), Color.DarkSlateBlue);
                        OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.BCPOutputFile) + file + Environment.NewLine, Color.DarkSlateBlue);
                    }
                    else
                    {
                        OutputAnalysisLine(Properties.Resources.BCPCouldNotFind1 + Environment.NewLine, Color.DarkRed);
                        OutputAnalysisLine("BCP " + bcpArgsOut.ToString().ToString() + Environment.NewLine, Color.DarkMagenta);
                        OutputAnalysisLine(Properties.Resources.BCPHereIsOutput + Environment.NewLine, Color.DarkRed);
                        OutputAnalysisLine(_bcpOutput.ToString(), Color.DarkMagenta);
                        OutputAnalysisLine(Properties.Resources.BCPExpectedRegexMatch + defaultLanguage["BCPRowsCopied"] + Environment.NewLine, Color.DarkRed);
                    }
                }
            }
            _bcpOutput.Remove(0, _bcpOutput.Length);
            return numberOfRowsDownloaded;
        }

        public void ScriptTableData(SqlSmoObject obj, ref StringCollection bcpOutputCommands, ref StringCollection bcpTargetCommands, ref string outputDir)
        {
            try
            {
                Table tbl = (Table)obj;
                string tableName = Regex.Replace(tbl.ToString(), "(\\[|\\])", "");
                string file = "";

                if (outputDir == "")
                {
                    file = "\"" + Path.GetTempFileName() + "\"";
                }
                else
                {
                    try
                    {
                        DirectoryInfo di = Directory.CreateDirectory(outputDir);
                        if (di.Exists)
                        {
                            file = "\"" + CommonFunc.GetBcpOutputFileName(di.FullName, tableName, 0, ConfigurationManager.AppSettings["BCPFileExt"]) + "\"";
                        }
                        else
                        {
                            OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.BCPOutputDirectoryNotFound, outputDir), Color.Red);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        OutputAnalysisLine(ex.Message, Color.Red);
                        return;
                    }
                }

                // Ok, before we do anything, lets see if we actually have data to migrate
                // Note that SMO does not support RowCount for SqlAzureDatabase

                if (_SqlServerSelection.ConnectionContext.DatabaseEngineType == DatabaseEngineType.SqlAzureDatabase)
                {
                    string connectionStr = "server=" + _ServerInstance + ";database=" + _sourceDB.Name + ";uid=" + _Login + ";pwd=" + "'" + _Password.Replace("'", "''") + "'";
                    string errMessage = "";

                    try
                    {
                        int rowCount = 0;
                        Retry.ExecuteRetryAction(() =>
                            {
                                using (SqlConnection con = new SqlConnection(connectionStr))
                                {
                                    ScalarResults sr = SqlHelper.ExecuteScalar(con, CommandType.Text, "SELECT COUNT(*) FROM " + tbl.ToString() + " WITH (NOLOCK)");
                                    rowCount = (int)sr.ExecuteScalarReturnValue;
                                }
                            });
                        if (rowCount < 1) return;
                    }
                    catch (Exception ex)
                    {
                        errMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        OutputAnalysisLine(errMessage, Color.Red);
                        return;
                    }
                }
                else
                {
                    if (tbl.RowCount < 1)  return;
                }

                StringBuilder bcpArgsOut = new StringBuilder(CommonFunc.FormatString(CommonFunc.GetAppSettingsStringValue("BCPArgsOut"), tbl.Parent.ToString() + "." + tbl.ToString(), file));
                StringBuilder bcpArgsIn = new StringBuilder(CommonFunc.FormatString(CommonFunc.GetAppSettingsStringValue("BCPArgsIn"), tbl.ToString(), file));

                bcpArgsOut.Append(" -S " + _ServerInstance);
                if (_LoginSecure)
                {
                    bcpArgsOut.Append(" -T ");
                }
                else
                {
                    string login = _Login[0] == '"' ? _Login : "\"" + _Login + "\"";
                    string password = "\"\""; // Empty Password

                    if (_Password.Length > 0)
                    {
                        password = _Password[0] == '"' ? _Password : "\"" + _Password + "\"";
                    }
                    bcpArgsOut.Append(" -U " + login + " -P " + password);
                }

                bcpOutputCommands.Add(CommonFunc.GetAppSettingsStringValue("BCPExe") + " " + bcpArgsOut.ToString());

                long numberOfRowsDownloaded = DownloadTableData(tbl.ToString(), bcpArgsOut.ToString(), file);

                if (numberOfRowsDownloaded > 0)
                {
                    bcpTargetCommands.Add("-- BCPArgs:" + numberOfRowsDownloaded + ":" + bcpArgsIn);
                }
            }
            catch (Exception ex)
            {
                string errMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                OutputAnalysisLine(errMessage, Color.Red);
                OutputAnalysisLine(ex.ToString(), Color.Red);
            }
        }

        public void ScriptStringCollection(ref StringCollection sc, Color color)
        {
            foreach (string str in sc)
            {
                OutputSQLString(str, color);
                OutputSQLString(Properties.Resources.Go, Color.Blue);
            }
        }

        public string FederatedColumn(FederationDetails fed, TargetServerInfo serverInfo, FederationMemberDistribution member,  string schema, string table)
        {
            string columnName = "";
            string use = FederationCommonFuncs.GetUseFederation(fed.FederationName, ref member);

            string tsql = "SELECT ISNULL((SELECT name FROM sys.columns where object_id = t.object_id AND column_id = ftc.column_id), '') AS FederatedColumn" +
                          "  FROM sys.tables t" +
                          "  JOIN sys.schemas s ON t.schema_id = s.schema_id AND s.name = '" + schema + "'" +
                          "  LEFT JOIN sys.federated_table_columns ftc ON ftc.object_id = t.object_id" +
                          " WHERE t.name = '" + table + "'";

            using (SqlConnection connection = new SqlConnection(serverInfo.ConnectionStringRootDatabase))
            {
                Retry.ExecuteRetryAction(() =>
                    {
                        connection.Open();
                        SqlHelper.ExecuteNonQuery(connection, CommandType.Text, use);
                        ScalarResults sr = SqlHelper.ExecuteScalar(connection, CommandType.Text, tsql);
                        if (sr.ExecuteScalarReturnValue == null)
                        {
                            columnName = "";
                        }
                        else
                        {
                            columnName = sr.ExecuteScalarReturnValue.ToString();
                        }
                        connection.Close();
                    }, () =>
                    {
                        connection.Close();
                    });
            }

            return columnName.Length > 0 ? "FEDERATED ON ([" + member.DistrubutionName + "] = [" + columnName + "])" : "";
        }

        public void ScriptTables(SqlSmoObject[] smoObjects, ref StringCollection tableForeignKey, ref StringCollection bcpOutputCommands, ref StringCollection bcpTargetCommands, ref string outputDir)
        {
            SqlSmoObject[] objs = new SqlSmoObject[1];
            StringCollection strColl = null;
            StringCollection collAlterTableCommands = new StringCollection();

            string clean;

            foreach (SqlSmoObject obj in smoObjects)
            {
                if (AsyncProcessingStatus.CancelProcessing) return;

                Table tbl = (Table)obj;
                objs[0] = tbl;

                if (_options.ScriptTable())
                {
                    if (tbl.Indexes.OfType<Index>().Where(i => i.IsClustered).Count() == 0 && _options.CheckCompatibility() != 1)
                    {
                        if (tbl.Indexes.Count == 0)
                        {
                            //create a new clustered index for the heap
                            Index ci = new Index(tbl, "ci_azure_fixup_" + Regex.Replace(tbl.ToString(), "(\\[|\\])", "").Replace('.', '_'));
                            ci.IndexKeyType = IndexKeyType.None;
                            ci.IsClustered = true;

                            var cols = tbl.Columns.OfType<Column>();
                            SqlDataType[] blobTypes = new SqlDataType[] { SqlDataType.VarCharMax, SqlDataType.VarBinaryMax, SqlDataType.Xml, SqlDataType.Image, SqlDataType.Text };
                            SqlDataType[] likelyKeyTypes = new SqlDataType[] { SqlDataType.SmallInt, SqlDataType.Int, SqlDataType.BigInt };

                            Func<Column, bool> IsBlob = c => blobTypes.Contains(c.DataType.SqlDataType);
                            Func<Column, bool> IsKeyType = c => likelyKeyTypes.Contains(c.DataType.SqlDataType);

                            //arbitrarilly pick a single column for the index
                            var col = cols.Where(c => c.Identity).FirstOrDefault()
                                      ?? cols.Where(c => c.Name.EndsWith(@"ID", StringComparison.OrdinalIgnoreCase) && IsKeyType(c)).FirstOrDefault()
                                      ?? cols.Where(c => IsKeyType(c)).FirstOrDefault()
                                      ?? cols.Where(c => !blobTypes.Contains(c.DataType.SqlDataType)).FirstOrDefault();

                            if (col == null)
                            {
                                strColl = _scrptr.Script(objs);
                                OutputSQLString(CommonFunc.FormatString(Properties.Resources.NoClusteredIndex, tbl.ToString()), Color.Brown);
                                OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.NoClusteredIndex, tbl.ToString()) + Environment.NewLine, Color.Brown);
                            }
                            else
                            {
                                ci.IndexedColumns.Add(new IndexedColumn(ci, col.Name));
                                tbl.Indexes.Add(ci);

                                try
                                {
                                    strColl = _scrptr.Script(objs);
                                    OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveComment + Properties.Resources.AddingClusteredIndex, ci.Name, tbl.ToString()), Color.Brown);
                                    OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.AddingClusteredIndex, ci.Name, tbl.ToString()) + Environment.NewLine, Color.Brown);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                    tbl.Indexes.Remove(ci);
                                    strColl = _scrptr.Script(objs);
                                    OutputSQLString(CommonFunc.FormatString(Properties.Resources.NoClusteredIndex, tbl.ToString()), Color.Brown);
                                    OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.NoClusteredIndex, tbl.ToString()) + Environment.NewLine, Color.Brown);
                                }
                            }
                        }
                        else
                        {
                            var ci = tbl.Indexes.OfType<Index>().Where(i => i.IsUnique).FirstOrDefault() ?? tbl.Indexes.OfType<Index>().First();
                            ci.IsClustered = true; //force a clusterd index.  This is just for the scripting.  We don't save it.

                            try
                            {
                                strColl = _scrptr.Script(objs);
                                OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveComment + Properties.Resources.ChangingClusteredIndex, tbl.ToString(), ci.Name), Color.Brown);
                                OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.ChangingClusteredIndex, tbl.ToString(), ci.Name) + Environment.NewLine, Color.Brown);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                ci.IsClustered = false;
                                strColl = _scrptr.Script(objs);
                                OutputSQLString(CommonFunc.FormatString(Properties.Resources.NoClusteredIndex, tbl.ToString()), Color.Brown);
                                OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.NoClusteredIndex, tbl.ToString()) + Environment.NewLine, Color.Brown);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            strColl = _scrptr.Script(objs);
                        }
                        catch (SmoException ex)
                        {
                            OutputAnalysisLine(ex.InnerException.Message + Environment.NewLine, Color.Red);
                        }
                    }

                    if (strColl != null)
                    {
                        foreach (String str in strColl)
                        {
                            clean = RemoveUnsupportedTableStatement(str, obj);
                            if (clean.Length > 0)
                            {
                                if (Regex.IsMatch(clean, "ALTER\\sTABLE\\s", RegexOptions.IgnoreCase))
                                {
                                    if (Regex.IsMatch(clean, "CHECK\\s+[\\s\\w]*CONSTRAINT", RegexOptions.IgnoreCase))
                                    {
                                        tableForeignKey.Add(clean);
                                    }
                                    else
                                    {
                                        collAlterTableCommands.Add(clean);
                                    }
                                    continue;
                                }

                                if (Regex.IsMatch(clean, "CREATE.*TRIGGER", RegexOptions.IgnoreCase))
                                {
                                    OutputSQLString(Properties.Resources.Go, Color.Blue);
                                }

#if SQLSERVER2008
                                // Must insert before END
                                if (_federationDetails != null && _options.IsTargetServerFederation() && Regex.IsMatch(clean, "CREATE.*TABLE ", RegexOptions.IgnoreCase))
                                {
                                    string federateon = FederatedColumn(_federationDetails, _serverInfo, _memberDistribution, tbl.Schema, tbl.Name);
                                    if (federateon.Length == 0)
                                    {
                                        OutputSQLString(clean, Color.Black);
                                    }
                                    else
                                    {
                                        if (clean.EndsWith("END"))
                                        {
                                            OutputSQLString(clean.Substring(0, clean.Length - 3) + federateon + Environment.NewLine + "END", Color.Black);
                                        }
                                        else
                                        {
                                            OutputSQLString(clean, Color.Black);
                                            OutputSQLString(federateon, Color.Black);
                                        }
                                    }
                                }
                                else
                                {
                                    OutputSQLString(clean, Color.Black);
                                }
                                
#else
                                if (!_options.IsTargetServerFederation())
                                {
                                    Match fed = Regex.Match(clean, @"\)FEDERATED\sON\s\(\[.*\]\)", RegexOptions.IgnoreCase);
                                    if (fed.Success)
                                    {
                                        // SQL Azure Federation objects are not supported in SQL Server 2008. To script the SQL Azure federation objects change the target database engine type to SQL Azure Database.

                                        string output = CommonFunc.FormatString(Properties.Resources.FederationNotSupported, fed.Value.Substring(1), tbl.ToString());
                                        OutputSQLString(Properties.Resources.RemoveComment + " " + output, Color.Brown);
                                        OutputAnalysisLine(output, Color.Brown);
                                        clean = Regex.Replace(clean, @"\)FEDERATED\sON\s\(\[.*\]\)\r\n\r\n", ")", RegexOptions.IgnoreCase);
                                    }
                                }
                                OutputSQLString(clean, Color.Black);
#endif
                            }
                        }
                        OutputSQLString(Properties.Resources.Go, Color.Blue);
                    }
                }

                if (_options.ScriptTableData() && _options.Migrate)
                {
                    ScriptTableData(obj, ref bcpOutputCommands, ref bcpTargetCommands, ref outputDir);
                }

                foreach (string str in collAlterTableCommands)
                {
                    clean = RemoveUnsupportedTableStatement(str, obj);
                    if (clean.Length > 0)
                    {
                        OutputSQLString(clean, Color.Black);
                        OutputSQLString(Properties.Resources.Go, Color.Blue);
                    }
                }
            }
        }

        public void ScriptProcedures(SqlSmoObject[] smoObjects)
        {
            ScriptObjectsOneByOneWithCheck(smoObjects);
        }

        public void ScriptViews(SqlSmoObject[] smoObjects)
        {
            SqlSmoObject[] objs = new SqlSmoObject[1];
            string clean;
            StringCollection strColl;

            foreach (SqlSmoObject obj in smoObjects)
            {
                if (AsyncProcessingStatus.CancelProcessing) return;

                try
                {
                    objs[0] = obj;
                    strColl = _scrptr.Script(objs);

                    foreach (String str in strColl)
                    {
                        if (Regex.IsMatch(str, "CREATE.*INDEX", RegexOptions.IgnoreCase))
                        {
                            OutputSQLString(Properties.Resources.Go, Color.Blue);
                        }

                        string working = @"";
                        int withIdx = str.IndexOf(@")WITH (", StringComparison.OrdinalIgnoreCase);

                        if (withIdx > 0)
                        {
                            working = str.Substring(0, withIdx) + RemoveUnsupportedIndexOptions(str.Substring(withIdx), obj, 0);
                        }
                        else
                        {
                            working = str;
                        }

                        clean = TSQLChecks(working, obj, 0);
                        if (clean.Length > 0)
                        {
                            OutputSQLString(clean, Color.Black); // + Environment.NewLine
                        }
                    }
                    OutputSQLString(Properties.Resources.Go, Color.Blue);
                }
                catch (SmoException ex)
                {
                    if (ex.InnerException != null)
                    {
                        OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.SMOScriptingError, obj.ToString(), ex.InnerException.Message) + Environment.NewLine, Color.Red);
                    }
                    else
                    {
                        OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.SMOScriptingError, obj.ToString(), ex.Message) + Environment.NewLine, Color.Red);
                    }
                    _scrptr.Server.ConnectionContext.Disconnect();
                }
            }
        }

        public void ParseFileTSQL(string sql)
        {
            string clean = string.Empty;

            if (sql.StartsWith("-- BCPArgs", StringComparison.OrdinalIgnoreCase))
            {
                clean = sql;
            }
            else
            {
                clean = TSQLChecks(sql, null, 0);
            }

            if (clean.Length > 0)
            {
                OutputSQLString(clean, Color.Black);
            }
        }

        public void ParseFileRole(string sql)
        {
            OutputSQLString(sql, Color.Black);
            OutputSQLString(Properties.Resources.Go, Color.Blue);
        }

        public void ParseFileTSQLGo(string sql)
        {
            ParseFileTSQL(sql);
            OutputSQLString(Properties.Resources.Go, Color.Blue);
        }

        public void ParseFileXMLSchemaCollections(string sql)
        {
            if (_options.CheckCompatibility() == 1)
            {
                OutputSQLString(sql, Color.Black);
                OutputSQLString(Properties.Resources.Go, Color.Blue);
                return;
            }

            OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentStart), Color.DarkGreen);
            OutputSQLString(CommonFunc.FormatString(Properties.Resources.XMLSchemaCollectionNotSupported) + Environment.NewLine, Color.Red);
            OutputAnalysisLine(CommonFunc.FormatString(Properties.Resources.XMLSchemaCollectionNotSupported1) + Environment.NewLine, Color.Red);

            // CREATE XML SCHEMA COLLECTION [HumanResources].[HRResumeSchemaCollection] AS

            OutputSQLString(sql, Color.Black);

            OutputSQLString(CommonFunc.FormatString(Properties.Resources.RemoveCommentEnd), Color.DarkGreen); // + Environment.NewLine
            OutputSQLString(Properties.Resources.Go, Color.Blue);
        }

        public void ParseFileUDT(string sql)
        {
            OutputSQLString(sql, Color.Black);
            OutputSQLString(Properties.Resources.Go, Color.Blue);
        }

        public void ParseFileIndex(string sql)
        {
            ParseFileTable(sql);
        }

        public void ParseFileTable(string sql)
        {
            string clean = RemoveUnsupportedTableStatement(sql, null);
            if (clean.Length > 0)
            {
                OutputSQLString(clean, Color.Black);
                OutputSQLString(Properties.Resources.Go, Color.Blue);
            }
        }
    }
}
