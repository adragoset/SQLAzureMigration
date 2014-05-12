namespace SQLAzureMW
{
    partial class FederationMemberDrop
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FederationMemberDrop));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDrop = new System.Windows.Forms.Button();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.rbDropMember = new System.Windows.Forms.RadioButton();
            this.gbMerge = new System.Windows.Forms.GroupBox();
            this.rbHigh = new System.Windows.Forms.RadioButton();
            this.rbLow = new System.Windows.Forms.RadioButton();
            this.rbDropFederation = new System.Windows.Forms.RadioButton();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.gbOptions.SuspendLayout();
            this.gbMerge.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnDrop
            // 
            resources.ApplyResources(this.btnDrop, "btnDrop");
            this.btnDrop.Name = "btnDrop";
            this.btnDrop.UseVisualStyleBackColor = true;
            this.btnDrop.Click += new System.EventHandler(this.btnDrop_Click);
            // 
            // gbOptions
            // 
            resources.ApplyResources(this.gbOptions, "gbOptions");
            this.gbOptions.Controls.Add(this.rbDropMember);
            this.gbOptions.Controls.Add(this.gbMerge);
            this.gbOptions.Controls.Add(this.rbDropFederation);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.TabStop = false;
            // 
            // rbDropMember
            // 
            resources.ApplyResources(this.rbDropMember, "rbDropMember");
            this.rbDropMember.Name = "rbDropMember";
            this.rbDropMember.TabStop = true;
            this.rbDropMember.UseVisualStyleBackColor = true;
            this.rbDropMember.CheckedChanged += new System.EventHandler(this.rbDropMember_CheckedChanged);
            // 
            // gbMerge
            // 
            resources.ApplyResources(this.gbMerge, "gbMerge");
            this.gbMerge.Controls.Add(this.rbHigh);
            this.gbMerge.Controls.Add(this.rbLow);
            this.gbMerge.Name = "gbMerge";
            this.gbMerge.TabStop = false;
            // 
            // rbHigh
            // 
            resources.ApplyResources(this.rbHigh, "rbHigh");
            this.rbHigh.Name = "rbHigh";
            this.rbHigh.TabStop = true;
            this.rbHigh.UseVisualStyleBackColor = true;
            // 
            // rbLow
            // 
            resources.ApplyResources(this.rbLow, "rbLow");
            this.rbLow.Name = "rbLow";
            this.rbLow.TabStop = true;
            this.rbLow.UseVisualStyleBackColor = true;
            // 
            // rbDropFederation
            // 
            resources.ApplyResources(this.rbDropFederation, "rbDropFederation");
            this.rbDropFederation.Name = "rbDropFederation";
            this.rbDropFederation.TabStop = true;
            this.rbDropFederation.UseVisualStyleBackColor = true;
            this.rbDropFederation.CheckedChanged += new System.EventHandler(this.rbDropMember_CheckedChanged);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Click += new System.EventHandler(this.btnSee_Click);
            // 
            // FederationMemberDrop
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnDrop);
            this.Controls.Add(this.btnCancel);
            this.Name = "FederationMemberDrop";
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.gbMerge.ResumeLayout(false);
            this.gbMerge.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDrop;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.RadioButton rbHigh;
        private System.Windows.Forms.RadioButton rbLow;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RadioButton rbDropFederation;
        private System.Windows.Forms.RadioButton rbDropMember;
        private System.Windows.Forms.GroupBox gbMerge;
        private System.Windows.Forms.Label label1;
    }
}