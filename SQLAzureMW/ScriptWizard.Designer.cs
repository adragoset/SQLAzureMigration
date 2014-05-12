namespace SQLAzureMW
{
    partial class ScriptWizard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptWizard));
            this.splitTargetServers = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gbSelectFederation = new System.Windows.Forms.GroupBox();
            this.lbFederations = new System.Windows.Forms.ListBox();
            this.gbSelectFederationMembers = new System.Windows.Forms.GroupBox();
            this.lbFederationMembers = new System.Windows.Forms.ListBox();
            this.lbTargetDatabases = new System.Windows.Forms.ListBox();
            this.splitDBDisplay = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbSourceFederations = new System.Windows.Forms.ListBox();
            this.gbSelectDatabase = new System.Windows.Forms.GroupBox();
            this.lbDatabases = new System.Windows.Forms.ListBox();
            this.lbAction = new System.Windows.Forms.Label();
            this.lbActionDesc = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.tvSummary = new System.Windows.Forms.TreeView();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panelTargetDatabase = new System.Windows.Forms.Panel();
            this.btnConnectTargetServer = new System.Windows.Forms.Button();
            this.btnCreateDatabase = new System.Windows.Forms.Button();
            this.btnDeleteDatabase = new System.Windows.Forms.Button();
            this.panelWizardOptions = new System.Windows.Forms.Panel();
            this.rbMaintenance = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lbProgram = new System.Windows.Forms.Label();
            this.panelFileToProcess = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSourceFile = new System.Windows.Forms.TextBox();
            this.btnFindFile = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.rbRunTSQLFile = new System.Windows.Forms.RadioButton();
            this.rbAnalyzeMigrateTSQLFile = new System.Windows.Forms.RadioButton();
            this.rbAnalyzeMigrateDatabase = new System.Windows.Forms.RadioButton();
            this.rbAnalyzeTraceFile = new System.Windows.Forms.RadioButton();
            this.panelResultsSummary = new System.Windows.Forms.Panel();
            this.lbResultsSummaryStatus = new System.Windows.Forms.Label();
            this.btnCancelProcessing = new System.Windows.Forms.Button();
            this.progressBarGenScript = new System.Windows.Forms.ProgressBar();
            this.btnSave = new System.Windows.Forms.Button();
            this.cbResultsScroll = new System.Windows.Forms.CheckBox();
            this.tabCtlResults = new System.Windows.Forms.TabControl();
            this.tabResultsSummary = new System.Windows.Forms.TabPage();
            this.rtbResultsSummary = new System.Windows.Forms.RichTextBox();
            this.tabSQLScript = new System.Windows.Forms.TabPage();
            this.rtbSQLScript = new System.Windows.Forms.RichTextBox();
            this.panelTargetResults = new System.Windows.Forms.Panel();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnSkip = new System.Windows.Forms.Button();
            this.btnRetry = new System.Windows.Forms.Button();
            this.tabCtlUploadStatus = new System.Windows.Forms.TabControl();
            this.tabResults = new System.Windows.Forms.TabPage();
            this.rtbTargetResults = new System.Windows.Forms.RichTextBox();
            this.btnSaveTargetResults = new System.Windows.Forms.Button();
            this.btnCancelTargetProcessing = new System.Windows.Forms.Button();
            this.cbAzureStatusScroll = new System.Windows.Forms.CheckBox();
            this.lbStatus = new System.Windows.Forms.Label();
            this.progressBarTargetServer = new System.Windows.Forms.ProgressBar();
            this.panelObjectTypes = new System.Windows.Forms.Panel();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.rbSpecificObjects = new System.Windows.Forms.RadioButton();
            this.rbScriptAll = new System.Windows.Forms.RadioButton();
            this.tvDatabaseObjects = new System.Windows.Forms.TreeView();
            this.panelTreeViewSummary = new System.Windows.Forms.Panel();
            this.panelDatabaseSource = new System.Windows.Forms.Panel();
            this.btnConnectToServer = new System.Windows.Forms.Button();
            this.splitTargetServers.Panel1.SuspendLayout();
            this.splitTargetServers.Panel2.SuspendLayout();
            this.splitTargetServers.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.gbSelectFederation.SuspendLayout();
            this.gbSelectFederationMembers.SuspendLayout();
            this.splitDBDisplay.Panel1.SuspendLayout();
            this.splitDBDisplay.Panel2.SuspendLayout();
            this.splitDBDisplay.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gbSelectDatabase.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panelTargetDatabase.SuspendLayout();
            this.panelWizardOptions.SuspendLayout();
            this.panelFileToProcess.SuspendLayout();
            this.panelResultsSummary.SuspendLayout();
            this.tabCtlResults.SuspendLayout();
            this.tabResultsSummary.SuspendLayout();
            this.tabSQLScript.SuspendLayout();
            this.panelTargetResults.SuspendLayout();
            this.tabCtlUploadStatus.SuspendLayout();
            this.tabResults.SuspendLayout();
            this.panelObjectTypes.SuspendLayout();
            this.panelTreeViewSummary.SuspendLayout();
            this.panelDatabaseSource.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitTargetServers
            // 
            resources.ApplyResources(this.splitTargetServers, "splitTargetServers");
            this.splitTargetServers.Name = "splitTargetServers";
            // 
            // splitTargetServers.Panel1
            // 
            resources.ApplyResources(this.splitTargetServers.Panel1, "splitTargetServers.Panel1");
            this.splitTargetServers.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitTargetServers.Panel2
            // 
            resources.ApplyResources(this.splitTargetServers.Panel2, "splitTargetServers.Panel2");
            this.splitTargetServers.Panel2.Controls.Add(this.lbTargetDatabases);
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.Controls.Add(this.gbSelectFederation);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.gbSelectFederationMembers);
            // 
            // gbSelectFederation
            // 
            resources.ApplyResources(this.gbSelectFederation, "gbSelectFederation");
            this.gbSelectFederation.Controls.Add(this.lbFederations);
            this.gbSelectFederation.Name = "gbSelectFederation";
            this.gbSelectFederation.TabStop = false;
            // 
            // lbFederations
            // 
            resources.ApplyResources(this.lbFederations, "lbFederations");
            this.lbFederations.FormattingEnabled = true;
            this.lbFederations.Name = "lbFederations";
            this.lbFederations.SelectedIndexChanged += new System.EventHandler(this.lbFederations_SelectedIndexChanged);
            // 
            // gbSelectFederationMembers
            // 
            resources.ApplyResources(this.gbSelectFederationMembers, "gbSelectFederationMembers");
            this.gbSelectFederationMembers.Controls.Add(this.lbFederationMembers);
            this.gbSelectFederationMembers.Name = "gbSelectFederationMembers";
            this.gbSelectFederationMembers.TabStop = false;
            // 
            // lbFederationMembers
            // 
            resources.ApplyResources(this.lbFederationMembers, "lbFederationMembers");
            this.lbFederationMembers.FormattingEnabled = true;
            this.lbFederationMembers.Name = "lbFederationMembers";
            this.lbFederationMembers.SelectedIndexChanged += new System.EventHandler(this.lbFederationMembers_SelectedIndexChanged);
            this.lbFederationMembers.DoubleClick += new System.EventHandler(this.lbFederationMembers_DoubleClick);
            // 
            // lbTargetDatabases
            // 
            resources.ApplyResources(this.lbTargetDatabases, "lbTargetDatabases");
            this.lbTargetDatabases.FormattingEnabled = true;
            this.lbTargetDatabases.Name = "lbTargetDatabases";
            this.lbTargetDatabases.SelectedIndexChanged += new System.EventHandler(this.lbTargetDatabases_SelectedIndexChanged);
            this.lbTargetDatabases.DoubleClick += new System.EventHandler(this.lbTargetDatabases_DoubleClick);
            // 
            // splitDBDisplay
            // 
            resources.ApplyResources(this.splitDBDisplay, "splitDBDisplay");
            this.splitDBDisplay.Name = "splitDBDisplay";
            // 
            // splitDBDisplay.Panel1
            // 
            resources.ApplyResources(this.splitDBDisplay.Panel1, "splitDBDisplay.Panel1");
            this.splitDBDisplay.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitDBDisplay.Panel2
            // 
            resources.ApplyResources(this.splitDBDisplay.Panel2, "splitDBDisplay.Panel2");
            this.splitDBDisplay.Panel2.Controls.Add(this.gbSelectDatabase);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.lbSourceFederations);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // lbSourceFederations
            // 
            resources.ApplyResources(this.lbSourceFederations, "lbSourceFederations");
            this.lbSourceFederations.FormattingEnabled = true;
            this.lbSourceFederations.Name = "lbSourceFederations";
            this.lbSourceFederations.SelectedIndexChanged += new System.EventHandler(this.lbSourceFederations_SelectedIndexChanged);
            // 
            // gbSelectDatabase
            // 
            resources.ApplyResources(this.gbSelectDatabase, "gbSelectDatabase");
            this.gbSelectDatabase.Controls.Add(this.lbDatabases);
            this.gbSelectDatabase.Name = "gbSelectDatabase";
            this.gbSelectDatabase.TabStop = false;
            // 
            // lbDatabases
            // 
            resources.ApplyResources(this.lbDatabases, "lbDatabases");
            this.lbDatabases.FormattingEnabled = true;
            this.lbDatabases.Name = "lbDatabases";
            this.lbDatabases.SelectedIndexChanged += new System.EventHandler(this.lbDatabases_SelectedIndexChanged);
            this.lbDatabases.DoubleClick += new System.EventHandler(this.lbDatabases_DoubleClick);
            // 
            // lbAction
            // 
            resources.ApplyResources(this.lbAction, "lbAction");
            this.lbAction.Name = "lbAction";
            // 
            // lbActionDesc
            // 
            resources.ApplyResources(this.lbActionDesc, "lbActionDesc");
            this.lbActionDesc.Name = "lbActionDesc";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.lbAction);
            this.panel1.Controls.Add(this.lbActionDesc);
            this.panel1.Name = "panel1";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnNext);
            this.panel2.Controls.Add(this.btnBack);
            this.panel2.Name = "panel2";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnNext
            // 
            resources.ApplyResources(this.btnNext, "btnNext");
            this.btnNext.Name = "btnNext";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnBack
            // 
            resources.ApplyResources(this.btnBack, "btnBack");
            this.btnBack.Name = "btnBack";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnClearAll
            // 
            resources.ApplyResources(this.btnClearAll, "btnClearAll");
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // btnSelectAll
            // 
            resources.ApplyResources(this.btnSelectAll, "btnSelectAll");
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // tvSummary
            // 
            resources.ApplyResources(this.tvSummary, "tvSummary");
            this.tvSummary.Name = "tvSummary";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.panelTargetDatabase);
            this.panel3.Controls.Add(this.panelWizardOptions);
            this.panel3.Controls.Add(this.panelResultsSummary);
            this.panel3.Controls.Add(this.panelTargetResults);
            this.panel3.Controls.Add(this.panelObjectTypes);
            this.panel3.Controls.Add(this.panelTreeViewSummary);
            this.panel3.Controls.Add(this.panelDatabaseSource);
            this.panel3.Name = "panel3";
            // 
            // panelTargetDatabase
            // 
            resources.ApplyResources(this.panelTargetDatabase, "panelTargetDatabase");
            this.panelTargetDatabase.BackColor = System.Drawing.Color.White;
            this.panelTargetDatabase.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTargetDatabase.Controls.Add(this.splitTargetServers);
            this.panelTargetDatabase.Controls.Add(this.btnConnectTargetServer);
            this.panelTargetDatabase.Controls.Add(this.btnCreateDatabase);
            this.panelTargetDatabase.Controls.Add(this.btnDeleteDatabase);
            this.panelTargetDatabase.Name = "panelTargetDatabase";
            // 
            // btnConnectTargetServer
            // 
            resources.ApplyResources(this.btnConnectTargetServer, "btnConnectTargetServer");
            this.btnConnectTargetServer.Name = "btnConnectTargetServer";
            this.btnConnectTargetServer.UseVisualStyleBackColor = true;
            this.btnConnectTargetServer.Click += new System.EventHandler(this.btnConnectTargetServer_Click);
            // 
            // btnCreateDatabase
            // 
            resources.ApplyResources(this.btnCreateDatabase, "btnCreateDatabase");
            this.btnCreateDatabase.Name = "btnCreateDatabase";
            this.btnCreateDatabase.UseVisualStyleBackColor = true;
            this.btnCreateDatabase.Click += new System.EventHandler(this.btnCreateDatabase_Click);
            // 
            // btnDeleteDatabase
            // 
            resources.ApplyResources(this.btnDeleteDatabase, "btnDeleteDatabase");
            this.btnDeleteDatabase.Name = "btnDeleteDatabase";
            this.btnDeleteDatabase.UseVisualStyleBackColor = true;
            this.btnDeleteDatabase.Click += new System.EventHandler(this.btnDeleteDatabase_Click);
            // 
            // panelWizardOptions
            // 
            resources.ApplyResources(this.panelWizardOptions, "panelWizardOptions");
            this.panelWizardOptions.BackColor = System.Drawing.Color.White;
            this.panelWizardOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWizardOptions.Controls.Add(this.rbMaintenance);
            this.panelWizardOptions.Controls.Add(this.label1);
            this.panelWizardOptions.Controls.Add(this.textBox1);
            this.panelWizardOptions.Controls.Add(this.lbProgram);
            this.panelWizardOptions.Controls.Add(this.panelFileToProcess);
            this.panelWizardOptions.Controls.Add(this.label6);
            this.panelWizardOptions.Controls.Add(this.label4);
            this.panelWizardOptions.Controls.Add(this.label3);
            this.panelWizardOptions.Controls.Add(this.rbRunTSQLFile);
            this.panelWizardOptions.Controls.Add(this.rbAnalyzeMigrateTSQLFile);
            this.panelWizardOptions.Controls.Add(this.rbAnalyzeMigrateDatabase);
            this.panelWizardOptions.Controls.Add(this.rbAnalyzeTraceFile);
            this.panelWizardOptions.Name = "panelWizardOptions";
            // 
            // rbMaintenance
            // 
            resources.ApplyResources(this.rbMaintenance, "rbMaintenance");
            this.rbMaintenance.Name = "rbMaintenance";
            this.rbMaintenance.TabStop = true;
            this.rbMaintenance.UseVisualStyleBackColor = true;
            this.rbMaintenance.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Name = "textBox1";
            // 
            // lbProgram
            // 
            resources.ApplyResources(this.lbProgram, "lbProgram");
            this.lbProgram.Name = "lbProgram";
            // 
            // panelFileToProcess
            // 
            resources.ApplyResources(this.panelFileToProcess, "panelFileToProcess");
            this.panelFileToProcess.Controls.Add(this.label2);
            this.panelFileToProcess.Controls.Add(this.tbSourceFile);
            this.panelFileToProcess.Controls.Add(this.btnFindFile);
            this.panelFileToProcess.Name = "panelFileToProcess";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // tbSourceFile
            // 
            resources.ApplyResources(this.tbSourceFile, "tbSourceFile");
            this.tbSourceFile.Name = "tbSourceFile";
            this.tbSourceFile.TextChanged += new System.EventHandler(this.tbOutputFile_TextChanged);
            // 
            // btnFindFile
            // 
            resources.ApplyResources(this.btnFindFile, "btnFindFile");
            this.btnFindFile.Name = "btnFindFile";
            this.btnFindFile.UseVisualStyleBackColor = true;
            this.btnFindFile.Click += new System.EventHandler(this.btnFindFile_Click);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // rbRunTSQLFile
            // 
            resources.ApplyResources(this.rbRunTSQLFile, "rbRunTSQLFile");
            this.rbRunTSQLFile.Name = "rbRunTSQLFile";
            this.rbRunTSQLFile.TabStop = true;
            this.rbRunTSQLFile.UseVisualStyleBackColor = true;
            this.rbRunTSQLFile.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbAnalyzeMigrateTSQLFile
            // 
            resources.ApplyResources(this.rbAnalyzeMigrateTSQLFile, "rbAnalyzeMigrateTSQLFile");
            this.rbAnalyzeMigrateTSQLFile.Name = "rbAnalyzeMigrateTSQLFile";
            this.rbAnalyzeMigrateTSQLFile.TabStop = true;
            this.rbAnalyzeMigrateTSQLFile.UseVisualStyleBackColor = true;
            this.rbAnalyzeMigrateTSQLFile.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbAnalyzeMigrateDatabase
            // 
            resources.ApplyResources(this.rbAnalyzeMigrateDatabase, "rbAnalyzeMigrateDatabase");
            this.rbAnalyzeMigrateDatabase.Name = "rbAnalyzeMigrateDatabase";
            this.rbAnalyzeMigrateDatabase.TabStop = true;
            this.rbAnalyzeMigrateDatabase.UseVisualStyleBackColor = true;
            this.rbAnalyzeMigrateDatabase.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbAnalyzeTraceFile
            // 
            resources.ApplyResources(this.rbAnalyzeTraceFile, "rbAnalyzeTraceFile");
            this.rbAnalyzeTraceFile.Name = "rbAnalyzeTraceFile";
            this.rbAnalyzeTraceFile.TabStop = true;
            this.rbAnalyzeTraceFile.UseVisualStyleBackColor = true;
            this.rbAnalyzeTraceFile.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // panelResultsSummary
            // 
            resources.ApplyResources(this.panelResultsSummary, "panelResultsSummary");
            this.panelResultsSummary.BackColor = System.Drawing.Color.White;
            this.panelResultsSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelResultsSummary.Controls.Add(this.lbResultsSummaryStatus);
            this.panelResultsSummary.Controls.Add(this.btnCancelProcessing);
            this.panelResultsSummary.Controls.Add(this.progressBarGenScript);
            this.panelResultsSummary.Controls.Add(this.btnSave);
            this.panelResultsSummary.Controls.Add(this.cbResultsScroll);
            this.panelResultsSummary.Controls.Add(this.tabCtlResults);
            this.panelResultsSummary.Name = "panelResultsSummary";
            // 
            // lbResultsSummaryStatus
            // 
            resources.ApplyResources(this.lbResultsSummaryStatus, "lbResultsSummaryStatus");
            this.lbResultsSummaryStatus.Name = "lbResultsSummaryStatus";
            // 
            // btnCancelProcessing
            // 
            resources.ApplyResources(this.btnCancelProcessing, "btnCancelProcessing");
            this.btnCancelProcessing.Name = "btnCancelProcessing";
            this.btnCancelProcessing.UseVisualStyleBackColor = true;
            this.btnCancelProcessing.Click += new System.EventHandler(this.btnCancelProcessing_Click);
            // 
            // progressBarGenScript
            // 
            resources.ApplyResources(this.progressBarGenScript, "progressBarGenScript");
            this.progressBarGenScript.Name = "progressBarGenScript";
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cbResultsScroll
            // 
            resources.ApplyResources(this.cbResultsScroll, "cbResultsScroll");
            this.cbResultsScroll.Name = "cbResultsScroll";
            this.cbResultsScroll.UseVisualStyleBackColor = true;
            // 
            // tabCtlResults
            // 
            resources.ApplyResources(this.tabCtlResults, "tabCtlResults");
            this.tabCtlResults.Controls.Add(this.tabResultsSummary);
            this.tabCtlResults.Controls.Add(this.tabSQLScript);
            this.tabCtlResults.Name = "tabCtlResults";
            this.tabCtlResults.SelectedIndex = 0;
            // 
            // tabResultsSummary
            // 
            resources.ApplyResources(this.tabResultsSummary, "tabResultsSummary");
            this.tabResultsSummary.Controls.Add(this.rtbResultsSummary);
            this.tabResultsSummary.Name = "tabResultsSummary";
            this.tabResultsSummary.UseVisualStyleBackColor = true;
            // 
            // rtbResultsSummary
            // 
            resources.ApplyResources(this.rtbResultsSummary, "rtbResultsSummary");
            this.rtbResultsSummary.Name = "rtbResultsSummary";
            this.rtbResultsSummary.ReadOnly = true;
            this.rtbResultsSummary.VScroll += new System.EventHandler(this.rtbResultsSummary_VScroll);
            // 
            // tabSQLScript
            // 
            resources.ApplyResources(this.tabSQLScript, "tabSQLScript");
            this.tabSQLScript.Controls.Add(this.rtbSQLScript);
            this.tabSQLScript.Name = "tabSQLScript";
            this.tabSQLScript.UseVisualStyleBackColor = true;
            // 
            // rtbSQLScript
            // 
            resources.ApplyResources(this.rtbSQLScript, "rtbSQLScript");
            this.rtbSQLScript.ForeColor = System.Drawing.SystemColors.WindowText;
            this.rtbSQLScript.Name = "rtbSQLScript";
            this.rtbSQLScript.VScroll += new System.EventHandler(this.rtbSQLScript_VScroll);
            // 
            // panelTargetResults
            // 
            resources.ApplyResources(this.panelTargetResults, "panelTargetResults");
            this.panelTargetResults.BackColor = System.Drawing.Color.White;
            this.panelTargetResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTargetResults.Controls.Add(this.btnEdit);
            this.panelTargetResults.Controls.Add(this.btnSkip);
            this.panelTargetResults.Controls.Add(this.btnRetry);
            this.panelTargetResults.Controls.Add(this.tabCtlUploadStatus);
            this.panelTargetResults.Controls.Add(this.btnSaveTargetResults);
            this.panelTargetResults.Controls.Add(this.btnCancelTargetProcessing);
            this.panelTargetResults.Controls.Add(this.cbAzureStatusScroll);
            this.panelTargetResults.Controls.Add(this.lbStatus);
            this.panelTargetResults.Controls.Add(this.progressBarTargetServer);
            this.panelTargetResults.Name = "panelTargetResults";
            // 
            // btnEdit
            // 
            resources.ApplyResources(this.btnEdit, "btnEdit");
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnSkip
            // 
            resources.ApplyResources(this.btnSkip, "btnSkip");
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.UseVisualStyleBackColor = true;
            this.btnSkip.Click += new System.EventHandler(this.btnSkip_Click);
            // 
            // btnRetry
            // 
            resources.ApplyResources(this.btnRetry, "btnRetry");
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.UseVisualStyleBackColor = true;
            this.btnRetry.Click += new System.EventHandler(this.btnRetry_Click);
            // 
            // tabCtlUploadStatus
            // 
            resources.ApplyResources(this.tabCtlUploadStatus, "tabCtlUploadStatus");
            this.tabCtlUploadStatus.Controls.Add(this.tabResults);
            this.tabCtlUploadStatus.Name = "tabCtlUploadStatus";
            this.tabCtlUploadStatus.SelectedIndex = 0;
            this.tabCtlUploadStatus.SelectedIndexChanged += new System.EventHandler(this.tabCtlUploadStatus_SelectedIndexChanged);
            // 
            // tabResults
            // 
            resources.ApplyResources(this.tabResults, "tabResults");
            this.tabResults.Controls.Add(this.rtbTargetResults);
            this.tabResults.Name = "tabResults";
            this.tabResults.UseVisualStyleBackColor = true;
            // 
            // rtbTargetResults
            // 
            resources.ApplyResources(this.rtbTargetResults, "rtbTargetResults");
            this.rtbTargetResults.BackColor = System.Drawing.SystemColors.Window;
            this.rtbTargetResults.Name = "rtbTargetResults";
            this.rtbTargetResults.VScroll += new System.EventHandler(this.rtbAzureStatus_VScroll);
            // 
            // btnSaveTargetResults
            // 
            resources.ApplyResources(this.btnSaveTargetResults, "btnSaveTargetResults");
            this.btnSaveTargetResults.Name = "btnSaveTargetResults";
            this.btnSaveTargetResults.UseVisualStyleBackColor = true;
            this.btnSaveTargetResults.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancelTargetProcessing
            // 
            resources.ApplyResources(this.btnCancelTargetProcessing, "btnCancelTargetProcessing");
            this.btnCancelTargetProcessing.Name = "btnCancelTargetProcessing";
            this.btnCancelTargetProcessing.UseVisualStyleBackColor = true;
            this.btnCancelTargetProcessing.Click += new System.EventHandler(this.btnCancelTargetProcessing_Click);
            // 
            // cbAzureStatusScroll
            // 
            resources.ApplyResources(this.cbAzureStatusScroll, "cbAzureStatusScroll");
            this.cbAzureStatusScroll.Name = "cbAzureStatusScroll";
            this.cbAzureStatusScroll.UseVisualStyleBackColor = true;
            // 
            // lbStatus
            // 
            resources.ApplyResources(this.lbStatus, "lbStatus");
            this.lbStatus.Name = "lbStatus";
            // 
            // progressBarTargetServer
            // 
            resources.ApplyResources(this.progressBarTargetServer, "progressBarTargetServer");
            this.progressBarTargetServer.Name = "progressBarTargetServer";
            // 
            // panelObjectTypes
            // 
            resources.ApplyResources(this.panelObjectTypes, "panelObjectTypes");
            this.panelObjectTypes.BackColor = System.Drawing.Color.White;
            this.panelObjectTypes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelObjectTypes.Controls.Add(this.btnAdvanced);
            this.panelObjectTypes.Controls.Add(this.rbSpecificObjects);
            this.panelObjectTypes.Controls.Add(this.rbScriptAll);
            this.panelObjectTypes.Controls.Add(this.tvDatabaseObjects);
            this.panelObjectTypes.Controls.Add(this.btnSelectAll);
            this.panelObjectTypes.Controls.Add(this.btnClearAll);
            this.panelObjectTypes.Name = "panelObjectTypes";
            // 
            // btnAdvanced
            // 
            resources.ApplyResources(this.btnAdvanced, "btnAdvanced");
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // rbSpecificObjects
            // 
            resources.ApplyResources(this.rbSpecificObjects, "rbSpecificObjects");
            this.rbSpecificObjects.Name = "rbSpecificObjects";
            this.rbSpecificObjects.UseVisualStyleBackColor = true;
            this.rbSpecificObjects.CheckedChanged += new System.EventHandler(this.rbSpecificObjects_CheckedChanged);
            // 
            // rbScriptAll
            // 
            resources.ApplyResources(this.rbScriptAll, "rbScriptAll");
            this.rbScriptAll.Checked = true;
            this.rbScriptAll.Name = "rbScriptAll";
            this.rbScriptAll.TabStop = true;
            this.rbScriptAll.UseVisualStyleBackColor = true;
            this.rbScriptAll.CheckedChanged += new System.EventHandler(this.rbScriptAll_CheckedChanged);
            // 
            // tvDatabaseObjects
            // 
            resources.ApplyResources(this.tvDatabaseObjects, "tvDatabaseObjects");
            this.tvDatabaseObjects.CheckBoxes = true;
            this.tvDatabaseObjects.Name = "tvDatabaseObjects";
            this.tvDatabaseObjects.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvDatabaseObjects_AfterCheck);
            // 
            // panelTreeViewSummary
            // 
            resources.ApplyResources(this.panelTreeViewSummary, "panelTreeViewSummary");
            this.panelTreeViewSummary.Controls.Add(this.tvSummary);
            this.panelTreeViewSummary.Name = "panelTreeViewSummary";
            // 
            // panelDatabaseSource
            // 
            resources.ApplyResources(this.panelDatabaseSource, "panelDatabaseSource");
            this.panelDatabaseSource.BackColor = System.Drawing.Color.White;
            this.panelDatabaseSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDatabaseSource.Controls.Add(this.splitDBDisplay);
            this.panelDatabaseSource.Controls.Add(this.btnConnectToServer);
            this.panelDatabaseSource.Name = "panelDatabaseSource";
            // 
            // btnConnectToServer
            // 
            resources.ApplyResources(this.btnConnectToServer, "btnConnectToServer");
            this.btnConnectToServer.Name = "btnConnectToServer";
            this.btnConnectToServer.UseVisualStyleBackColor = true;
            this.btnConnectToServer.Click += new System.EventHandler(this.btnConnectToServer_Click);
            // 
            // ScriptWizard
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Name = "ScriptWizard";
            this.Load += new System.EventHandler(this.ScriptWizard_Load);
            this.splitTargetServers.Panel1.ResumeLayout(false);
            this.splitTargetServers.Panel2.ResumeLayout(false);
            this.splitTargetServers.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.gbSelectFederation.ResumeLayout(false);
            this.gbSelectFederationMembers.ResumeLayout(false);
            this.splitDBDisplay.Panel1.ResumeLayout(false);
            this.splitDBDisplay.Panel2.ResumeLayout(false);
            this.splitDBDisplay.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.gbSelectDatabase.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panelTargetDatabase.ResumeLayout(false);
            this.panelWizardOptions.ResumeLayout(false);
            this.panelWizardOptions.PerformLayout();
            this.panelFileToProcess.ResumeLayout(false);
            this.panelFileToProcess.PerformLayout();
            this.panelResultsSummary.ResumeLayout(false);
            this.panelResultsSummary.PerformLayout();
            this.tabCtlResults.ResumeLayout(false);
            this.tabResultsSummary.ResumeLayout(false);
            this.tabSQLScript.ResumeLayout(false);
            this.panelTargetResults.ResumeLayout(false);
            this.panelTargetResults.PerformLayout();
            this.tabCtlUploadStatus.ResumeLayout(false);
            this.tabResults.ResumeLayout(false);
            this.panelObjectTypes.ResumeLayout(false);
            this.panelObjectTypes.PerformLayout();
            this.panelTreeViewSummary.ResumeLayout(false);
            this.panelDatabaseSource.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbAction;
        private System.Windows.Forms.Label lbActionDesc;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.ListBox lbDatabases;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.TreeView tvSummary;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TabControl tabCtlResults;
        private System.Windows.Forms.TabPage tabResultsSummary;
        private System.Windows.Forms.RichTextBox rtbResultsSummary;
        private System.Windows.Forms.TabPage tabSQLScript;
        private System.Windows.Forms.RichTextBox rtbSQLScript;
        private System.Windows.Forms.Panel panelDatabaseSource;
        private System.Windows.Forms.Button btnConnectToServer;
        private System.Windows.Forms.TextBox tbSourceFile;
        private System.Windows.Forms.Button btnFindFile;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panelTreeViewSummary;
        private System.Windows.Forms.ProgressBar progressBarGenScript;
        private System.Windows.Forms.Panel panelObjectTypes;
        private System.Windows.Forms.Panel panelTargetResults;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.ProgressBar progressBarTargetServer;
        private System.Windows.Forms.RichTextBox rtbTargetResults;
        private System.Windows.Forms.Label lbResultsSummaryStatus;
        private System.Windows.Forms.CheckBox cbAzureStatusScroll;
        private System.Windows.Forms.Button btnCancelTargetProcessing;
        private System.Windows.Forms.Panel panelResultsSummary;
        private System.Windows.Forms.CheckBox cbResultsScroll;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnSaveTargetResults;
        private System.Windows.Forms.Button btnCancelProcessing;
        private System.Windows.Forms.Panel panelWizardOptions;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rbRunTSQLFile;
        private System.Windows.Forms.RadioButton rbAnalyzeMigrateTSQLFile;
        private System.Windows.Forms.RadioButton rbAnalyzeMigrateDatabase;
        private System.Windows.Forms.RadioButton rbAnalyzeTraceFile;
        private System.Windows.Forms.TreeView tvDatabaseObjects;
        private System.Windows.Forms.RadioButton rbSpecificObjects;
        private System.Windows.Forms.RadioButton rbScriptAll;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.Panel panelFileToProcess;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbProgram;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panelTargetDatabase;
        private System.Windows.Forms.Button btnDeleteDatabase;
        private System.Windows.Forms.ListBox lbTargetDatabases;
        private System.Windows.Forms.Button btnCreateDatabase;
        private System.Windows.Forms.Button btnConnectTargetServer;
        private System.Windows.Forms.TabControl tabCtlUploadStatus;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnSkip;
        private System.Windows.Forms.Button btnRetry;
        private System.Windows.Forms.GroupBox gbSelectFederationMembers;
        private System.Windows.Forms.ListBox lbFederationMembers;
        private System.Windows.Forms.GroupBox gbSelectFederation;
        private System.Windows.Forms.ListBox lbFederations;
        private System.Windows.Forms.SplitContainer splitDBDisplay;
        private System.Windows.Forms.ListBox lbSourceFederations;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbSelectDatabase;
        private System.Windows.Forms.SplitContainer splitTargetServers;
        private System.Windows.Forms.RadioButton rbMaintenance;
        private System.Windows.Forms.Label label1;
    }
}