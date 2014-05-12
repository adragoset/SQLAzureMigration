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
    public partial class FederationMemberCreate : Form
    {
        private TargetServerInfo _serverInfo;
        private FederationDetails _federationDetails;
        private bool _cancelProcessing = false;
        private Thread _runningThread = null;

        public FederationMemberCreate(TargetServerInfo serverInfo, FederationDetails fd)
        {
            InitializeComponent();

            progressBar1.Visible = false;
            cbDistributionDataType.SelectedIndex = 0;
            cbDistributionType.SelectedIndex = 0;

            _serverInfo = serverInfo;
            _federationDetails = fd;

            if (fd != null)
            {
                tbDistributionDataType.Text = fd.Members[0].FedType;
                tabControlCreate.SelectedIndex = 1;
                tbSplitpoint.Focus();
            }
            else
            {
                tabControlCreate.TabPages.RemoveAt(1);
                tbFederationName.Focus();
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
                if (_cancelProcessing)  return;

                if ( args.PercentComplete < 0)
                {
                    progressBar1.Value = 0;
                    btnCreate.Text = Properties.Resources.Retry;
                    btnCreate.Enabled = true;
                }
                else if (args.PercentComplete == 100)
                {
                    progressBar1.Value = 100;
                    // btnCreate.Text = Properties.Resources.Done;
                    btnCreate.Enabled = true;
                    btnCancel.Enabled = false;

                    if (tabControlCreate.SelectedIndex == 1)
                    {
                        MessageBox.Show(Properties.Resources.FederationMemberCreated);
                    }
                    else
                    {
                        MessageBox.Show(CommonFunc.FormatString(Properties.Resources.FederationCreated, tbFederationName.Text));
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

        private void CreateSplitpoint(object obj)
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
                            SqlHelper.ExecuteNonQuery(connection, CommandType.Text, tsql.ToString());

                            if (_federationDetails.Federation_id == 0)
                            {
                                ScalarResults sr = SqlHelper.ExecuteScalar(connection, CommandType.Text, "SELECT federation_id FROM sys.federations fed where fed.name = '" + _federationDetails.FederationName + "'");
                                _federationDetails.Federation_id = Convert.ToInt32(sr.ExecuteScalarReturnValue);
                            }

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
                _cancelProcessing = true;
                _runningThread.Abort();
                this.DialogResult = System.Windows.Forms.DialogResult.Abort;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            this.Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (btnCreate.Text.Equals(Properties.Resources.Done, StringComparison.CurrentCulture))
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
            {
                StringBuilder tsql = new StringBuilder();
                if (tabControlCreate.SelectedIndex == 0)
                {
                    _federationDetails = new FederationDetails();
                    _federationDetails.FederationName = tbFederationName.Text;
                    _federationDetails.Federation_id = 0;

                    // CREATE FEDERATION SKUFederation_INT(goods INT RANGE)

                    tsql.Append("CREATE FEDERATION [" + tbFederationName.Text + "] (" + tbDistributionName.Text + " " + cbDistributionDataType.SelectedItem);

                    if (cbDistributionDataType.SelectedIndex == 3)
                    {
                        if (numericUpDownVarbinary.Value > 900)
                        {
                            MessageBox.Show(Properties.Resources.FederationVarbinaryRange);
                            numericUpDownVarbinary.Focus();
                            return;
                        }
                        tsql.Append("(" + numericUpDownVarbinary.Value.ToString() + ") " + cbDistributionType.SelectedItem + ")");
                    }
                    else
                    {
                        tsql.Append(" " + cbDistributionType.SelectedItem + ")");
                    }
                }
                else
                {
                    // ALTER FEDERATION CustomerFederation SPLIT AT (cid=1000)
                    
                    tsql.Append("ALTER FEDERATION [" + _federationDetails.FederationName + "] SPLIT AT (" + _federationDetails.Members[0].DistrubutionName + " = ");
                    if (_federationDetails.Members[0].FedType == "uniqueidentifier")
                    {
                        tsql.Append("\"" + tbSplitpoint.Text + "\")");
                    }
                    else
                    {
                        tsql.Append(tbSplitpoint.Text + ")");
                    }
               }

                progressBar1.Visible = true;
                btnCreate.Enabled = false;
                _cancelProcessing = false;

                ThreadStart ts = new ThreadStart(delegate() { CreateSplitpoint(tsql.ToString()); });
                _runningThread = new Thread(ts);
                _runningThread.CurrentCulture = CultureInfo.CurrentCulture;
                _runningThread.CurrentUICulture = CultureInfo.CurrentUICulture;
                _runningThread.Start();
             }
        }

        private void cbFederationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbDistributionDataType.SelectedIndex == 3)
            {
                numericUpDownVarbinary.Visible = true;
            }
            else
            {
                numericUpDownVarbinary.Visible = false;
            }
        }

        private void tabControlCreate_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControlCreate.SelectedIndex == 0)
            {
                tbFederationName.Focus();
            }
            else
            {
                tbSplitpoint.Focus();
            }
        }
    }
}
