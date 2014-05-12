using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SQLAzureMWUtils
{
    public class TargetProcessor : Form
    {
        private static StringBuilder _bcpOutput = new StringBuilder(3000);
        private static FederationDetails _federationInfo;
        public bool CancelProcessing = false;

        public TargetProcessor(FederationDetails federationInfo = null)
        {
            _federationInfo = federationInfo;
        }

        public bool DoesDatabaseExist(TargetServerInfo targetServer)
        {
            bool dbExists = false;
            Retry.ExecuteRetryAction(() =>
            {
                using (SqlConnection connection = new SqlConnection(targetServer.ConnectionStringRootDatabase))
                {
                    string query = "IF (EXISTS(select 1 from sys.databases where name = '" + targetServer.TargetDatabase + "'))" + Environment.NewLine +
                                    "   SELECT 1" + Environment.NewLine +
                                    "ELSE" + Environment.NewLine +
                                    "   SELECT 0";
                    ScalarResults sr = SqlHelper.ExecuteScalar(connection, CommandType.Text, query.ToString());
                    dbExists = Convert.ToBoolean((int) sr.ExecuteScalarReturnValue);
                }
            });
            return dbExists;
        }

        public void CreateDatabase(TargetServerInfo targetServer, string collation, string edition, int dbSize, bool bDropDatabaseIfExists)
        {
            if (DoesDatabaseExist(targetServer))
            {
                if (bDropDatabaseIfExists)
                {
                    Retry.ExecuteRetryAction(() =>
                    {
                        using (SqlConnection connection = new SqlConnection(targetServer.ConnectionStringRootDatabase))
                        {
                            string tsql ="DROP DATABASE " + targetServer.TargetDatabase.Replace("\\[", "").Replace("\\])", "");
                            SqlHelper.ExecuteNonQuery(connection, CommandType.Text, tsql);
                        }
                    });
                }
                else
                {
                    return;
                }
            }

            try
            {
                StringBuilder sqlDBCreate = new StringBuilder("CREATE DATABASE ");
                if (targetServer.TargetDatabase.StartsWith("[", StringComparison.Ordinal) && targetServer.TargetDatabase.StartsWith("]", StringComparison.Ordinal))
                {
                    sqlDBCreate.Append(targetServer.TargetDatabase);
                }
                else
                {
                    sqlDBCreate.Append("[" + targetServer.TargetDatabase + "]");
                }

                if (collation.Length > 0)
                {
                    sqlDBCreate.Append(" COLLATE " + collation);
                }

                if (targetServer.ServerType == ServerTypes.SQLAzure)
                {
                    sqlDBCreate.Append(" (MAXSIZE = " + dbSize + " GB, EDITION = '" + edition + "')");
                }

                Retry.ExecuteRetryAction(() =>
                {
                    using (SqlConnection connection = new SqlConnection(targetServer.ConnectionStringRootDatabase))
                    {
                        SqlHelper.ExecuteNonQuery(connection, CommandType.Text, sqlDBCreate.ToString());
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs args)
        {
            if (!String.IsNullOrEmpty(args.Data))
            {
                _bcpOutput.Append(args.Data + Environment.NewLine);
            }
        }

        public void ResetConnection(SqlConnection con)
        {
            con.Close();
            Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["TimeToSleepOnConnectionReset"]));
            Retry.ExecuteRetryAction(() =>
                {
                    con.Open();
                });
        }

        public void QueueBCPJobs(AsyncUpdateStatus updateStatus, AsyncQueueBCPJob queueBCPJob, string schema, string table, long totalRowsCount, string bcpArgs, string connectionString)
        {
            AsyncProcessingStatus.FinishedProcessingJobs = false;

            BCPJobInfo jobInfo = new BCPJobInfo();
            jobInfo.FederationInfo = _federationInfo;
            jobInfo.Schema = schema;
            jobInfo.TableName = table;
            jobInfo.NumberOfRows = totalRowsCount;
            jobInfo.BCPUploadCommand = ConfigurationManager.AppSettings["BCPExe"] + " " + bcpArgs;
            jobInfo.ConnectionString = connectionString;
            jobInfo.JobStatus = CommandStatus.Waiting;
            jobInfo.UpdateStatus = updateStatus;

            AsyncQueueBCPJobArgs jobArgs = new AsyncQueueBCPJobArgs();
            jobArgs.JobInfo = jobInfo;
            queueBCPJob(jobArgs);
            return;
        }

        public void BCPUploadData(TargetServerInfo targetServer, AsyncUpdateStatus updateStatus, AsyncQueueBCPJob queueBCPJob, string bcpArgs, ref StringCollection bcpUploadCommands)
        {
            AsyncNotificationEventArgs eventArgs = new AsyncNotificationEventArgs(0, 0, "", "", Color.DarkSlateBlue);

            NameValueCollection englishLanguage = (NameValueCollection)ConfigurationManager.GetSection("en-US");
            NameValueCollection defaultLanguage = (NameValueCollection)ConfigurationManager.GetSection(Thread.CurrentThread.CurrentCulture.Name);
            if (defaultLanguage == null)
            {
                defaultLanguage = englishLanguage;
                if (defaultLanguage == null)
                {
                    eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.ErrorLanguageSectionNotFound, Application.ExecutablePath);
                    eventArgs.DisplayColor = Color.Red;
                    eventArgs.StatusMsg = Properties.Resources.ErrorApplicationConfiguration;
                    updateStatus(eventArgs);
                    return;
                }
            }

            try
            {
                int totalRowsCount = Convert.ToInt32(Regex.Match(bcpArgs, defaultLanguage["BCPNumber"]).Value, CultureInfo.InvariantCulture);

                // bcpArgs example: ":128425:[dbo].[Categories] in "C:\Users\ghuey\AppData\Local\Temp\tmp1CD8.tmp" -n -E

                // schema_table[0] == schema
                // schema_table[1] == table

                string[] schema_table = CommonFunc.GetSchemaAndTableFromBCPArgs(bcpArgs);
                string args = CommonFunc.BuildBCPUploadCommand(targetServer, bcpArgs);

                QueueBCPJobs(updateStatus, queueBCPJob, schema_table[0], schema_table[1], totalRowsCount, args, targetServer.ConnectionStringTargetDatabase);
            }
            catch (Exception ex)
            {
                eventArgs.DisplayText = ex.ToString();
                eventArgs.DisplayColor = Color.Red;
                eventArgs.StatusMsg = Properties.Resources.ErrorParsingBCPArgs;
                updateStatus(eventArgs);
            }
        }

        public void ExecuteSQLonTarget(TargetServerInfo targetServer, AsyncUpdateStatus updateStatus, AsyncQueueBCPJob queueBCPJob, string sqlToExecute)
        {
            DateTime dtStart = DateTime.Now;
            AsyncNotificationEventArgs eventArgs = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ExecuteSqlOnAzure, 0, "", CommonFunc.FormatString(Properties.Resources.ProcessStarted, dtStart.ToString(), dtStart.ToUniversalTime().ToString()) + Environment.NewLine, Color.DarkSlateBlue);
            StringCollection strColl = new StringCollection();
            StringCollection bcpUploadCommands = new StringCollection();
            bool inBCPCommand = false;
            int idx = 0;

            AsyncProcessingStatus.FinishedProcessingJobs = true;

            updateStatus(eventArgs);

            string connectionStr = targetServer.ConnectionStringTargetDatabase;

            CommentAreaHelper cah = new CommentAreaHelper();
            try
            {
                cah.FindCommentAreas(sqlToExecute);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            long lCharCnt = 0;
            int sqlCmdLoopCtr = 0;

            AsyncProcessingStatus.NumberOfCommands = 0;
            AsyncProcessingStatus.NumberOfCommandsExecuted = 0;

            if (cah.Lines.Count() > 0)
            {
                foreach (string line in cah.Lines)
                {
                    if (line.StartsWith(Properties.Resources.Go) && !cah.IsIndexInComments(lCharCnt)) AsyncProcessingStatus.NumberOfCommands++;
                    lCharCnt += line.Length + cah.CrLf;
                }
            }

            using (SqlConnection con = new SqlConnection(connectionStr))
            {
                StringBuilder sql = new StringBuilder(5000);
                StringCollection sqlDepends = new StringCollection();
                string currentObject = "";
                bool comment = false;

                try
                {
                    Retry.ExecuteRetryAction(() =>
                    {
                        con.Open();
                    }, () =>
                    {
                        con.Close();
                    });
                }
                catch (Exception ex)
                {
                    eventArgs.DisplayColor = Color.Red;
                    eventArgs.PercentComplete = 100;
                    if (ex is SqlException)
                    {
                        eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.ProcessAborting, ((SqlException)ex).Number.ToString(), ex.Message, sql.ToString(), DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                    }
                    else
                    {
                        eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.ProcessAborting, "", ex.Message, sql.ToString(), DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                    }
                    updateStatus(eventArgs);
                    return;
                }

                lCharCnt = 0;
                foreach (string sqlCmd in cah.Lines)
                {
                    ++sqlCmdLoopCtr;
                    if (CancelProcessing)
                    {
                        con.Close();
                        eventArgs.StatusMsg = Properties.Resources.Canceled;
                        eventArgs.DisplayColor = Color.DarkCyan;
                        eventArgs.DisplayText = Environment.NewLine + CommonFunc.FormatString(Properties.Resources.ProcessCanceledAt, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                        eventArgs.PercentComplete = 100;
                        updateStatus(eventArgs);
                        return;
                    }

                    if (inBCPCommand)
                    {
                        if (sqlCmd.Length == 0)
                        {
                            ++lCharCnt;
                            continue;  // Get rid of blank line when in BCP Command
                        }
                        if (sqlCmd.StartsWith(Properties.Resources.Go) && !cah.IsIndexInComments(lCharCnt))
                        {
                            sql.Remove(0, sql.Length);
                            inBCPCommand = false;
                            lCharCnt += sqlCmd.Length + cah.CrLf;
                            continue;
                        }
                    }

                    if (sqlCmd.StartsWith(CommonFunc.FormatString(Properties.Resources.RemoveComment)))
                    {
                        lCharCnt += sqlCmd.Length + cah.CrLf;
                        continue;  // Get rid of program generated comments
                    }

                    if (!comment)
                    {
                        idx = sqlCmd.IndexOf(CommonFunc.FormatString(Properties.Resources.RemoveCommentStart));
                        if (idx > -1)
                        {
                            comment = true;
                            lCharCnt += sqlCmd.Length + cah.CrLf;
                            continue;
                        }
                    }
                    else
                    {
                        idx = sqlCmd.IndexOf(CommonFunc.FormatString(Properties.Resources.RemoveCommentEnd));
                        if (idx > -1)
                        {
                            comment = false;
                            lCharCnt += sqlCmd.Length + cah.CrLf;
                            continue;
                        }
                        lCharCnt += sqlCmd.Length + cah.CrLf;
                        continue;
                    }

                    // Look for BCP string.  I.E. "-- BCPArgs:2345:.dbo.Categories" in "C:\Users\ghuey\AppData\Local\Temp\tmp1CD8.tmp" -n -E 
                    if (sqlCmd.StartsWith("-- BCPArgs:", StringComparison.Ordinal))
                    {
                        BCPUploadData(targetServer, updateStatus, queueBCPJob, sqlCmd.Substring(11), ref bcpUploadCommands);

                        // if queueBCPJob is null, then BCP upload is not queued up for a parallel batch process (basically, it is finished by now).
                        if (queueBCPJob == null)
                        {
                            ++AsyncProcessingStatus.NumberOfCommandsExecuted;
                            eventArgs.DisplayText = "";
                            eventArgs.PercentComplete = (int)(((float)AsyncProcessingStatus.NumberOfCommandsExecuted / (float)AsyncProcessingStatus.NumberOfCommands) * 100.0);
                            eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.BCPProcessingStatus, AsyncProcessingStatus.NumberOfCommandsExecuted.ToString(), AsyncProcessingStatus.NumberOfCommands.ToString());
                            updateStatus(eventArgs);
                        }
                        inBCPCommand = true;
                        lCharCnt += sqlCmd.Length + cah.CrLf;
                        continue;
                    }

                    if (sqlCmd.StartsWith(CommonFunc.FormatString(Properties.Resources.Go)) && !cah.IsIndexInComments(lCharCnt) || sqlCmdLoopCtr == cah.Lines.Count())
                    {
                        if (sql.Length == 0)
                        {
                            lCharCnt += sqlCmd.Length + cah.CrLf;
                            continue;
                        }

                        try
                        {
                            Retry.ExecuteRetryAction(() =>
                            {
                                NonQueryResults nqr = SqlHelper.ExecuteNonQuery(con, CommandType.Text, sql.ToString());
                            }, () =>
                            {
                                ResetConnection(con);
                            });

                            Match cmdStr = Regex.Match(sql.ToString(), "(CREATE|ALTER)[\\w\\W]+(INDEX|TABLE|VIEW|PROCEDURE|ROLE|SCHEMA|TRIGGER|TYPE).*", RegexOptions.IgnoreCase);
                            if (cmdStr.Success)
                            {
                                int cr = cmdStr.Value.IndexOf("\r");
                                if (cr > 0)
                                {
                                    currentObject = cmdStr.Value.Substring(0, cr > 70 ? 70 : cr);
                                }
                                else
                                {
                                    currentObject = cmdStr.Value.Substring(0, cmdStr.Value.Length > 70 ? 70 : cmdStr.Value.Length);
                                }
                            }
                            else
                            {
                                currentObject = sql.ToString().Substring(0, sql.ToString().Length > 70 ? 70 : sql.ToString().Length);
                            }
                            currentObject = currentObject.Replace("\r", "").Replace("\n", " ");

                            eventArgs.DisplayColor = Color.DarkGreen;
                            eventArgs.DisplayText = Properties.Resources.Success + " " + currentObject + Environment.NewLine;
                        }
                        catch (Exception ex)
                        {
                            eventArgs.DisplayColor = Color.Red;
                            if (ex is SqlException)
                            {
                                if (((SqlException)ex).Number == 208)
                                {
                                    --AsyncProcessingStatus.NumberOfCommandsExecuted;
                                    sqlDepends.Add(sql.ToString());
                                    eventArgs.DisplayText = "";
                                }
                                else
                                {
                                    eventArgs.DisplayText = DateTime.Now.ToString(CultureInfo.CurrentCulture) + CommonFunc.FormatString(Properties.Resources.ErrorNumAndMsg, ((SqlException)ex).Number.ToString(), ex.Message) + Environment.NewLine + sql.ToString() + Environment.NewLine;
                                }
                            }
                            else
                            {
                                eventArgs.DisplayText = DateTime.Now.ToString(CultureInfo.CurrentCulture) + CommonFunc.FormatString(Properties.Resources.ErrorNumAndMsg, "", ex.Message) + Environment.NewLine + sql.ToString() + Environment.NewLine;
                            }
                        }

                        ++AsyncProcessingStatus.NumberOfCommandsExecuted;
                        eventArgs.PercentComplete = (int)(((float)AsyncProcessingStatus.NumberOfCommandsExecuted / (float)AsyncProcessingStatus.NumberOfCommands) * 100.0);
                        eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.BCPProcessingStatus, AsyncProcessingStatus.NumberOfCommandsExecuted.ToString(), AsyncProcessingStatus.NumberOfCommands.ToString());
                        updateStatus(eventArgs);
                        sql.Remove(0, sql.Length);
                    }
                    else
                    {
                        sql.AppendLine(sqlCmd);
                    }
                    lCharCnt += sqlCmd.Length + cah.CrLf;
                }

                // Ok, check for error that happened because of dependency and retry

                foreach (string sqlDep in sqlDepends)
                {
                    try
                    {
                        Retry.ExecuteRetryAction(() =>
                        {
                            NonQueryResults nqr = SqlHelper.ExecuteNonQuery(con, CommandType.Text, sqlDep);
                        },
                        () =>
                        {
                            ResetConnection(con);
                        });

                        int startIdx = sqlDep.IndexOf("CREATE ", 0, StringComparison.CurrentCultureIgnoreCase);
                        if (startIdx < 0)
                        {
                            startIdx = sqlDep.IndexOf("ALTER ", 0, StringComparison.CurrentCultureIgnoreCase);
                        }
                        int len = sqlDep.Substring(startIdx).Length > 70 ? 70 : sqlDep.Substring(startIdx).Length;
                        currentObject = sqlDep.Substring(startIdx, len) + " ...";

                        eventArgs.DisplayColor = Color.DarkGreen;
                        eventArgs.DisplayText = Properties.Resources.Success + " " + currentObject.Replace("\r", "").Replace("\n", " ") + Environment.NewLine;
                    }
                    catch (Exception ex)
                    {
                        eventArgs.DisplayColor = Color.Red;
                        if (ex is SqlException)
                        {
                            eventArgs.DisplayText = DateTime.Now.ToString(CultureInfo.CurrentCulture) + CommonFunc.FormatString(Properties.Resources.ErrorNumAndMsg, ((SqlException)ex).Number.ToString(), ex.Message) + Environment.NewLine + sql.ToString() + Environment.NewLine;
                        }
                        else
                        {
                            eventArgs.DisplayText = DateTime.Now.ToString(CultureInfo.CurrentCulture) + CommonFunc.FormatString(Properties.Resources.ErrorNumAndMsg, "", ex.Message) + Environment.NewLine + sql.ToString() + Environment.NewLine;
                        }
                    }

                    ++AsyncProcessingStatus.NumberOfCommandsExecuted;
                    eventArgs.PercentComplete = (int)(((float)AsyncProcessingStatus.NumberOfCommandsExecuted / (float)AsyncProcessingStatus.NumberOfCommands) * 100.0);
                    eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.BCPProcessingStatus, AsyncProcessingStatus.NumberOfCommandsExecuted.ToString(), AsyncProcessingStatus.NumberOfCommands.ToString());
                    updateStatus(eventArgs);
                }
                con.Close();

                // Output BCP upload command summary
                if (bcpUploadCommands.Count > 0)
                {
                    eventArgs.DisplayColor = Color.Green;
                    eventArgs.DisplayText = Properties.Resources.BCPUploadSummary + Environment.NewLine;
                    eventArgs.PercentComplete = 99;
                    updateStatus(eventArgs);

                    foreach (string bcpUploadCommand in bcpUploadCommands)
                    {
                        eventArgs.DisplayText = bcpUploadCommand + Environment.NewLine;
                        updateStatus(eventArgs);
                    }
                }

                AsyncProcessingStatus.FinishedAddingJobs = true;

                while (true)
                {
                    if (AsyncProcessingStatus.FinishedProcessingJobs) break;
                    Thread.Sleep(500);
                }

                // Done

                DateTime dtEnd = DateTime.Now;
                TimeSpan tsDuration = dtEnd.Subtract(dtStart);
                string sHour = tsDuration.Hours == 1 ? Properties.Resources.TimeHour : Properties.Resources.TimeHours;
                string sMin = tsDuration.Minutes == 1 ? Properties.Resources.TimeMinute : Properties.Resources.TimeMinutes;
                string sSecs = tsDuration.Seconds == 1 ? Properties.Resources.TimeSecond : Properties.Resources.TimeSeconds;

                eventArgs.StatusMsg = Properties.Resources.Done;
                eventArgs.DisplayColor = Color.DarkCyan;
                eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.ProcessingFinished, dtEnd.ToString(), dtEnd.ToUniversalTime().ToString(), tsDuration.Hours + sHour + tsDuration.Minutes.ToString() + sMin + tsDuration.Seconds.ToString() + sSecs);
                eventArgs.PercentComplete = 100;
                updateStatus(eventArgs);
            }
        }
    }
}
