using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace SQLAzureMWUtils
{
    public class TsqlFileMigrator
    {
        private IMigrationOutput _Output = null;

        public TsqlFileMigrator(IMigrationOutput output)
        {
            this._Output = output;
        }

        public void ParseFile(string _FileToProcess, bool _ParseFile)
        {
            DateTime startTime = DateTime.Now;
            AsyncNotificationEventArgs e = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ParseFile, 0, "", "", Color.Black);

            ScriptDatabase sdb = new ScriptDatabase();
            sdb.Initialize(_Output.StatusUpdateHandler, SMOScriptOptions.CreateFromConfig(), false);

            /****************************************************************/

            string sqlText = CommonFunc.GetTextFromFile(_FileToProcess);
            CommentAreaHelper cah = new CommentAreaHelper();
            long totalCharacterOffset = 0;
            bool bCommentedLine = false;

            List<string> sqlCmds = new List<string>();
            if (_ParseFile)
            {
                StringBuilder sb = new StringBuilder();
                cah.FindCommentAreas(sqlText);
                foreach (string line in cah.Lines)
                {
                    if (line.Equals(Properties.Resources.Go, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!cah.IsIndexInComments(totalCharacterOffset))
                        {
                            sqlCmds.Add(sb.ToString());
                            sb.Length = 0;
                        }
                        else
                        {
                            sb.Append(line + Environment.NewLine);
                        }
                    }
                    else
                    {
                        sb.Append(line + Environment.NewLine);
                    }
                    totalCharacterOffset += line.Length + cah.CrLf;
                }
            }
            else
            {
                sqlCmds.Add(sqlText);
            }

            int numCmds = sqlCmds.Count();

            if (numCmds == 0)
            {
                e.PercentComplete = 100;
                e.DisplayText = "No data to process";
                _Output.StatusUpdateHandler(e);
                return;
            }

            int loopCtr = 0;

            e.DisplayColor = Color.DarkBlue;
            e.DisplayText = "";
            e.StatusMsg = "Processing " + loopCtr.ToString() + " out of " + numCmds.ToString();
            e.PercentComplete = 0;
            _Output.StatusUpdateHandler(e);
            totalCharacterOffset = 0;

            foreach (string cmd in sqlCmds)
            {
                ++loopCtr;

                if (AsyncProcessingStatus.CancelProcessing) break;
                if (cmd.Length == 0 || cmd.Equals(Environment.NewLine)) continue;

                foreach (CommentArea ca in cah.CommentAreas)
                {
                    if (ca.Start == totalCharacterOffset && ca.End == totalCharacterOffset + cmd.Length - cah.CrLf - 1) // note that the -1 is to put you at zero based counting
                    {
                        bCommentedLine = true;
                        break;
                    }
                    bCommentedLine = false;
                }
                totalCharacterOffset += cmd.Length + cah.CrLf;

                if (_ParseFile && !bCommentedLine && !(cmd.StartsWith("/*~") || cmd.StartsWith("~*/")))
                {
                    if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sPROCEDURE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileTSQLGo(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "(CREATE|ALTER)\\sTABLE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileTable(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "CREATE\\sXML\\sSCHEMA\\sCOLLECTION", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileXMLSchemaCollections(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "CREATE\\sTYPE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileUDT(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "CREATE\\s[a-z\\s]*\\sINDEX", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileIndex(cmd);
                    }
                    else if (Regex.IsMatch(cmd, "CREATE ROLE", RegexOptions.IgnoreCase))
                    {
                        sdb.ParseFileRole(cmd);
                    }
                    else
                    {
                        sdb.ParseFileTSQLGo(cmd);
                    }
                }
                else
                {
                    sdb.OutputSQLString(cmd, Color.Black);
                }

                if (loopCtr % 20 == 0)
                {
                    e.PercentComplete = (int)(((float)loopCtr / (float)numCmds) * 100.0);
                    e.DisplayColor = Color.DarkBlue;
                    e.DisplayText = "";
                    e.StatusMsg = "Processing " + loopCtr.ToString() + " out of " + numCmds.ToString();
                    _Output.StatusUpdateHandler(e);
                }
            }

            if (AsyncProcessingStatus.CancelProcessing)
            {
                e.DisplayText = "Canceled processing ..." + Environment.NewLine;
                e.StatusMsg = "Canceled!";
            }
            else
            {
                e.DisplayColor = Color.DarkCyan;
                if (_ParseFile)
                {
                    e.DisplayText = Properties.Resources.AnalysisComplete + Environment.NewLine;
                }
                else
                {
                    e.DisplayText = "File read and ready for processing.";
                }
                e.StatusMsg = "Done!";
            }
            e.PercentComplete = 100;
            _Output.StatusUpdateHandler(e);

            DateTime endTime = DateTime.Now;
            e.DisplayText = string.Format(
                "{1}Total processing time --> {0}",
                endTime.Subtract(startTime).ToString(),
                Properties.Resources.RemoveComment);
            _Output.StatusUpdateHandler(e);
        }
    }
}