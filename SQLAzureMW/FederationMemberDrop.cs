using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SQLAzureMWUtils;
using System.Data.SqlClient;
using System.Threading;
using System.Globalization;

namespace SQLAzureMW
{
    public partial class FederationMemberDrop : Form
    {
        private TargetServerInfo _serverInfo;
        private FederationDetails _federationDetails;
        private FederationMemberDistribution _member;
        private bool _cancelProcessing;
        private Thread _runningThread = null;

        public FederationMemberDrop(TargetServerInfo serverInfo, FederationDetails fd, FederationMemberDistribution member, int memberLocation)
        {
            InitializeComponent();

            progressBar1.Visible = false;

            _serverInfo = serverInfo;
            _federationDetails = fd;
            _member = member;

            gbOptions.Text = CommonFunc.FormatString(Properties.Resources.FederationInfo, fd.FederationName, member.DistrubutionName);
            rbDropFederation.Text = CommonFunc.FormatString(Properties.Resources.FederationDrop, fd.FederationName);
            rbDropMember.Checked = true;

            if (fd.Members.Count < 2)
            {
                rbDropFederation.Checked = true;
                rbHigh.Enabled = false;
                rbLow.Enabled = false;
            }
            else if (memberLocation == 0)
            {
                rbHigh.Checked = true;
                rbLow.Enabled = false;
            }
            else if (memberLocation == fd.Members.Count - 1)
            {
                rbLow.Checked = true;
                rbHigh.Enabled = false;
            }
            else
            {
                rbHigh.Checked = true;
            }
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
                if (_cancelProcessing) return;

                if (args.PercentComplete < 0)
                {
                    progressBar1.Value = 0;
                    btnDrop.Text = Properties.Resources.Retry;
                    btnDrop.Enabled = true;
                }
                else if (args.PercentComplete == 100)
                {
                    progressBar1.Value = 100;
                    //btnDrop.Text = Properties.Resources.Done;
                    btnDrop.Enabled = true;
                    btnCancel.Enabled = false;

                    if (rbDropFederation.Checked)
                    {
                        MessageBox.Show(CommonFunc.FormatString(Properties.Resources.FederationDropped, _federationDetails.FederationName));
                    }
                    else
                    {
                        MessageBox.Show(CommonFunc.FormatString(Properties.Resources.FederationMemberDropped, _member.DistrubutionName));
                    }
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
                else
                {
                    progressBar1.Value = args.PercentComplete > 5 ? args.PercentComplete : 5;
                }
            }
        }

        private string GetTSQLForDrop()
        {
            StringBuilder tsql = new StringBuilder();
            if (rbDropFederation.Checked)
            {
                tsql.Append("DROP FEDERATION [" + _federationDetails.FederationName + "]");
            }
            else
            {
                tsql.Append("ALTER FEDERATION [" + _federationDetails.FederationName + "] DROP AT (");
                if (rbHigh.Checked)
                {
                    tsql.Append("LOW " + _member.DistrubutionName + " = " + _member.High + ")");
                }
                else
                {
                    tsql.Append("HIGH " + _member.DistrubutionName + " = " + _member.Low + ")");
                }
            }
            return tsql.ToString();
        }

        private void btnDrop_Click(object sender, EventArgs e)
        {
            if (btnDrop.Text.Equals(Properties.Resources.Done, StringComparison.CurrentCulture))
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
            {
                string tsql = GetTSQLForDrop();

                progressBar1.Visible = true;
                progressBar1.Show();
                _cancelProcessing = false;

                System.Threading.ThreadStart ts = new System.Threading.ThreadStart(delegate() { DropSplitpoint(tsql); });
                _runningThread = new System.Threading.Thread(ts);
                _runningThread.CurrentCulture = CultureInfo.CurrentCulture;
                _runningThread.CurrentUICulture = CultureInfo.CurrentUICulture;
                _runningThread.Start();
            }
        }

        private void DropSplitpoint(object obj)
        {
            string tsql = (string)obj;

            AsyncUpdateStatus updateStatus = new AsyncUpdateStatus(StatusUpdateHandler);
            AsyncNotificationEventArgs eventArgs = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ExecuteSqlOnAzure, 5, "", "", Color.Black);
            updateStatus(eventArgs);

            try
            {
                Retry.ExecuteRetryAction(() =>
                    {
                        using (SqlConnection connection = new SqlConnection(_serverInfo.ConnectionStringRootDatabase))
                        {
                            connection.Open();
                            SqlHelper.ExecuteNonQuery(connection, CommandType.Text, "USE FEDERATION ROOT WITH RESET");
                            SqlHelper.ExecuteNonQuery(connection, CommandType.Text, tsql);

                            while (true)
                            {
                                ScalarResults sr = SqlHelper.ExecuteScalar(connection, CommandType.Text, "SELECT percent_complete FROM sys.dm_federation_operations WHERE federation_id = " + _federationDetails.Federation_id);
                                if (sr.ExecuteScalarReturnValue == null || _cancelProcessing) break;

                                eventArgs.PercentComplete = Convert.ToInt32(sr.ExecuteScalarReturnValue);
                                if (eventArgs.PercentComplete == 100) break;

                                updateStatus(eventArgs);
                                Thread.Sleep(500);
                            }

                            connection.Close();
                            eventArgs.PercentComplete = 100;
                            updateStatus(eventArgs);
                        }
                    });
            }
            catch (Exception ex)
            {
                eventArgs.PercentComplete = -1;
                updateStatus(eventArgs);
                ErrorHelper.ShowException(null, ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_runningThread != null)
            {
                _runningThread.Abort();
                _cancelProcessing = true;
                this.DialogResult = System.Windows.Forms.DialogResult.Abort;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            this.Close();
        }

        private void rbDropMember_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDropMember.Checked)
            {
                gbMerge.Enabled = true;
            }
            else
            {
                gbMerge.Enabled = false;
            }
        }

        private void btnSee_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetTSQLForDrop());
        }
    }
}
