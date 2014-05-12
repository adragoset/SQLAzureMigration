namespace SQLAzureMW
{
    partial class FederationMemberCreate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FederationMemberCreate));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.tbSplitpoint = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControlCreate = new System.Windows.Forms.TabControl();
            this.tabCreateFederation = new System.Windows.Forms.TabPage();
            this.numericUpDownVarbinary = new System.Windows.Forms.NumericUpDown();
            this.cbDistributionType = new System.Windows.Forms.ComboBox();
            this.cbDistributionDataType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbDistributionName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFederationName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabCreateFederationMember = new System.Windows.Forms.TabPage();
            this.tbDistributionDataType = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControlCreate.SuspendLayout();
            this.tabCreateFederation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVarbinary)).BeginInit();
            this.tabCreateFederationMember.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // tbSplitpoint
            // 
            resources.ApplyResources(this.tbSplitpoint, "tbSplitpoint");
            this.tbSplitpoint.BackColor = System.Drawing.Color.White;
            this.tbSplitpoint.Name = "tbSplitpoint";
            // 
            // btnCreate
            // 
            resources.ApplyResources(this.btnCreate, "btnCreate");
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tabControlCreate
            // 
            resources.ApplyResources(this.tabControlCreate, "tabControlCreate");
            this.tabControlCreate.Controls.Add(this.tabCreateFederation);
            this.tabControlCreate.Controls.Add(this.tabCreateFederationMember);
            this.tabControlCreate.Name = "tabControlCreate";
            this.tabControlCreate.SelectedIndex = 0;
            this.tabControlCreate.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControlCreate_Selected);
            // 
            // tabCreateFederation
            // 
            resources.ApplyResources(this.tabCreateFederation, "tabCreateFederation");
            this.tabCreateFederation.Controls.Add(this.numericUpDownVarbinary);
            this.tabCreateFederation.Controls.Add(this.cbDistributionType);
            this.tabCreateFederation.Controls.Add(this.cbDistributionDataType);
            this.tabCreateFederation.Controls.Add(this.label4);
            this.tabCreateFederation.Controls.Add(this.tbDistributionName);
            this.tabCreateFederation.Controls.Add(this.label3);
            this.tabCreateFederation.Controls.Add(this.label2);
            this.tabCreateFederation.Controls.Add(this.tbFederationName);
            this.tabCreateFederation.Controls.Add(this.label1);
            this.tabCreateFederation.Name = "tabCreateFederation";
            this.tabCreateFederation.UseVisualStyleBackColor = true;
            // 
            // numericUpDownVarbinary
            // 
            resources.ApplyResources(this.numericUpDownVarbinary, "numericUpDownVarbinary");
            this.numericUpDownVarbinary.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.numericUpDownVarbinary.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownVarbinary.Name = "numericUpDownVarbinary";
            this.numericUpDownVarbinary.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cbDistributionType
            // 
            resources.ApplyResources(this.cbDistributionType, "cbDistributionType");
            this.cbDistributionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDistributionType.FormattingEnabled = true;
            this.cbDistributionType.Items.AddRange(new object[] {
            resources.GetString("cbDistributionType.Items")});
            this.cbDistributionType.Name = "cbDistributionType";
            // 
            // cbDistributionDataType
            // 
            resources.ApplyResources(this.cbDistributionDataType, "cbDistributionDataType");
            this.cbDistributionDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDistributionDataType.FormattingEnabled = true;
            this.cbDistributionDataType.Items.AddRange(new object[] {
            resources.GetString("cbDistributionDataType.Items"),
            resources.GetString("cbDistributionDataType.Items1"),
            resources.GetString("cbDistributionDataType.Items2"),
            resources.GetString("cbDistributionDataType.Items3")});
            this.cbDistributionDataType.Name = "cbDistributionDataType";
            this.cbDistributionDataType.SelectedIndexChanged += new System.EventHandler(this.cbFederationType_SelectedIndexChanged);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // tbDistributionName
            // 
            resources.ApplyResources(this.tbDistributionName, "tbDistributionName");
            this.tbDistributionName.Name = "tbDistributionName";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // tbFederationName
            // 
            resources.ApplyResources(this.tbFederationName, "tbFederationName");
            this.tbFederationName.Name = "tbFederationName";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // tabCreateFederationMember
            // 
            resources.ApplyResources(this.tabCreateFederationMember, "tabCreateFederationMember");
            this.tabCreateFederationMember.BackColor = System.Drawing.Color.Transparent;
            this.tabCreateFederationMember.Controls.Add(this.tbDistributionDataType);
            this.tabCreateFederationMember.Controls.Add(this.label6);
            this.tabCreateFederationMember.Controls.Add(this.label5);
            this.tabCreateFederationMember.Controls.Add(this.tbSplitpoint);
            this.tabCreateFederationMember.Name = "tabCreateFederationMember";
            // 
            // tbDistributionDataType
            // 
            resources.ApplyResources(this.tbDistributionDataType, "tbDistributionDataType");
            this.tbDistributionDataType.Name = "tbDistributionDataType";
            this.tbDistributionDataType.ReadOnly = true;
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // FederationMemberCreate
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlCreate);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.progressBar1);
            this.Name = "FederationMemberCreate";
            this.tabControlCreate.ResumeLayout(false);
            this.tabCreateFederation.ResumeLayout(false);
            this.tabCreateFederation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownVarbinary)).EndInit();
            this.tabCreateFederationMember.ResumeLayout(false);
            this.tabCreateFederationMember.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox tbSplitpoint;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabControl tabControlCreate;
        private System.Windows.Forms.TabPage tabCreateFederation;
        private System.Windows.Forms.TabPage tabCreateFederationMember;
        private System.Windows.Forms.ComboBox cbDistributionType;
        private System.Windows.Forms.ComboBox cbDistributionDataType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbDistributionName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFederationName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownVarbinary;
        private System.Windows.Forms.TextBox tbDistributionDataType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
    }
}