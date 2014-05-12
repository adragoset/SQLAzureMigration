using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using SQLAzureMWUtils;
using System.Text.RegularExpressions;

namespace SQLAzureMW
{
    public partial class CreateDatabase : Form
    {
        private TargetServerInfo _TargetServerInfo = null;
        private string _DatabaseName;

        public string DatabaseName
        {
            get { return _DatabaseName; }
            set { _DatabaseName = value; }
        }

        public CreateDatabase(TargetServerInfo TargetServer)
        {
            Init();

            _TargetServerInfo = TargetServer;
            if (_TargetServerInfo.ServerType == ServerTypes.SQLAzure)
            {
                gbDatabaseSize.Enabled = true;
            }
            else
            {
                gbDatabaseSize.Enabled = false;
            }
        }

        private void Init()
        {
            InitializeComponent();
            cbEdition.Items.Add(Properties.Resources.CreateDatabaseWebEdition);
            cbEdition.Items.Add(Properties.Resources.CreateDatabaseBusinessEdition);
            cbEdition.SelectedIndex = 0;

            List<Collation> colList = CollationHelper.GetCollationList();
            cbCollations.DataSource = colList;
            string dbCol = CommonFunc.GetAppSettingsStringValue("DBCollation");
            if (dbCol.Length == 0) return;

            int index = 0;
            foreach (Collation col in colList)
            {
                if (dbCol.Equals(col.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    cbCollations.SelectedIndex = index;
                    break;
                }
                ++index;
            }
        }

        private void btnCreateDatabase_Click(object sender, EventArgs e)
        {
            string edition = cbEdition.SelectedIndex == 0 ? "web" : "business";
            int dbSize = 1;

            if (tbNewDatabase.Text.Length == 0)
            {
                MessageBox.Show(label1.Text, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbNewDatabase.Focus();
                return;
            }

            string sizeofdb = Regex.Match(cbMaxDatabaseSize.SelectedItem.ToString(), "[0-9]*").Value;
            if (sizeofdb.Length == 0)
            {
                cbMaxDatabaseSize.Focus();
                return;
            }

            dbSize = Convert.ToInt32(sizeofdb);

            try
            {
                TargetProcessor tp = new TargetProcessor();
                _TargetServerInfo.TargetDatabase = tbNewDatabase.Text;
                if (tp.DoesDatabaseExist(_TargetServerInfo))
                {
                    MessageBox.Show(Properties.Resources.MessageDatabaseExists, Properties.Resources.DatabaseExists, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbNewDatabase.Focus();
                    return;
                }

                tp.CreateDatabase(_TargetServerInfo, ((Collation)cbCollations.SelectedValue).Name, edition, dbSize, false);

                DatabaseName = "[" + tbNewDatabase.Text + "]";
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(CommonFunc.GetLowestException(ex), Properties.Resources.ErrorCreatingDB, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Abort;
            }
            Close();
        }

        private void CreateDatabase_Load(object sender, EventArgs e)
        {
            tbNewDatabase.Focus();
        }

        private void cbEdition_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbEdition.SelectedIndex == 0)
            {
                cbMaxDatabaseSize.Items.Clear();
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeOneGB);
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeFiveGB);
                cbMaxDatabaseSize.SelectedIndex = 0;
            }
            else
            {
                cbMaxDatabaseSize.Items.Clear();
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeTenGB);
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeTwentyGB);
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeThirtyGB);
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeFourtyGB);
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeFiftyGB);
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeOneHundredGB);
                cbMaxDatabaseSize.Items.Add(Properties.Resources.CreateDatabaseSizeOneHundredFiftyGB);
                cbMaxDatabaseSize.SelectedIndex = 0;
            }
        }

        private void cbCollations_SelectedIndexChanged(object sender, EventArgs e)
        {
            string col = "";

            if (((Collation) cbCollations.SelectedValue).Name.Length > 0)
            {
                col = " (" + ((Collation)cbCollations.SelectedValue).Name + ")";
            }
            lbCollation.Text = CommonFunc.FormatString(Properties.Resources.DatabaseCollation, col);
        }
    }
}
