namespace SQLAzureMW
{
    partial class CreateDatabase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateDatabase));
            this.gbDatabaseSize = new System.Windows.Forms.GroupBox();
            this.cbMaxDatabaseSize = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cbEdition = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbNewDatabase = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCreateDatabase = new System.Windows.Forms.Button();
            this.lbCollation = new System.Windows.Forms.Label();
            this.cbCollations = new System.Windows.Forms.ComboBox();
            this.gbDatabaseSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDatabaseSize
            // 
            resources.ApplyResources(this.gbDatabaseSize, "gbDatabaseSize");
            this.gbDatabaseSize.Controls.Add(this.cbMaxDatabaseSize);
            this.gbDatabaseSize.Controls.Add(this.label4);
            this.gbDatabaseSize.Controls.Add(this.label5);
            this.gbDatabaseSize.Controls.Add(this.cbEdition);
            this.gbDatabaseSize.Name = "gbDatabaseSize";
            this.gbDatabaseSize.TabStop = false;
            // 
            // cbMaxDatabaseSize
            // 
            resources.ApplyResources(this.cbMaxDatabaseSize, "cbMaxDatabaseSize");
            this.cbMaxDatabaseSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMaxDatabaseSize.FormattingEnabled = true;
            this.cbMaxDatabaseSize.Name = "cbMaxDatabaseSize";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // cbEdition
            // 
            resources.ApplyResources(this.cbEdition, "cbEdition");
            this.cbEdition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEdition.FormattingEnabled = true;
            this.cbEdition.Name = "cbEdition";
            this.cbEdition.SelectedIndexChanged += new System.EventHandler(this.cbEdition_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // tbNewDatabase
            // 
            resources.ApplyResources(this.tbNewDatabase, "tbNewDatabase");
            this.tbNewDatabase.Name = "tbNewDatabase";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            // 
            // btnCreateDatabase
            // 
            resources.ApplyResources(this.btnCreateDatabase, "btnCreateDatabase");
            this.btnCreateDatabase.Name = "btnCreateDatabase";
            this.btnCreateDatabase.Click += new System.EventHandler(this.btnCreateDatabase_Click);
            // 
            // lbCollation
            // 
            resources.ApplyResources(this.lbCollation, "lbCollation");
            this.lbCollation.Name = "lbCollation";
            // 
            // cbCollations
            // 
            resources.ApplyResources(this.cbCollations, "cbCollations");
            this.cbCollations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCollations.FormattingEnabled = true;
            this.cbCollations.Name = "cbCollations";
            this.cbCollations.SelectedIndexChanged += new System.EventHandler(this.cbCollations_SelectedIndexChanged);
            // 
            // CreateDatabase
            // 
            this.AcceptButton = this.btnCreateDatabase;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.cbCollations);
            this.Controls.Add(this.lbCollation);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreateDatabase);
            this.Controls.Add(this.tbNewDatabase);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gbDatabaseSize);
            this.Name = "CreateDatabase";
            this.Load += new System.EventHandler(this.CreateDatabase_Load);
            this.gbDatabaseSize.ResumeLayout(false);
            this.gbDatabaseSize.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDatabaseSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbNewDatabase;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCreateDatabase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbEdition;
        private System.Windows.Forms.ComboBox cbMaxDatabaseSize;
        private System.Windows.Forms.ComboBox cbCollations;
        private System.Windows.Forms.Label lbCollation;
    }
}