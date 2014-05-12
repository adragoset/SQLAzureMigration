using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using Microsoft.SqlServer.Management.Trace;
using SQLAzureMWUtils;
using System.Globalization;
using System.Collections;

namespace SQLAzureMW
{
    /// <summary>
    /// This is the SQL Azure Migration Wizard.
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

    public partial class ScriptWizard : Form, IMigrationOutput
    {
        private StringBuilder rtbSQLScriptBuffer = new StringBuilder(5000);
        private Color rtbSQLScriptCurrentColor = Color.White;
        private SMOScriptOptions _smoScriptOpts;
        private Database _sourceDatabase;
        private TabPage _sqlResultTab;
        private Server _SourceServer;
        private FederationMemberDistribution _memberDistribution;
        private FederationDetails _federationDetails;
        private TargetServerInfo _TargetServerInfo;
        private TargetServerInfo _SourceServerInfo;
        private string _FileToProcess;
        private string _sqlForAzure;
        private bool _ParseFile;
        private int[] _crumb;
        private int _wizardIndex;
        private int _crumbIdx;
        private bool _Reset;
        private bool _IgnorCheck;
        private System.Threading.Thread _runningThread;

        private string[] _CurrentWizardAction = new string[7];
        private string[] _CurrentWizardActDes = new string[7];

        public ScriptWizard()
        {
            //string culture = "af-ZA";
            //string culture = "de-DE";
            //string culture = "ja-JP";
            //string culture = "fr-FR";
            //string culture = "nl-nl";
            //string culture = "es-ES";
            //string culture = "pt-BR";
            //string culture = "pt-PT";
            //string culture = "zh-CN";
            //string culture = "zh-TW";

            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

            _CurrentWizardAction[0] = CommonFunc.FormatString(Properties.Resources.WizardAction);
            _CurrentWizardAction[1] = CommonFunc.FormatString(Properties.Resources.WizardActionSelectSource);
            _CurrentWizardAction[2] = CommonFunc.FormatString(Properties.Resources.WizardActionObjectTypes);
            _CurrentWizardAction[3] = CommonFunc.FormatString(Properties.Resources.WizardActionScriptWizardSummary);
            _CurrentWizardAction[4] = CommonFunc.FormatString(Properties.Resources.WizardActionResultsSummary);
            _CurrentWizardAction[5] = CommonFunc.FormatString(Properties.Resources.WizardActionSetupTargetConnection);
            _CurrentWizardAction[6] = CommonFunc.FormatString(Properties.Resources.WizardActionTargetResults);

            _CurrentWizardActDes[0] = CommonFunc.FormatString(Properties.Resources.WizardActionDesc);
            _CurrentWizardActDes[1] = CommonFunc.FormatString(Properties.Resources.WizardActionSelectSourceDesc);
            _CurrentWizardActDes[2] = CommonFunc.FormatString(Properties.Resources.WizardActionObjectTypesDesc);
            _CurrentWizardActDes[3] = CommonFunc.FormatString(Properties.Resources.WizardActionScriptWizardSummaryDesc);
            _CurrentWizardActDes[4] = CommonFunc.FormatString(Properties.Resources.WizardActionResultsSummaryDesc);
            _CurrentWizardActDes[5] = CommonFunc.FormatString(Properties.Resources.WizardActionSetupTargetConnectionDesc);
            _CurrentWizardActDes[6] = CommonFunc.FormatString(Properties.Resources.WizardActionTargetResultsDesc);

            InitializeComponent();
            InitializeAll();
            DisplayNext();
            Initialize_BCPJobs();

            _smoScriptOpts = SMOScriptOptions.CreateFromConfig();
        }

        private void ScriptWizard_Load(object sender, EventArgs e)
        {
            this.ClientSize = new System.Drawing.Size(600, 630);
            this.Show();
            Application.DoEvents();
        }

        private void CancelAsyncProcesses()
        {
            AsyncProcessingStatus.FinishedProcessingJobs = true;

            if (_ThreadManager != null)
            {
                _ThreadManager.Abort();
                _ThreadManager = null;
            }

            if (_runningThread != null)
            {
                _runningThread.Abort();
                _runningThread = null;
            }
        }

        private void StartThread(ThreadStart ts)
        {
            if (_runningThread != null)
            {
                _runningThread.Abort();
            }

            _runningThread = new System.Threading.Thread(ts);
            _runningThread.CurrentCulture = CultureInfo.CurrentCulture;
            _runningThread.CurrentUICulture = CultureInfo.CurrentUICulture;
            _runningThread.Start();
        }

        public void rtbSQLScriptAppendText(string text, Color textColor)
        {
            if (rtbSQLScriptCurrentColor == Color.White)
            {
                rtbSQLScriptCurrentColor = textColor;
            }

            if (rtbSQLScriptCurrentColor == textColor)
            {
                rtbSQLScriptBuffer.Append(text);
            }
            else
            {
                AppendText(rtbSQLScript, rtbSQLScriptBuffer.ToString(), rtbSQLScriptCurrentColor);
                rtbSQLScriptBuffer.Length = 0;
                rtbSQLScriptCurrentColor = textColor;
                rtbSQLScriptBuffer.Append(text);
            }
        }

        private void AppendText(RichTextBox rtb, string text, Color selectionColor)
        {
            rtb.SuspendLayout();
            rtb.SelectionColor = selectionColor;
            rtb.AppendText(text);
            if (cbResultsScroll.Checked)
            {
                rtb.ScrollToCaret();
            }
            rtb.ResumeLayout();
        }

        public void StatusUpdateHandler(AsyncNotificationEventArgs args)
        {
            if (this.InvokeRequired)
            {
                AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(StatusUpdateHandler);
                this.Invoke(updateStatus, new object[] { args });
            }
            else
            {
                if (args.FunctionCode == NotificationEventFunctionCode.SqlOutput)
                {
                    rtbSQLScriptAppendText(args.DisplayText, args.DisplayColor);
                }
                else if (args.FunctionCode == NotificationEventFunctionCode.AnalysisOutput)
                {
                    AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                }
                else
                {
                    progressBarGenScript.Value = args.PercentComplete;
                    lbResultsSummaryStatus.Text = args.StatusMsg;

                    if (args.DisplayText.Length > 0)
                    {
                        AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                    }
                }

                if (args.PercentComplete == 100)
                {
                    rtbSQLScriptAppendText("", Color.White); // Flush rtbSQLScript buffer
                    rtbResultsSummary.ScrollToCaret();
                    btnSave.Enabled = true;
                    btnBack.Enabled = true;
                    if (AsyncProcessingStatus.CancelProcessing || !_smoScriptOpts.Migrate)
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                }
            }
        }

        private void StartFileParseAsyncProcess(string file, bool parse)
        {
            System.Threading.ThreadStart ts = null;
            rtbSQLScript.Clear();
            rtbSQLScriptCurrentColor = Color.White;
            rtbSQLScriptBuffer.Length = 0;

            rtbResultsSummary.Clear();

            btnBack.Enabled = false;
            btnSave.Enabled = false;
            progressBarGenScript.Value = 0;
            progressBarGenScript.Visible = true;
            lbResultsSummaryStatus.Text = "";
            lbResultsSummaryStatus.Visible = true;
            _FileToProcess = file;

            if (rbAnalyzeTraceFile.Checked)
            {
                tabCtlResults.TabPages.Remove(_sqlResultTab);
                ts = new System.Threading.ThreadStart(ParseTraceFile);
            }
            else
            {
                if (tabCtlResults.TabPages.Count == 1)
                {
                    tabCtlResults.TabPages.Add(_sqlResultTab);
                }

                _ParseFile = parse;

                ts = new System.Threading.ThreadStart(RunFileParsingOnNewThread);
            }
            StartThread(ts);
        }

        private void RunFileParsingOnNewThread()
        {
            var migrator = new TsqlFileMigrator(this);
            migrator.ParseFile(_FileToProcess, _ParseFile);
        }

        private void GenScriptAsyncUpdateStatusHandler(AsyncNotificationEventArgs args)
        {
            if (this.InvokeRequired)
            {
                AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(GenScriptAsyncUpdateStatusHandler);
                this.Invoke(updateStatus, new object[] { args });
            }
            else
            {
                if (args.FunctionCode == NotificationEventFunctionCode.AnalysisOutput)
                {
                    AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                }
                else if (args.FunctionCode == NotificationEventFunctionCode.SqlOutput)
                {
                    rtbSQLScriptAppendText(args.DisplayText, args.DisplayColor);
                }
                else
                {
                    progressBarGenScript.Value = args.PercentComplete;
                    lbResultsSummaryStatus.Text = args.StatusMsg;

                    if (args.DisplayText.Length > 0)
                    {
                        AppendText(rtbResultsSummary, args.DisplayText, args.DisplayColor);
                    }

                    if (args.PercentComplete == 100)
                    {
                        rtbSQLScriptAppendText("", Color.White); // Flush rtbSQLScript buffer
                        rtbResultsSummary.ScrollToCaret();
                        if (_smoScriptOpts.Migrate)
                        {
                            btnNext.Enabled = true;
                        }
                        btnBack.Enabled = true;
                        btnSave.Enabled = true;
                    }
                }
            }
        }

        private Database GetSourceDatabaseFromListBox()
        {
            DatabaseInfo di = (DatabaseInfo)lbDatabases.SelectedItem;
            Database db = null;

            if (di.ConnectedTo == TypeOfConnection.SQLAzureFederation)
            {
                if (di.FederationMember.DatabaseName == null)
                {
                    di.FederationMember.DatabaseName = FederationCommonFuncs.GetFederationMemberDatabaseName(((FederationDetails)lbSourceFederations.SelectedItem).FederationName, di.FederationMember, _SourceServerInfo.ConnectionStringRootDatabase);
                }
                db = _SourceServer.Databases[di.FederationMember.DatabaseName];
            }
            else
            {
                db = di.DatabaseObject;
            }
            return db;
        }

        private void GetSelectedFederationInfo()
        {
            DatabaseInfo di = (DatabaseInfo)lbDatabases.SelectedItem;

            if (di.ConnectedTo == TypeOfConnection.SQLAzureFederation)
            {
                _federationDetails = (FederationDetails)lbSourceFederations.SelectedItem;
                _memberDistribution = di.FederationMember;
            }
            else
            {
                _federationDetails = null;
                _memberDistribution = null;
            }
        }

        private void StartGenScriptAsyncProcess()
        {
            AsyncProcessingStatus.CancelProcessing = false;
            GetSelectedFederationInfo();
            _sourceDatabase = GetSourceDatabaseFromListBox();
            rtbResultsSummary.Clear();
            rtbSQLScript.Clear();
            rtbSQLScriptBuffer.Length = 0;
            rtbSQLScriptCurrentColor = Color.White;
            btnBack.Enabled = false;
            btnSave.Enabled = false;
            progressBarGenScript.Value = 0;
            progressBarGenScript.Visible = true;
            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(GenerateScriptFromSourceServer);
            StartThread(ts);
        }

        private void TargetAsyncUpdateStatusHandler(AsyncNotificationEventArgs e)
        {
            if (this.InvokeRequired)
            {
                AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(TargetAsyncUpdateStatusHandler);
                this.Invoke(updateStatus, new object[] { e });
            }
            else
            {
                Color selectionColor = e.DisplayColor;
                if (e.FunctionCode == NotificationEventFunctionCode.BcpUploadData)
                {
                    if (e.PercentComplete == 0)
                    {
                        rtbTargetResults.AppendText(Environment.NewLine);
                    }
                    else if (e.PercentComplete != 100)
                    {
                        if (rtbTargetResults.SelectionColor == Color.DarkBlue)
                        {
                            selectionColor = Color.DarkRed;
                        }
                        else
                        {
                            selectionColor = Color.DarkBlue;
                        }
                    }
                }
                else if (e.FunctionCode == NotificationEventFunctionCode.ExecuteSqlOnAzure)
                {
                    progressBarTargetServer.Value = e.PercentComplete > 100 ? 100 : e.PercentComplete;
                    lbStatus.Text = e.StatusMsg;

                    if (e.PercentComplete >= 100)
                    {
                        btnSaveTargetResults.Enabled = true;
                        btnBack.Enabled = true;
                    }
                }

                rtbTargetResults.SelectionColor = selectionColor;
                rtbTargetResults.AppendText(e.DisplayText);
                if (cbAzureStatusScroll.Checked)
                {
                    rtbTargetResults.ScrollToCaret();
                }
            }
        }

        private void StartFederationTargetAsyncProcess()
        {
            rtbTargetResults.Clear();
            rtbTargetResults.SuspendLayout();

            FederationDetails fd = (FederationDetails)lbFederations.SelectedItem;
            FederationMemberDistribution member = (FederationMemberDistribution)lbFederationMembers.SelectedItem;
            if (member.DatabaseName == null)
            {
                string use = FederationCommonFuncs.GetUseFederation(fd.FederationName, ref member);

                Retry.ExecuteRetryAction(() =>
                    {
                        using (SqlConnection connection = new SqlConnection(_TargetServerInfo.ConnectionStringRootDatabase))
                        {
                            connection.Open();
                            SqlHelper.ExecuteNonQuery(connection, CommandType.Text, use);
                            member.DatabaseName = (string)SqlHelper.ExecuteScalar(connection, CommandType.Text, "SELECT DB_NAME()").ExecuteScalarReturnValue;
                        }
                    });
            }

            _TargetServerInfo.TargetDatabase = member.DatabaseName;
            _sqlForAzure = rtbSQLScript.Text;

            btnBack.Enabled = false;
            progressBarTargetServer.Value = 0;
            lbStatus.Text = "";
            lbStatus.Visible = true;

            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(ExecuteSQLonTarget);
            StartThread(ts);
        }

        private void StartTargetAsyncProcess()
        {
            rtbTargetResults.Clear();
            rtbTargetResults.SuspendLayout();
            _TargetServerInfo.TargetDatabase = lbTargetDatabases.SelectedItem.ToString().Replace("[", "").Replace("]", "");

            _sqlForAzure = rtbSQLScript.Text;

            btnBack.Enabled = false;
            progressBarTargetServer.Value = 0;
            lbStatus.Text = "";
            lbStatus.Visible = true;

            System.Threading.ThreadStart ts = new System.Threading.ThreadStart( ExecuteSQLonTarget );
            StartThread(ts);
        }

        private void InitializeAll()
        {
            lbProgram.Text = Application.ProductName + " v" + Application.ProductVersion.Substring(0, Application.ProductVersion.Length);
            if (lbProgram.Text.EndsWith(".0", StringComparison.Ordinal))
            {
                lbProgram.Text = lbProgram.Text.Substring(0, lbProgram.Text.Length - 2);
            }

            if (lbProgram.Text.EndsWith(".0", StringComparison.Ordinal))
            {
                lbProgram.Text = lbProgram.Text.Substring(0, lbProgram.Text.Length - 2);
            }

            _wizardIndex = WizardSteps.SelectProcess;
            _crumbIdx = WizardSteps.SelectProcess;
            _Reset = true;
            _crumb = new int[_CurrentWizardAction.Length];

            splitDBDisplay.Panel1Collapsed = true;
            gbSelectDatabase.Text = Properties.Resources.SelectDatabase;

            foreach (Control con in panel3.Controls)
            {
                con.Dock = DockStyle.Fill;
            }

            //tvDatabaseObjects.StateImageList = new ImageList();
            //tvDatabaseObjects.StateImageList.Images.Add(SystemIcons.Asterisk);
            //tvDatabaseObjects.StateImageList.Images.Add(SystemIcons.Exclamation);
            //tvDatabaseObjects.StateImageList.Images.Add(SystemIcons.Question);

            lbDatabases.DrawMode = DrawMode.OwnerDrawFixed;
            lbDatabases.DrawItem += new System.Windows.Forms.DrawItemEventHandler(lbDatabases_DrawItem);

            btnNext.Enabled = false;
            btnSave.Enabled = false;

            _sqlResultTab = tabCtlResults.TabPages[1];
            HideAll();
        }

        private void HideAll()
        {
            foreach (Control con in panel3.Controls)
            {
                con.Visible = false;
            }
        }

        private TreeNode GetEncryptedNode(string nodeText)
        {
            TreeNode tn = new TreeNode(CommonFunc.FormatString(Properties.Resources.Encrypted, nodeText));
            tn.ForeColor = Color.Red;
            tn.BackColor = Color.Yellow;
            return tn;
        }

        private void FigureOutObjectTypesAvailable()
        {
            ResetFields(panel3);
            _Reset = false;

            tvDatabaseObjects.SuspendLayout();
            tvDatabaseObjects.Nodes.Clear();

            DatabaseInfo di = (DatabaseInfo) lbDatabases.SelectedItem;
            Database db = GetSourceDatabaseFromListBox();

            try
            {
                // Database Triggers
                if (db.Triggers.Count > 0)
                {
                    TreeNode tnTriggers = null;
                    foreach (DatabaseDdlTrigger trig in db.Triggers)
                    {
                        if (!trig.IsSystemObject)
                        {
                            if (tnTriggers == null)
                            {
                                tnTriggers = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeTriggers);
                            }

                            if (trig.IsEncrypted)
                            {
                                tnTriggers.Nodes.Add(GetEncryptedNode(trig.ToString()));
                            }
                            else
                            {
                                tnTriggers.Nodes.Add(trig.ToString());
                            }
                        }
                    }
                }

                // Roles
                if (db.Roles.Count > 0)
                {
                    TreeNode tnRoles = null;
                    foreach (DatabaseRole role in db.Roles)
                    {
                        switch (role.Name)
                        {
                            case "db_accessadmin":
                            case "db_backupoperator":
                            case "db_datareader":
                            case "db_datawriter":
                            case "db_ddladmin":
                            case "db_denydatareader":
                            case "db_denydatawriter":
                            case "db_owner":
                            case "db_securityadmin":
                            case "public":
                                break;

                            default:
                                if (tnRoles == null)
                                {
                                    tnRoles = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeRoles);
                                }
                                tnRoles.Nodes.Add(role.ToString());
                                break;
                        }
                    }
                }

                // Schemas
                if (db.Schemas.Count > 0)
                {
                    TreeNode tnSchemas = null;
                    foreach (Schema sch in db.Schemas)
                    {
                        switch (sch.Name)
                        {
                            case "sys":
                            case "INFORMATION_SCHEMA":
                            case "guest":
                            case "dbo":
                            case "db_securityadmin":
                            case "db_owner":
                            case "db_denydatawriter":
                            case "db_denydatareader":
                            case "db_ddladmin":
                            case "db_datawriter":
                            case "db_datareader":
                            case "db_backupoperator":
                            case "db_accessadmin":
                                break;

                            default:
                                if (tnSchemas == null)
                                {
                                    tnSchemas = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeSchemas);
                                }
                                tnSchemas.Nodes.Add(sch.ToString());
                                break;
                        }
                    }
                }

                try
                {
                    if (db.UserDefinedTableTypes.Count > 0)
                    {
                        TreeNode tnTableTypes = null;
                        foreach (UserDefinedTableType tt in db.UserDefinedTableTypes)
                        {
                            if (tnTableTypes == null)
                            {
                                tnTableTypes = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeUDTT);
                            }
                            tnTableTypes.Nodes.Add(tt.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                // Stored Procedures
                if (db.StoredProcedures.Count > 0)
                {
                    TreeNode tnSP = null;
                    foreach (StoredProcedure sp in db.StoredProcedures)
                    {
                        if (!sp.IsSystemObject)
                        {
                            if (tnSP == null)
                            {
                                tnSP = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeStoredProcedures);
                            }

                            if (sp.IsEncrypted)
                            {
                                tnSP.Nodes.Add(GetEncryptedNode(sp.ToString()));
                            }
                            else
                            {
                                tnSP.Nodes.Add(sp.ToString());
                            }
                        }
                    }
                }

                // Tables
                if (db.Tables.Count > 0)
                {
                    TreeNode tnTables = null;
                    foreach (Table tb in db.Tables)
                    {
                        if (!tb.IsSystemObject)
                        {
                            if (tnTables == null)
                            {
                                tnTables = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeTables);
                            }
                            tnTables.Nodes.Add(tb.ToString());
                        }
                    }
                }

                try
                {
                    // UDT
                    if (db.UserDefinedDataTypes.Count > 0)
                    {
                        TreeNode tnUDT = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeUDT);
                        foreach (UserDefinedDataType uddt in db.UserDefinedDataTypes)
                        {
                            tnUDT.Nodes.Add(uddt.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHelper.ShowException(this, ex);
                }

                // UDF
                if (db.UserDefinedFunctions.Count > 0)
                {
                    TreeNode tnUDF = null;
                    foreach (UserDefinedFunction udf in db.UserDefinedFunctions)
                    {
                        if (!udf.IsSystemObject)
                        {
                            if (tnUDF == null)
                            {
                                tnUDF = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeUDF);
                            }

                            if (udf.IsEncrypted)
                            {
                                tnUDF.Nodes.Add(GetEncryptedNode(udf.ToString()));
                            }
                            else
                            {
                                tnUDF.Nodes.Add(udf.ToString());
                            }
                        }
                    }
                }

                // Views
                if (db.Views.Count > 0)
                {
                    TreeNode tnViews = null;
                    foreach (Microsoft.SqlServer.Management.Smo.View vw in db.Views)
                    {
                        if (!vw.IsSystemObject)
                        {
                            if (tnViews == null)
                            {
                                tnViews = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeViews);
                            }

                            if (vw.IsEncrypted)
                            {
                                tnViews.Nodes.Add(GetEncryptedNode(vw.ToString()));
                            }
                            else
                            {
                                tnViews.Nodes.Add(vw.ToString());
                            }
                        }
                    }
                }

                if (_SourceServer.ConnectionContext.DatabaseEngineType != DatabaseEngineType.SqlAzureDatabase) //_SourceServer.ServerType
                {
                    // XML Schema Collections
                    if (db.XmlSchemaCollections.Count > 0)
                    {
                        TreeNode tnXMLSC = tvDatabaseObjects.Nodes.Add(Properties.Resources.ObjectTypeXMLSchemaCollections);
                        foreach (XmlSchemaCollection xsc in db.XmlSchemaCollections)
                        {
                            tnXMLSC.Nodes.Add(xsc.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
            }
            tvDatabaseObjects.ResumeLayout();
        }

        private void AddChildNode(ref TreeNode tnObjects, ref TreeNode tnEncrypted, TreeNode parent)
        {
            if (parent.Checked)
            {
                TreeNode tnObj = tnObjects.Nodes.Add(parent.Text);
                TreeNode tnObjEnc = new TreeNode(parent.Text);

                foreach (TreeNode child in parent.Nodes)
                {
                    if (child.ForeColor == Color.Red)
                    {
                        tnObjEnc.Nodes.Add(child.Text);
                    }
                    else if (child.Checked)
                    {
                        tnObj.Nodes.Add(child.Text);
                    }
                }

                if (tnObjEnc.Nodes.Count > 0)
                {
                    tnEncrypted.Nodes.Add(tnObjEnc);
                }
            }
        }

        private void DisplaySummary()
        {
            tvSummary.Nodes.Clear();
            Database db = GetSourceDatabaseFromListBox();

            TreeNode tnDatabase = tvSummary.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeDatabase));
            tnDatabase.Nodes.Add(db.ToString());

            SMOScriptOptions scriptOptions = _smoScriptOpts;
            TreeNode tnOptions = tvSummary.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeOptions));

            TreeNode tnGen = tnOptions.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeGeneral));
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptHeaders) + scriptOptions.ScriptHeaders.ToString());
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptDefaults) + scriptOptions.ScriptDefaults.ToString());
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeIfNotExists) + scriptOptions.IncludeIfNotExists.ToString());
            tnGen.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptDropCreate) + scriptOptions.ScriptDropCreate.ToString());

            TreeNode tnTblVwOpt = tnOptions.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeTableView));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptConstraints, scriptOptions.ScriptCheckConstraints));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptCollation, scriptOptions.ScriptCollation));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptData, scriptOptions.ScriptTableAndOrData));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptForeignKeys, scriptOptions.ScriptForeignKeys));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptIndexes, scriptOptions.ScriptIndexes));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptPrimaryKey, scriptOptions.ScriptPrimaryKeys));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptUniqueKeys, scriptOptions.ScriptUniqueKeys));
            tnTblVwOpt.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeScriptUniqueKeys, scriptOptions.ScriptTableTriggers));

            TreeNode tnTSQLCompat = tnOptions.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeTSQLCompatibility));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecks, scriptOptions.CompatibilityChecks));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksActiveDirectory, scriptOptions.ActiveDirectorySP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksBackupRestore, scriptOptions.BackupandRestoreTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksCDC, scriptOptions.ChangeDataCapture));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksCDCT, scriptOptions.ChangeDataCaptureTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksEngineSP, scriptOptions.DatabaseEngineSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksMail, scriptOptions.DatabaseMailSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksMaintenance, scriptOptions.DatabaseMaintenancePlan));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksDataControls, scriptOptions.DataControl));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksDistributedQueries, scriptOptions.DistributedQueriesSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksFullTextSearch, scriptOptions.FullTextSearchSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksGeneralExtended, scriptOptions.GeneralExtendedSPs));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksGeneralTSQL, scriptOptions.GeneralTSQL));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksIntegrationServicesTable, scriptOptions.IntegrationServicesTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksLogShipping, scriptOptions.LogShipping));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksMetadataFunction, scriptOptions.MetadataFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksOLEAutomationSP, scriptOptions.OLEAutomationSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksOLEDBTable, scriptOptions.OLEDBTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksProfilerSP, scriptOptions.ProfilerSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksReplicationSP, scriptOptions.ReplicationSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksReplicationTable, scriptOptions.ReplicationTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksRowsetFunction, scriptOptions.RowsetFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSecurityFunction, scriptOptions.SecurityFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSecuritySP, scriptOptions.SecuritySP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSQLMailSP, scriptOptions.SQLMailSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSQLServerAgentSP, scriptOptions.SQLServerAgentSP));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSQLServerAgentTable, scriptOptions.SQLServerAgentTable));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSystemCatalogView, scriptOptions.SystemCatalogView));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSystemFunction, scriptOptions.SystemFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksSystemStatisticalFunction, scriptOptions.SystemStatisticalFunction));
            tnTSQLCompat.Nodes.Add(CommonFunc.FormatString(Properties.Resources.CompatibilityChecksUnclassified, scriptOptions.Unclassified));

            TreeNode tnObjects = tvSummary.Nodes.Add(CommonFunc.FormatString(Properties.Resources.TreeNodeObjects));
            TreeNode tnEncrypted = new TreeNode(CommonFunc.FormatString(Properties.Resources.TreeNodeEncrypted));
            foreach (TreeNode parent in tvDatabaseObjects.Nodes)
            {
                AddChildNode(ref tnObjects, ref tnEncrypted, parent);
            }

            if (tnEncrypted.Nodes.Count > 0)
            {
                tnEncrypted.BackColor = Color.Yellow;
                tnEncrypted.ForeColor = Color.Red;
                tvSummary.Nodes.Add(tnEncrypted);
            }

            panelTreeViewSummary.Visible = true;
        }

        private void SetActionTitles()
        {
            lbAction.Text = _CurrentWizardAction[_wizardIndex];
            lbActionDesc.Text = _CurrentWizardActDes[_wizardIndex];
        }

        private void DisplayPrevious()
        {
            if (_wizardIndex == WizardSteps.ResultsSummary)
            {
                btnNext.Enabled = true;
            }
            _wizardIndex = _crumb[--_crumbIdx];

            SetActionTitles();

            btnSelectAll.Enabled = false;
            btnClearAll.Enabled = false;

            HideAll();

            switch (_wizardIndex)
            {
                case WizardSteps.SelectProcess:
                    btnBack.Enabled = false;
                    btnNext.Enabled = true;
                    panelWizardOptions.Visible = true;
                    break;

                case WizardSteps.SelectDatabaseSource:
                    btnBack.Enabled = true;
                    panelDatabaseSource.Visible = true;
                    break;

                case WizardSteps.SelectObjectsToScript:
                    btnBack.Enabled = true;
                    panelObjectTypes.Visible = true;
                    break;

                case WizardSteps.ScriptWizardSummary:
                    btnNext.Enabled = true;
                    panelTreeViewSummary.Visible = true;
                    break;

                case WizardSteps.ResultsSummary:
                    panelResultsSummary.Visible = true;
                    btnNext.Enabled = true;
                    break;

                case WizardSteps.SetupTargetConnection:
                    panelTargetDatabase.Visible = true;
                    btnNext.Enabled = true;
                    break;
            }
        }

        private void ReadSourceFile(string file)
        {
            if (tabCtlResults.TabPages.Count == 1)
            {
                tabCtlResults.TabPages.Add(_sqlResultTab);
            }
            tabCtlResults.SelectedIndex = 1;

            try
            {
                StartFileParseAsyncProcess(file, rbAnalyzeMigrateTSQLFile.Checked);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                rtbResultsSummary.SelectionColor = Color.Red;
                rtbResultsSummary.AppendText(ex.Message + Environment.NewLine);
            }
        }

        private void DisplayTargetServer(ServerTypes serverType)
        {
            if (serverType == ServerTypes.SQLAzureFed)
            {
                splitTargetServers.Panel1Collapsed = false;
                splitTargetServers.Panel2Collapsed = true;
                btnDeleteDatabase.Text = Properties.Resources.FederationDelete;
                btnCreateDatabase.Text = Properties.Resources.FederationCreate;
            }
            else
            {
                splitTargetServers.Panel1Collapsed = true;
                splitTargetServers.Panel2Collapsed = false;
                btnDeleteDatabase.Text = Properties.Resources.DeleteDatabase;
                btnCreateDatabase.Text = Properties.Resources.CreateDatabase;
            }
        }

        private void DisplayNext()
        {
            SetActionTitles();

            btnClearAll.Enabled = false;
            btnSelectAll.Enabled = false;
            btnCancelProcessing.Enabled = true;

            HideAll();

            switch (_wizardIndex)
            {
                case WizardSteps.SelectProcess:
                    btnBack.Enabled = false;
                    panelWizardOptions.Visible = true;
                    break;

                case WizardSteps.SelectDatabaseSource: // Script Options
                    btnBack.Enabled = true;
                    panelDatabaseSource.Visible = true;
                    if (lbDatabases.Items.Count == 0)
                    {
                        btnConnectToServer_Click(null, null);
                    }
                    break;

                case WizardSteps.SelectObjectsToScript:
                    if (_Reset)
                    {
                        FigureOutObjectTypesAvailable();
                        rbScriptAll.Checked = true;
                        btnSelectAll_Click(null, null);
                        _Reset = false;
                    }

                    btnBack.Enabled = true;
                    panelObjectTypes.Visible = true;
                    break;

                case WizardSteps.ScriptWizardSummary:
                    DisplaySummary();
                    btnNext.Enabled = true;
                    break;

                case WizardSteps.ResultsSummary:
                    panelResultsSummary.Visible = true;
                    if (rbAnalyzeTraceFile.Checked)
                    {
                        tabCtlResults.TabPages.Remove(_sqlResultTab);
                        tabCtlResults.SelectedIndex = 0;
                    }
                    else
                    {
                        if (tabCtlResults.TabPages.Count == 1)
                        {
                            tabCtlResults.TabPages.Add(_sqlResultTab);
                        }
                        tabCtlResults.SelectedIndex = 0;
                    }
                    btnNext.Enabled = false;
                    break;

                case WizardSteps.SetupTargetConnection:
                    if (_TargetServerInfo != null)
                    {
                        DisplayTargetServer(_TargetServerInfo.ServerType);
                        panelTargetDatabase.Visible = true;
                    }
                    else
                    {
                        DisplayTargetServer(CommonFunc.GetEnumServerType(CommonFunc.GetAppSettingsStringValue("TargetServerType")));
                        panelTargetDatabase.Visible = true;
                        btnConnectTargetServer_Click(null, null);
                    }
                    if (rbMaintenance.Checked) btnNext.Enabled = false;
                    btnBack.Enabled = true;
                    break;

                case WizardSteps.TargetResults:
                    panelTargetResults.Visible = true;
                    btnNext.Enabled = false;
                    break;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (rbMaintenance.Checked)
            {
                _crumb[_crumbIdx++] = _wizardIndex;
                _wizardIndex = WizardSteps.SetupTargetConnection;
                DisplayNext();
                return;
            }
            else if (_wizardIndex == WizardSteps.SelectProcess && (rbRunTSQLFile.Checked || rbAnalyzeTraceFile.Checked || rbAnalyzeMigrateTSQLFile.Checked))
            {
                if (File.Exists(tbSourceFile.Text))
                {
                    _crumb[_crumbIdx++] = _wizardIndex;
                    _wizardIndex = WizardSteps.ResultsSummary;
                    DisplayNext();
                    ReadSourceFile(tbSourceFile.Text);
                }
                else
                {
                    btnNext.Enabled = false;
                    MessageBox.Show(Properties.Resources.MessageFileDoesNotExists);
                    tbSourceFile.Focus();
                }
                return;
            }
            else if (_wizardIndex == WizardSteps.ScriptWizardSummary || _wizardIndex == WizardSteps.SetupTargetConnection)
            {
                this.Cursor = Cursors.WaitCursor;

                switch (_wizardIndex)
                {
                    case WizardSteps.ScriptWizardSummary:
                        DialogResult dr = MessageBox.Show(Properties.Resources.MessageReadyToGenerate, Properties.Resources.TitleGenerateScript, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            btnNext.Enabled = false;
                            _crumb[_crumbIdx++] = _wizardIndex++;
                            DisplayNext();
                            StartGenScriptAsyncProcess();
                        }
                        break;

                    case WizardSteps.SetupTargetConnection:
                        DialogResult dr2 = MessageBox.Show(Properties.Resources.MessageExecuteScript, Properties.Resources.TitleExecuteScript, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr2 == DialogResult.Yes)
                        {
                            btnNext.Enabled = false;
                            _crumb[_crumbIdx++] = _wizardIndex++;
                            DisplayNext();
                            if (_TargetServerInfo.ServerType == ServerTypes.SQLAzureFed)
                            {
                                StartFederationTargetAsyncProcess();
                            }
                            else
                            {
                                StartTargetAsyncProcess();
                            }
                        }
                        break;
                }
                this.Cursor = Cursors.Default;
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            _crumb[_crumbIdx++] = _wizardIndex++;
            DisplayNext();

            this.Cursor = Cursors.Default;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            CancelAsyncProcesses();
            DisplayPrevious();
            AsyncProcessingStatus.CancelProcessing = false;
        }

        private void lbDatabases_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e != null && e.Index > -1)
            {
                e.DrawBackground();
                Brush myBrush = Brushes.Gray;

                DatabaseInfo db = (DatabaseInfo)lbDatabases.Items[e.Index];
                if (db.IsDbOwner) //IsDbAccessAdmin
                {
                    myBrush = Brushes.Black;
                }
                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                e.DrawFocusRectangle();
            }
        }

        private void DisplayFederatedMembers(TargetServerInfo targetServer, SqlConnection connection, ListBox lb)
        {
            lb.Items.Clear();
            string sqlQuery = "SELECT fed.federation_id, fed.name, fmd.distribution_name, fmd.member_id, range_low, range_high, typ.name" +
                               " FROM sys.federations fed" +
                               " JOIN sys.Federation_distributions dis ON dis.federation_id = fed.federation_id" +
                               " JOIN sys.federation_member_distributions fmd ON fmd.federation_id = fed.federation_id" +
                               " JOIN sys.types typ ON typ.system_type_id = dis.system_type_id" +
                              " ORDER BY fed.name, range_low";

            List<FederationDetails> federations = new List<FederationDetails>();
            FederationDetails details = null;

            Cursor orig = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            FederationMemberDistribution mdRoot = FederationCommonFuncs.GetRootTables(targetServer);

            // Note to self.  For some reason yet to be figured out, ExecuteReader throws an error when swapping between SQL Azure and SQL Azure Federations
            // Closing the connection and opening the connection again seems to fix the problem.

            if (connection.State != ConnectionState.Closed) connection.Close();

            Retry.ExecuteRetryAction(() =>
                {
                    connection.Open();

                    using (SqlDataReader sdr = SqlHelper.ExecuteReader(connection, CommandType.Text, sqlQuery))
                    {
                        while (sdr.Read())
                        {
                            if (details == null || details.FederationName != sdr.GetString(1))
                            {
                                if (details != null) federations.Add(details);
                                details = new FederationDetails();
                                details.Federation_id = sdr.GetInt32(0);
                                details.FederationName = sdr.GetString(1);
                                if (mdRoot.Tables.Count > 0)
                                {
                                    details.Members.Add(mdRoot);
                                }
                            }

                            FederationMemberDistribution md = new FederationMemberDistribution();
                            md.DistrubutionName = sdr.GetString(2);
                            md.Member_ID = sdr.GetInt32(3);
                            md.FedType = sdr.GetString(6);
                            if (md.FedType == "varbinary")
                            {
                                md.Low = CommonFunc.ConverByteArrayToString(sdr.GetSqlBinary(4));
                                if (sdr[5].ToString().Length == 0)
                                {
                                    md.High = "";
                                }
                                else
                                {
                                    md.High = CommonFunc.ConverByteArrayToString(sdr.GetSqlBinary(5));
                                }
                            }
                            else
                            {
                                md.Low = sdr[4].ToString();
                                md.High = sdr[5].ToString();
                            }
                            details.Members.Add(md);
                        }
                        if (details != null) federations.Add(details);
                        sdr.Close();
                    }
                    connection.Close();
                    connection.Dispose();
                });

            foreach (FederationDetails fd in federations)
            {
                lb.Items.Add(fd);
            }

            if (lb.Items.Count > 0)
            {
                lb.SetSelected(0, true);
            }
            Cursor.Current = orig;
        }

        private bool IsDatabaseOwner(SqlConnection connection)
        {
            bool owner = false;
            string sqlQuery = "SELECT is_member(N'db_owner') AS [IsDbOwner]";

            Retry.ExecuteRetryAction(() =>
            {
                ScalarResults sr = SqlHelper.ExecuteScalar(connection, CommandType.Text, sqlQuery);
                owner = Convert.ToBoolean((int) sr.ExecuteScalarReturnValue);
            });
            return owner;
        }

        private void DisplayDatabases(SqlConnection connection, ListBox lb)
        {
            lb.Items.Clear();
            string database = connection.Database;

            if (database.Length > 0 && !database.Equals("master", StringComparison.OrdinalIgnoreCase))
            {
                DatabaseInfo dbi = new DatabaseInfo();
                dbi.DatabaseName = database;
                dbi.ConnectedTo = TypeOfConnection.UsingADO;
                dbi.IsDbOwner = IsDatabaseOwner(connection);
                lb.Items.Add(dbi);
            }
            else
            {
                string sqlQuery = "SELECT dtb.name AS [Name]" +
                                       " , CAST(case when dtb.name in ('master','model','msdb','tempdb') then 1 else dtb.is_distributor end AS bit) AS [IsSystemObject]" +
                                       " , dtb.is_read_only AS [ReadOnly]" +
                                  " FROM sys.databases AS dtb" +
                                 " ORDER BY[Name] ASC";

                // Note to self.  For some reason yet to be figured out, ExecuteReader throws an error when swapping between SQL Azure and SQL Azure Federations
                // Closing the connection and opening the connection again seems to fix the problem.

                if (connection.State != ConnectionState.Closed) connection.Close();

                string tmp = _TargetServerInfo.TargetDatabase;
                Retry.ExecuteRetryAction(() =>
                {
                    connection.Open();

                    using (SqlDataReader sdr = SqlHelper.ExecuteReader(connection, CommandType.Text, sqlQuery))
                    {
                        while (sdr.Read())
                        {
                            bool IsSystemObject = sdr.GetBoolean(1);
                            if (IsSystemObject) continue;

                            DatabaseInfo dbi = new DatabaseInfo();
                            dbi.DatabaseName = sdr.GetString(0);
                            bool ReadOnly = sdr.GetBoolean(2);
                            if (ReadOnly)
                            {
                                dbi.IsDbOwner = false;
                            }
                            else
                            {
                                _TargetServerInfo.TargetDatabase = dbi.DatabaseName;
                                using (SqlConnection con = new SqlConnection(_TargetServerInfo.ConnectionStringTargetDatabase))
                                {
                                    dbi.IsDbOwner = IsDatabaseOwner(con);
                                }
                            }
                            lb.Items.Add(dbi);
                        }
                        sdr.Close();
                    }

                    connection.Close();
                });
                _TargetServerInfo.TargetDatabase = tmp;
            }
        }

        private void DisplayDatabases(ref Server dbServer, ListBox lb)
        {
            try
            {
                // Clear control
                lb.Items.Clear();
                string database = dbServer.ConnectionContext.SqlConnectionObject.Database;

                if (database.Length > 0 && !database.Equals("master", StringComparison.OrdinalIgnoreCase))
                {
                    Server svr = CommonFunc.GetSmoServer(dbServer.ConnectionContext);
                    Database db = (Database)svr.Databases[database];
                    DatabaseInfo di = new DatabaseInfo();
                    di.DatabaseObject = db;
                    try
                    {
                        di.IsDbOwner = db.IsDbOwner;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        di.IsDbOwner = false;
                    }
                    lb.Items.Add(di);
                }
                else
                {
                    // Add database objects to combobox; the default ToString will display the database name
                    foreach (Database db in dbServer.Databases)
                    {
                        DatabaseInfo di = new DatabaseInfo();
                        di.DatabaseObject = db;
                        if (db.IsSystemObject == true) continue;
                        try
                        {
                            if (db.IsDbOwner) //IsDbAccessAdmin
                            {
                                di.IsDbOwner = true;
                            }
                            else
                            {
                                di.IsDbOwner = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            di.IsDbOwner = false;
                        }

                        lb.Items.Add(di);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
            }
        }

        private void ResetFields(Panel pan)
        {
            foreach (Control ctl in pan.Controls)
            {
                if (ctl.GetType() == typeof(CheckedListBox))
                {
                    ((CheckedListBox)ctl).Items.Clear();
                }
                else if (ctl.GetType() == typeof(TreeView))
                {
                    ((TreeView)ctl).Nodes.Clear();
                }
                else if (ctl.GetType() == typeof(RichTextBox))
                {
                    ((RichTextBox)ctl).Clear();
                }
                else if (ctl.GetType() == typeof(Panel))
                {
                    ResetFields((Panel) ctl);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            bool allowNext = false;

            _IgnorCheck = true;
            foreach (TreeNode tn in tvDatabaseObjects.Nodes)
            {
                int numChildSelected = 0;
                foreach (TreeNode child in tn.Nodes)
                {
                    if (child.ForeColor == Color.Red) continue;

                    child.Checked = true;
                    ++numChildSelected;
                }

                if (numChildSelected > 0)
                {
                    tn.Checked = true;
                    allowNext = true;
                }
            }
            _IgnorCheck = false;

            if (allowNext) btnNext.Enabled = true;
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            _IgnorCheck = true;
            foreach (TreeNode tn in tvDatabaseObjects.Nodes)
            {
                tn.Checked = false;
                foreach (TreeNode child in tn.Nodes)
                {
                    child.Checked = false;
                }
            }
            _IgnorCheck = false;
            btnNext.Enabled = false;
        }

        private void lbDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Reset = true;
            if (lbDatabases.SelectedItem != null)
            {
                DatabaseInfo db = (DatabaseInfo)lbDatabases.SelectedItem;
                try
                {
                    if (db.IsDbOwner)
                    {
                        btnNext.Enabled = true;
                    }
                    else
                    {
                        btnNext.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    btnNext.Enabled = false;
                }
                return;
            }
            else
            {
                btnNext.Enabled = false;
            }
        }

        private SqlSmoObject[] GetSortedObjects(Database sourceDatabase)
        {
            // Ok, we need to get all of the selected objects and put them into one array
            // so that we can get them sorted by dependency.
            
            bool dataOnly = Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase);
            List<SqlSmoObject> objectList = new List<SqlSmoObject>();

            foreach (TreeNode parent in tvDatabaseObjects.Nodes)
            {
                if (parent.Checked)
                {
                    if (!dataOnly && _SourceServer.ConnectionContext.ServerVersion.Major > 9 && parent.Text.Equals(Properties.Resources.ObjectTypeTriggers))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.Triggers)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.ForeColor == Color.Red) continue;

                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (!dataOnly && parent.Text.Equals(Properties.Resources.ObjectTypeStoredProcedures))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.StoredProcedures)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.ForeColor == Color.Red) continue;

                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (parent.Text.Equals(Properties.Resources.ObjectTypeTables))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.Tables)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (!dataOnly && parent.Text.Equals(Properties.Resources.ObjectTypeUDF))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.UserDefinedFunctions)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.ForeColor == Color.Red) continue;

                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    else if (!dataOnly && parent.Text.Equals(Properties.Resources.ObjectTypeViews))
                    {
                        foreach (SqlSmoObject obj in sourceDatabase.Views)
                        {
                            foreach (TreeNode child in parent.Nodes)
                            {
                                if (child.ForeColor == Color.Red) continue;

                                if (child.Checked)
                                {
                                    if (obj.ToString().Equals(child.Text))
                                    {
                                        objectList.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Sort(sourceDatabase, objectList.ToArray());
        }

        // Method for sorting the dependencies -- Thanks to bktan81 for code

        private SqlSmoObject[] Sort(Database sourceDatabase, SqlSmoObject[] smoObjects)
        {
            if (smoObjects.Count() < 2) return smoObjects;

            DependencyTree dt = null;
            DependencyWalker dw = new DependencyWalker(sourceDatabase.Parent);

            try
            {
                dt = dw.DiscoverDependencies(smoObjects, true);
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
                return smoObjects;
            }

            SqlSmoObject[] sorted = new SqlSmoObject[smoObjects.Count()];
            int index = 0;

            foreach (DependencyCollectionNode d in dw.WalkDependencies(dt))
            {
                if (d.Urn.Type.Equals("UnresolvedEntity")) continue;

                foreach (SqlSmoObject sso in smoObjects)
                {
                    if (sso.Urn.ToString().Equals(d.Urn, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sorted[index++] = sso;
                        break;
                    }
                }
            }
            return sorted;
        }

        private SqlSmoObject[] GetSMOObjectListFromTreeViewNode(string objectType)
        {
            List<SqlSmoObject> objectList = new List<SqlSmoObject>();
            foreach (TreeNode parent in tvDatabaseObjects.Nodes)
            {
                if (parent.Text.Equals(objectType))
                {
                    if (parent.Checked)
                    {
                        if (parent.Text.Equals(Properties.Resources.ObjectTypeRoles))
                        {
                            foreach (SqlSmoObject obj in _sourceDatabase.Roles)
                            {
                                foreach (TreeNode child in parent.Nodes)
                                {
                                    if (child.Checked)
                                    {
                                        if (obj.ToString().Equals(child.Text))
                                        {
                                            objectList.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        else if (parent.Text.Equals(Properties.Resources.ObjectTypeTriggers))
                        {
                            foreach (SqlSmoObject obj in _sourceDatabase.Triggers)
                            {
                                foreach (TreeNode child in parent.Nodes)
                                {
                                    if (child.ForeColor == Color.Red) continue;

                                    if (child.Checked)
                                    {
                                        if (obj.ToString().Equals(child.Text))
                                        {
                                            objectList.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        else if (parent.Text.Equals(Properties.Resources.ObjectTypeSchemas))
                        {
                            foreach (SqlSmoObject obj in _sourceDatabase.Schemas)
                            {
                                foreach (TreeNode child in parent.Nodes)
                                {
                                    if (child.Checked)
                                    {
                                        if (obj.ToString().Equals(child.Text))
                                        {
                                            objectList.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        else if (parent.Text.Equals(Properties.Resources.ObjectTypeUDT))
                        {
                            foreach (SqlSmoObject obj in _sourceDatabase.UserDefinedDataTypes)
                            {
                                foreach (TreeNode child in parent.Nodes)
                                {
                                    if (child.Checked)
                                    {
                                        if (obj.ToString().Equals(child.Text))
                                        {
                                            objectList.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        else if (parent.Text.Equals(Properties.Resources.ObjectTypeXMLSchemaCollections))
                        {
                            foreach (SqlSmoObject obj in _sourceDatabase.XmlSchemaCollections)
                            {
                                foreach (TreeNode child in parent.Nodes)
                                {
                                    if (child.Checked)
                                    {
                                        if (obj.ToString().Equals(child.Text))
                                        {
                                            objectList.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                        else if (parent.Text.Equals(Properties.Resources.ObjectTypeUDTT))
                        {
                            foreach (SqlSmoObject obj in _sourceDatabase.UserDefinedTableTypes)
                            {
                                foreach (TreeNode child in parent.Nodes)
                                {
                                    if (child.Checked)
                                    {
                                        if (obj.ToString().Equals(child.Text))
                                        {
                                            objectList.Add(obj);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return objectList.ToArray();
        }

        private void ScriptDrops(SqlSmoObject[] sorted, SqlSmoObject[] smoRoles, SqlSmoObject[] smoSchemas, SqlSmoObject[] smoSchemasCols, SqlSmoObject[] smoUDTs, SqlSmoObject[] smoUDTTs, ScriptDatabase sdb)
        {
            int objCount = sorted.Count<SqlSmoObject>();
            if (objCount == 0) return;

            SqlSmoObject[] smoReverseSorted = new SqlSmoObject[objCount];
            for (int index = 0; index < objCount; index++)
            {
                smoReverseSorted[index] = sorted[objCount - 1 - index];
            }

            if (sorted.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoReverseSorted);
            if (smoUDTs.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoUDTs);
            if (smoUDTTs.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoUDTTs);
            if (smoSchemas.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoSchemas);
            if (smoRoles.Count<SqlSmoObject>() > 0)
            {
                SqlSmoObject[] roles = new SqlSmoObject[1];
                foreach (SqlSmoObject role in smoRoles)
                {
                    roles[0] = role;
                    sdb.ScriptDrops(roles);
                }
            }
        }

        private void GenerateScriptFromSourceServer()
        {
            DateTime dtStart = DateTime.Now;
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(GenScriptAsyncUpdateStatusHandler);
            AsyncNotificationEventArgs args = new AsyncNotificationEventArgs(NotificationEventFunctionCode.GenerateScriptFromSQLServer, 0, "", CommonFunc.FormatString(Properties.Resources.MessageProcessStarted, dtStart.ToString(CultureInfo.CurrentUICulture), dtStart.ToUniversalTime().ToString(CultureInfo.CurrentUICulture)) + Environment.NewLine, Color.DarkBlue);
            StreamWriter swTSQL = null;
            SqlSmoObject[] smoTriggers = null;
            Object sender = System.Threading.Thread.CurrentThread;

            updateStatus(args);

            ServerConnection sc = new ServerConnection();

            sc.ServerInstance = _SourceServerInfo.ServerInstance;
            sc.SqlExecutionModes = SqlExecutionModes.ExecuteSql;
            sc.ConnectTimeout = (Int32)15;
            if (_SourceServerInfo.ServerType == ServerTypes.SQLAzureFed)
            {
                sc.DatabaseName = "";
            }
            else
            {
                sc.DatabaseName = _SourceServerInfo.TargetDatabase;
            }
            sc.LoginSecure = _SourceServerInfo.LoginSecure;
            if (!_SourceServerInfo.LoginSecure)
            {
                sc.Login = _SourceServerInfo.Login;
                sc.Password = _SourceServerInfo.Password;
            }
            sc.Connect();

            Server ss = new Server(sc);
            Database db = ss.Databases[_sourceDatabase.Name];

            ScriptDatabase sdb = new ScriptDatabase(_federationDetails, _SourceServerInfo, _memberDistribution);
            sdb.Initialize(ss, db, updateStatus, _smoScriptOpts, swTSQL);

            args.DisplayText = "";
            args.StatusMsg = Properties.Resources.MessageSorting;
            args.PercentComplete = 1;
            updateStatus(args);

            // Tables, Views, Stored Procedures, and Triggers can all have dependencies.  GetSortedObjects returns
            // these objects in dependency order.

            SqlSmoObject[] sorted = GetSortedObjects(_sourceDatabase);
            if (_SourceServer.ConnectionContext.ServerVersion.Major < 10)
            {
                smoTriggers = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeTriggers);
            }

            SqlSmoObject[] smoRoles = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeRoles);
            SqlSmoObject[] smoSchemas = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeSchemas);
            SqlSmoObject[] smoSchemasCols = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeXMLSchemaCollections);
            SqlSmoObject[] smoUDTs = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeUDT);
            SqlSmoObject[] smoUDTTs = GetSMOObjectListFromTreeViewNode(Properties.Resources.ObjectTypeUDTT);

            if (Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDrop"), RegexOptions.IgnoreCase) ||
                Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDropCreate"), RegexOptions.IgnoreCase))
            {
                args.StatusMsg = Properties.Resources.MessageCreatingDropScripts;
                args.PercentComplete = 2;
                updateStatus(args);

                ScriptDrops(sorted, smoRoles, smoSchemas, smoSchemasCols, smoUDTs, smoUDTTs, sdb);
            }

            if (Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSCreate"), RegexOptions.IgnoreCase) ||
                Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDropCreate"), RegexOptions.IgnoreCase))
            {
                SourceProcessor sp = new SourceProcessor();
                sp.Initialize(sdb, _smoScriptOpts, updateStatus, args, ConfigurationManager.AppSettings["BCPFileDir"]);

                // Roles, Schemas, XML Schema Collections and UDT have no dependencies.  Thus we process one at a time.

                if (!Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase))
                {
                    if (sp.Process(DatabaseObjectsTypes.Roles, smoRoles, 5)) return;
                    if (sp.Process(DatabaseObjectsTypes.Schemas, smoSchemas, 10)) return;
                    if (sp.Process(DatabaseObjectsTypes.XMLSchemaCollections, smoSchemasCols, 15)) return;
                    if (sp.Process(DatabaseObjectsTypes.UserDefinedDataTypes, smoUDTs, 20)) return;
                    if (sp.Process(DatabaseObjectsTypes.UserDefinedTableTypes, smoUDTTs, 25)) return;
                }

                if (sp.Process(sorted, 30)) return;

                if (!Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase))
                {
                    if (sp.Process(DatabaseObjectsTypes.Triggers, smoTriggers, 95)) return;
                }
            }

            if (swTSQL != null)
            {
                swTSQL.Flush();
                swTSQL.Close();
            }

            DateTime dtEnd = DateTime.Now;
            TimeSpan tsDuration = dtEnd.Subtract(dtStart);
            string sHour = tsDuration.Minutes == 1 ? Properties.Resources.MessageHour : Properties.Resources.MessageHours;
            string sMin = tsDuration.Minutes == 1 ? Properties.Resources.MessageMinute : Properties.Resources.MessageMinutes;
            string sSecs = tsDuration.Seconds == 1 ? Properties.Resources.MessageSecond : Properties.Resources.MessageSeconds;

            args.StatusMsg = Properties.Resources.Done;
            args.DisplayColor = Color.DarkCyan;

            if (_smoScriptOpts.CheckCompatibility() == 1)
            {
                args.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageFinishedNoAnalysis, dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), tsDuration.Hours + sHour + tsDuration.Minutes.ToString(CultureInfo.CurrentUICulture) + sMin + tsDuration.Seconds.ToString(CultureInfo.CurrentUICulture) + sSecs);
            }
            else
            {
                args.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageFinishedWithAnalysis, dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), tsDuration.Hours + sHour + tsDuration.Minutes.ToString(CultureInfo.CurrentUICulture) + sMin + tsDuration.Seconds.ToString(CultureInfo.CurrentUICulture) + sSecs);
            }
            args.PercentComplete = 100;
            updateStatus(args);
        }

        private void ExecuteSQLonTarget()
        {
            AsyncQueueBCPJob queueBCPJob = new AsyncQueueBCPJob(AsyncQueueJobHandler);
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(TargetAsyncUpdateStatusHandler);
            TargetProcessor tp = new TargetProcessor();
 
            tp.ExecuteSQLonTarget(_TargetServerInfo, updateStatus, queueBCPJob, _sqlForAzure);
            tp.Close();
        }

        private void ParseTraceFile()
        {
            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(StatusUpdateHandler);
            AsyncNotificationEventArgs e = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ParseFile, 0, "", "", Color.Black);
            ScriptDatabase sdb = new ScriptDatabase();
            TraceFile tf = null;
            int totalRecords = 0;

            sdb.Initialize(updateStatus, _smoScriptOpts, true);

            /****************************************************************/

            e.DisplayColor = Color.DarkBlue;
            e.DisplayText = Properties.Resources.MessageCalculatingSize;
            e.StatusMsg = Properties.Resources.MessageCalculatingNumberOrRecs + Environment.NewLine;
            e.PercentComplete = 0;
            updateStatus(e);

            try
            {
                // Part 1 -- Figure out number of records so that we can update the status bar in later processing

                using (tf = new TraceFile())
                {
                    int cnt = 0;
                    tf.InitializeAsReader(_FileToProcess);

                    while (tf.Read())
                    {
                        if (AsyncProcessingStatus.CancelProcessing) break;

                        ++totalRecords;
                        if (totalRecords % 10000 == 0)
                        {
                            ++cnt;
                            if (e.DisplayColor == Color.DarkBlue)
                            {
                                e.DisplayColor = Color.Brown;
                            }
                            else
                            {
                                e.DisplayColor = Color.DarkBlue;
                            }

                            if (cnt > 60)
                            {
                                cnt = 0;
                                e.DisplayText = "." + Environment.NewLine;
                            }
                            else
                            {
                                e.DisplayText = ".";
                            }

                            e.PercentComplete = 2;
                            updateStatus(e);
                        }
                    }
                    tf.Close();
                }
            }
            catch (Exception ex)
            {
                if (!AsyncProcessingStatus.CancelProcessing)
                {
                    e.DisplayColor = Color.Red;
                    e.DisplayText = Environment.NewLine + CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailedAt, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine;
                    e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailed, ex.Message);
                    e.PercentComplete = 100;
                    updateStatus(e);
                    return;
                }
            }

            if (!AsyncProcessingStatus.CancelProcessing)
            {
                int one = 1;
                e.DisplayColor = Color.DarkBlue;
                e.DisplayText = Environment.NewLine;
                e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingStatus, one.ToString(CultureInfo.CurrentUICulture), totalRecords.ToString(CultureInfo.CurrentUICulture));
                e.PercentComplete = 3;
                updateStatus(e);

                string[] eventsToCheck = CommonFunc.GetAppSettingsStringValue("TraceEventsToCheck").Split('|');

                try
                {
                    // Part 2 -- Start over and analyze TSQL

                    using (tf = new TraceFile())
                    {
                        int loopCtr = 1;
                        tf.InitializeAsReader(_FileToProcess);

                        int ecOrdinal = tf.GetOrdinal("EventClass");
                        int tbOrdinal = tf.GetOrdinal("TextData");

                        while (tf.Read())
                        {
                            if (AsyncProcessingStatus.CancelProcessing) break;

                            string eventClass = tf.GetString(ecOrdinal);
                            if (eventClass != null && eventClass.Length > 0)
                            {
                                foreach (string etc in eventsToCheck)
                                {
                                    if (eventClass.Equals(etc, StringComparison.OrdinalIgnoreCase))
                                    {
                                        string sql = tf.GetString(tbOrdinal);
                                        if (sql != null && sql.Length > 0)
                                        {
                                            sdb.TSQLChecks(sql, null, loopCtr);
                                        }
                                        break;
                                    }
                                }
                            }

                            if (loopCtr++ % 20 == 0)
                            {
                                e.PercentComplete = (int)(((float) loopCtr / (float) totalRecords) * 100.0);
                                e.DisplayColor = Color.DarkBlue;
                                e.DisplayText = "";
                                e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageProcessingStatus, loopCtr.ToString(CultureInfo.CurrentUICulture), totalRecords.ToString(CultureInfo.CurrentUICulture));
                                updateStatus(e);
                            }
                        }
                    }
                    tf.Close();
                }
                catch (Exception ex)
                {
                    if (!AsyncProcessingStatus.CancelProcessing)
                    {
                        e.DisplayColor = Color.Red;
                        e.DisplayText = Environment.NewLine + CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailedAt, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine;
                        e.StatusMsg = CommonFunc.FormatString(Properties.Resources.MessageAnalysisFailed, ex.ToString());
                        e.PercentComplete = 100;
                        updateStatus(e);
                        return;
                    }
                }
            }

            if (AsyncProcessingStatus.CancelProcessing)
            {
                e.StatusMsg = Properties.Resources.MessageCanceled;
                e.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
            }
            else
            {
                e.DisplayColor = Color.DarkCyan;
                e.DisplayText = Properties.Resources.AnalysisComplete + Environment.NewLine;
                e.StatusMsg = Properties.Resources.Done;
            }
            e.PercentComplete = 100;
            updateStatus(e);
        }

        private void tbOutputFile_TextChanged(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text.Length > 0)
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
            }
        }

        private void ConnectToTargetServer()
        {
            if (_TargetServerInfo == null)
            {
                _TargetServerInfo = new TargetServerInfo();
                _TargetServerInfo.LoginSecure = CommonFunc.GetAppSettingsBoolValue("TargetConnectNTAuth");
                _TargetServerInfo.ServerType = CommonFunc.GetEnumServerType(CommonFunc.GetAppSettingsStringValue("TargetServerType"));
                _TargetServerInfo.ServerInstance = CommonFunc.GetAppSettingsStringValue("TargetServerName");
                _TargetServerInfo.Login = CommonFunc.GetAppSettingsStringValue("TargetUserName");
                _TargetServerInfo.Password = CommonFunc.GetAppSettingsStringValue("TargetPassword");
                _TargetServerInfo.RootDatabase = CommonFunc.GetAppSettingsStringValue("TargetDatabase");
            }

            SqlConnection connection = new SqlConnection();
            ServerConnect scForm = new ServerConnect(ref connection, ref _TargetServerInfo, true);
            DialogResult dr = scForm.ShowDialog(this);

            if (dr == DialogResult.OK)
            {
                Cursor orig = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                connection.Close();

                if (_TargetServerInfo.ServerType == ServerTypes.SQLAzureFed)
                {
                    ConnectToFederatedServer(connection);
                }
                else
                {
                    //ServerConnection connection = scForm.ServerConn;
                    ConnectToNonFederatedServer(connection);
                }
                Cursor.Current = orig;
            }
        }

        private void ConnectToNonFederatedServer(SqlConnection connection)
        {
            if (connection == null) return;

            DisplayTargetServer(ServerTypes.SQLServer);
            DisplayDatabases(connection, lbTargetDatabases);

            if (_TargetServerInfo.RootDatabase.Length > 0)
            {
                btnCreateDatabase.Enabled = false;
                btnDeleteDatabase.Enabled = false;
                lbTargetDatabases.SelectedIndex = 0;
                btnNext.Enabled = true;
            }
            else
            {
                btnCreateDatabase.Enabled = true;
            }
        }

        private void ConnectToFederatedServer(SqlConnection connection)
        {
            if (connection == null) return;

            DisplayTargetServer(ServerTypes.SQLAzureFed);

            lbFederations.Items.Clear();
            lbFederationMembers.Items.Clear();

            DisplayFederatedMembers(_TargetServerInfo, connection, lbFederations);
            if (lbFederations.Items.Count > 0)
            {
                lbFederations.SelectedIndex = 0;
                btnNext.Enabled = true;
                btnDeleteDatabase.Enabled = true;
            }
            else
            {
                btnDeleteDatabase.Enabled = false;
                btnNext.Enabled = false;
            }
        }

        private void btnConnectTargetServer_Click(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            ConnectToTargetServer();
        }

        private void btnConnectToServer_Click(object sender, EventArgs e)
        {
            ServerConnection connection = new ServerConnection();

            btnNext.Enabled = false;
            splitDBDisplay.Panel1Collapsed = true;
            gbSelectDatabase.Text = Properties.Resources.SelectDatabase;

            if (_SourceServerInfo == null)
            {
                _SourceServerInfo = new TargetServerInfo();
                _SourceServerInfo.ServerInstance = ConfigurationManager.AppSettings["SourceServerName"];
                _SourceServerInfo.Login = ConfigurationManager.AppSettings["SourceUserName"];
                _SourceServerInfo.Password = ConfigurationManager.AppSettings["SourcePassword"];
                _SourceServerInfo.LoginSecure = CommonFunc.GetAppSettingsBoolValue("SourceConnectNTAuth");
                _SourceServerInfo.ServerType = CommonFunc.GetEnumServerType(CommonFunc.GetAppSettingsStringValue("SourceServerType"));
                _SourceServerInfo.RootDatabase = ConfigurationManager.AppSettings["SourceDatabase"];
            }

            ServerConnect scForm = new ServerConnect(ref connection, ref _SourceServerInfo);
            DialogResult dr = scForm.ShowDialog(this);

            if ((dr == DialogResult.OK) && (connection.SqlConnectionObject.State == ConnectionState.Open))
            {
                Cursor orig = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                _SourceServer = CommonFunc.GetSmoServer(connection);

                if (_SourceServer != null)
                {
                    if (_SourceServerInfo.ServerType == ServerTypes.SQLAzureFed)
                    {
                        _SourceServerInfo.RootDatabase = scForm.SpecifiedDatabaseName;
                        splitDBDisplay.Panel1Collapsed = false;
                        gbSelectDatabase.Text = Properties.Resources.SelectFederationMember;

                        SqlConnection conn = new SqlConnection(_SourceServerInfo.ConnectionStringRootDatabase);
                        DisplayFederatedMembers(_SourceServerInfo, conn, lbSourceFederations);
                    }
                    else
                    {
                        _SourceServerInfo.TargetDatabase = scForm.SpecifiedDatabase ? scForm.SpecifiedDatabaseName : "";
                        DisplayDatabases(ref _SourceServer, lbDatabases);
                        if (scForm.SpecifiedDatabase)
                        {
                            lbDatabases.SelectedIndex = 0;
                            btnNext.Enabled = true;
                        }
                    }
                    _SourceServer.ConnectionContext.Disconnect();
                }
                Cursor.Current = orig;
            }
        }

        private void btnFindFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            if (rbAnalyzeTraceFile.Checked)
            {
                openFileDialog1.Filter = Properties.Resources.DialogFilterTrace;
                openFileDialog1.Title = Properties.Resources.DialogTitleTrace;
            }
            else
            {
                openFileDialog1.Filter = Properties.Resources.DialogFilterTSQL;
                openFileDialog1.Title = Properties.Resources.DialogTitleTSQL;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbSourceFile.Text = openFileDialog1.FileName;
                btnNext.Enabled = true;
            }
        }

        private void lbDatabases_DoubleClick(object sender, EventArgs e)
        {
            if (lbDatabases.SelectedItem != null && ((DatabaseInfo)lbDatabases.SelectedItem).IsDbOwner)
            {
                _Reset = true;
                btnNext_Click(sender, e);
            }
        }

        private void btnCancelTargetProcessing_Click(object sender, EventArgs e)
        {
            AsyncProcessingStatus.CancelProcessing = true;
            CancelAsyncProcesses();
            btnBack.Enabled = true;
            btnCancelTargetProcessing.Enabled = false;
            progressBarTargetServer.Value = 0;
            lbStatus.Text = Properties.Resources.MessageCanceled;
        }

        private void btnCancelProcessing_Click(object sender, EventArgs e)
        {
            AsyncProcessingStatus.CancelProcessing = true;
            CancelAsyncProcesses();
            btnBack.Enabled = true;
            progressBarGenScript.Value = 0;
            btnCancelProcessing.Enabled = false;
            lbResultsSummaryStatus.Text = Properties.Resources.MessageCanceled;
        }

        private void rtbSQLScript_VScroll(object sender, EventArgs e)
        {
            cbResultsScroll.Focus();
        }

        private void rtbAzureStatus_VScroll(object sender, EventArgs e)
        {
            cbAzureStatusScroll.Focus();
        }

        private void rtbResultsSummary_VScroll(object sender, EventArgs e)
        {
            cbResultsScroll.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = null;
            string title = "";

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.Title = title;
            saveFileDialog1.Filter = Properties.Resources.DialogFilterRTF;

            switch (_wizardIndex)
            {
                case WizardSteps.TargetResults:
                    rtb = rtbTargetResults;
                    title = Properties.Resources.SaveAzureResults;
                    break;

                case WizardSteps.ResultsSummary:
                    if (tabCtlResults.SelectedIndex == 0)
                    {
                        rtb = rtbResultsSummary;
                        title = Properties.Resources.SaveResults;
                    }
                    else
                    {
                        rtb = rtbSQLScript;
                        title = Properties.Resources.SaveSQLScript;
                        saveFileDialog1.Filter = Properties.Resources.DialogFilterSQLRTF;
                    }
                    break;
            }
            this.Cursor = Cursors.Default;

            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            string file = saveFileDialog1.FileName;
            try
            {
                StreamWriter sw = null;
                sw = new StreamWriter(file);
                if (file.EndsWith("rtf", StringComparison.CurrentCultureIgnoreCase))
                {
                    sw.Write(rtb.Rtf);
                }
                else
                {
                    foreach (string str in rtb.Lines)
                    {
                        sw.WriteLine(str);
                    }
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
                Cursor.Current = Cursors.Default;
                return;
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show(Properties.Resources.Done);
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAnalyzeMigrateTSQLFile.Checked || rbAnalyzeTraceFile.Checked || rbRunTSQLFile.Checked)
            {
                panelFileToProcess.Visible = true;
                if (tbSourceFile.Text.Length > 0)
                {
                    btnNext.Enabled = true;
                }
                else
                {
                    btnNext.Enabled = false;
                }
            }
            else
            {
                btnNext.Enabled = true;
                panelFileToProcess.Visible = false;
                if (rbMaintenance.Checked) return;
            }

            if (rbRunTSQLFile.Checked)
            {
                _smoScriptOpts.CompatibilityChecks = _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsOverrideNot");
            }
            else
            {
                if (ConfigurationManager.AppSettings["CompatibilityChecks"].Equals("ScriptOptionsOverrideNot"))
                {
                    _smoScriptOpts.CompatibilityChecks = _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsUseDefault");
                }
                else
                {
                    _smoScriptOpts.CompatibilityChecks = _smoScriptOpts.GetAppConfigCompabilitySetting();
                }
            }

            _smoScriptOpts.Migrate = !rbAnalyzeTraceFile.Checked;
        }

        private void rbScriptAll_CheckedChanged(object sender, EventArgs e)
        {
            tvDatabaseObjects.Enabled = !rbScriptAll.Checked;
            if (rbScriptAll.Checked == true)
            {
                btnSelectAll_Click(sender, e);
                btnSelectAll.Enabled = false;
                btnClearAll.Enabled = false;
                btnNext.Enabled = true;
            }
        }

        private void rbSpecificObjects_CheckedChanged(object sender, EventArgs e)
        {
            tvDatabaseObjects.Enabled = rbSpecificObjects.Checked;
            if (rbSpecificObjects.Checked == true)
            {
                btnClearAll_Click(sender, e);
                btnSelectAll.Enabled = true;
                btnClearAll.Enabled = true;
                btnNext.Enabled = false;
            }
        }

        private int ChildNodesSelected(TreeNode node)
        {
            if (node.Nodes.Count == 0) return 0;

            int cnt = 0;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Checked) cnt++;
            }
            return cnt;
        }

        private void tvDatabaseObjects_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode tn = e.Node;

            if (tn == null) return;

            if (tn.ForeColor == Color.Red && tn.Checked)
            {
                tn.Checked = false;
                return;
            }

            if (_IgnorCheck) return;

            _IgnorCheck = true;


            if (tn.Parent != null)
            {
                int cnt = ChildNodesSelected(tn.Parent);
                if (cnt > 0)
                {
                    tn.Parent.Checked = true;
                }
                else
                {
                    tn.Parent.Checked = false;
                }
            }
            else if (tn.Nodes.Count > 0)
            {
                foreach (TreeNode child in tn.Nodes)
                {
                    if (child.ForeColor == Color.Red) continue;

                    child.Checked = tn.Checked;
                }
            }

            // Ok, we need to see if any nodes are checked to figure out if we need to enable the next button
            if (tn.Checked)
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
                foreach (TreeNode top in tvDatabaseObjects.Nodes)
                {
                    if (top.Checked)
                    {
                        btnNext.Enabled = true;
                        break;
                    }
                }
            }

            _IgnorCheck = false;
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            AdvancedSettings advanced = new AdvancedSettings(_smoScriptOpts);
            DialogResult dr = advanced.ShowDialog(this);

            if (dr == DialogResult.OK)
            {
                _smoScriptOpts = advanced.GetOptions;
            }
        }

        private void lbTargetDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTargetDatabases.SelectedIndex < 0)
            {
                btnNext.Enabled = false;
                btnDeleteDatabase.Enabled = false;
            }
            else if (_TargetServerInfo.RootDatabase.Length > 0) // if this is specified, then user can't delete the database
            {
                if (!rbMaintenance.Checked) btnNext.Enabled = true;
                btnDeleteDatabase.Enabled = false;
            }
            else
            {
                if (!rbMaintenance.Checked) btnNext.Enabled = true;
                btnDeleteDatabase.Enabled = true;
            }
        }

        private void btnCreateDatabase_Click(object sender, EventArgs e)
        {
            if (splitTargetServers.Panel1Collapsed)
            {
                CreateDatabase cdForm = new CreateDatabase(_TargetServerInfo);
                DialogResult dr = cdForm.ShowDialog(this);

                if (dr == DialogResult.OK)
                {
                    lbTargetDatabases.Items.Add(cdForm.DatabaseName);
                    lbTargetDatabases.SelectedIndex = lbTargetDatabases.Items.Count - 1;
                    lbTargetDatabases.SelectedItem = cdForm.DatabaseName;
                }
            }
            else
            {
                FederationDetails fd = (FederationDetails)lbFederations.SelectedItem;
                FederationMemberDistribution member = (FederationMemberDistribution)lbFederationMembers.SelectedItem;
                FederationMemberCreate fmcForm = new FederationMemberCreate(_TargetServerInfo, fd);
                DialogResult dr = fmcForm.ShowDialog(this);

                if (dr == DialogResult.Cancel) return;

                using (SqlConnection connection = new SqlConnection(_TargetServerInfo.ConnectionStringRootDatabase))
                {
                    ConnectToFederatedServer(connection);
                    btnDeleteDatabase.Enabled = true;
                }
            }
        }

        private void btnDeleteDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                if (splitTargetServers.Panel1Collapsed)
                {
                    if (lbTargetDatabases.SelectedItem != null)
                    {
                        string database = lbTargetDatabases.SelectedItem.ToString();
                        if (MessageBox.Show(CommonFunc.FormatString(Properties.Resources.MessageAreYouSure, database), Properties.Resources.DeleteDatabase, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Retry.ExecuteRetryAction(() =>
                            {
                                using (SqlConnection connection = new SqlConnection(_TargetServerInfo.ConnectionStringRootDatabase))
                                {
                                    string tsql = "DROP DATABASE " + database;
                                    SqlHelper.ExecuteNonQuery(connection, CommandType.Text, tsql);
                                }
                                lbTargetDatabases.Items.RemoveAt(lbTargetDatabases.SelectedIndex);
                            });
                        }
                    }
                }
                else
                {
                    FederationDetails fd = (FederationDetails)lbFederations.SelectedItem;
                    FederationMemberDistribution member = (FederationMemberDistribution)lbFederationMembers.SelectedItem;
                    FederationMemberDrop fmdForm = new FederationMemberDrop(_TargetServerInfo, fd, member, lbFederationMembers.SelectedIndex);
                    DialogResult dr = fmdForm.ShowDialog(this);

                    if (dr == DialogResult.Cancel) return;

                    using (SqlConnection connection = new SqlConnection(_TargetServerInfo.ConnectionStringRootDatabase))
                    {
                        ConnectToFederatedServer(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.ShowException(this, ex);
            }
        }

        private void lbTargetDatabases_DoubleClick(object sender, EventArgs e)
        {
            if (!rbMaintenance.Checked && lbTargetDatabases.SelectedItem != null)
            {
                _Reset = true;
                btnNext_Click(sender, e);
            }
        }

        private void lbFederations_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            lbFederationMembers.Items.Clear();
            foreach (FederationMemberDistribution md in ((FederationDetails)lbFederations.SelectedItem).Members)
            {
                lbFederationMembers.Items.Add(md);
                lbFederationMembers.SetSelected(lbFederationMembers.Items.Count - 1, true);
            }
        }

        private void lbFederationMembers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!rbMaintenance.Checked && lbFederationMembers.SelectedIndices.Count > 0)
            {
                btnNext.Enabled = true;
            }
        }

        private void lbFederationMembers_DoubleClick(object sender, EventArgs e)
        {
            if (!rbMaintenance.Checked && lbFederationMembers.SelectedItem != null)
            {
                _Reset = true;
                btnNext_Click(sender, e);
            }
        }

        private void lbSourceFederations_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnNext.Enabled = false;
            lbDatabases.Items.Clear();
            foreach (FederationMemberDistribution md in ((FederationDetails)lbSourceFederations.SelectedItem).Members)
            {
                DatabaseInfo di = new DatabaseInfo();
                di.FederationMember = md;
                di.ConnectedTo = TypeOfConnection.SQLAzureFederation;
                di.IsDbOwner = true;
                lbDatabases.Items.Add(di);
            }
        }
    }
}
