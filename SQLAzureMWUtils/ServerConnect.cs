/*============================================================================
  File:    ServerConnect.cs 

  Summary: Implements a sample SMO server connection utility in C#.

  Date:    June 06, 2005
------------------------------------------------------------------------------
  This file is part of the Microsoft SQL Server Code Samples.

  Copyright (C) Microsoft Corporation.  All rights reserved.

  This source code is intended only as a supplement to Microsoft
  Development Tools and/or on-line documentation.  See these other
  materials for detailed information regarding Microsoft code samples.

  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.
============================================================================*/

#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

// SMO namespaces
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer;
using System.Text.RegularExpressions;
using SQLAzureMWUtils;
using System.Diagnostics;

#endregion

namespace SQLAzureMWUtils
{
    public partial class ServerConnect : Form
    {
        private TargetServerInfo _serverInfo = null;
        public SqlConnection SqlConn;
        public ServerConnection ServerConn;

        private Boolean _Warned;
        private Boolean _ErrorFlag;
        private Boolean _TargetServer;
        private string _OldDatabase;
        public Boolean SpecifiedDatabase { get { return rbSpecifiedDB.Checked; } }
        public Boolean LoginSecure { get { return WindowsAuthenticationRadioButton.Checked; } }
        public string SpecifiedDatabaseName { get { return tbDatabase.Text; } }
        public string ServerInstance { get { return ServerNamesComboBox.Text; } }
        public string UserName { get { return UserNameTextBox.Text; } }
        public string Password { get { return PasswordTextBox.Text; } }

        public ServerTypes ServerType
        {
            get
            {
                if (ServerTypeComboBox.SelectedIndex == 0)
                {
                    return ServerTypes.SQLAzure;
                }
                else if (ServerTypeComboBox.SelectedIndex == 1)
                {
                    return ServerTypes.SQLAzureFed;
                }
                return ServerTypes.SQLServer;
            }
        }

        public ServerConnect(ref SqlConnection connection, ref TargetServerInfo serverInfo, bool target = false, bool lockServerChoice = false)
        {
            SqlConn = connection;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Initialize(ref serverInfo, target);
            if (lockServerChoice)
            {
                ServerTypeComboBox.Enabled = false;
            }
        }

        public ServerConnect(ref TargetServerInfo serverInfo, bool target = false, bool lockServerChoice = false)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Initialize(ref serverInfo, target);
        }

        public ServerConnect(ref ServerConnection connection, ref TargetServerInfo serverInfo, bool target = false, bool lockServerChoice = false)
        {
            ServerConn = connection;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Initialize(ref serverInfo, target);
        }

        private void ServerConnect_Load(object sender, EventArgs e)
        {
            ProcessWindowsAuthentication();
        }

        public void Initialize(ref TargetServerInfo serverInfo, bool target)
        {
            ServerTypeComboBox.Items.Add(Properties.Resources.ServerType_SQLAzure);
            ServerTypeComboBox.Items.Add(Properties.Resources.ServerType_SQLAzureFed);
            ServerTypeComboBox.Items.Add(Properties.Resources.ServerType_SQLServer);

            _TargetServer = target;
            Initialize(ref serverInfo);
        }

        private void WindowsAuthenticationRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ProcessWindowsAuthentication();
        }

        private void Initialize(ref TargetServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
            switch (serverInfo.ServerType)
            {
                case ServerTypes.SQLAzure:
                    ServerTypeComboBox.SelectedIndex = 0;
                    break;

                case ServerTypes.SQLAzureFed:
                    ServerTypeComboBox.SelectedIndex = 1;
                    break;

                default:
                    ServerTypeComboBox.SelectedIndex = 2;
                    break;
            }

            if (serverInfo.LoginSecure)
            {
                WindowsAuthenticationRadioButton.Checked = true;
            }
            else
            {
                rbSpecifyUserPassword.Checked = true;
            }

            ServerNamesComboBox.Text = serverInfo.ServerInstance == null ? "" : serverInfo.ServerInstance;
            UserNameTextBox.Text = serverInfo.Login == null ? "" : serverInfo.Login;
            PasswordTextBox.Text = serverInfo.Password == null ? "" : serverInfo.Password;
            tbDatabase.Text = _OldDatabase = serverInfo.RootDatabase == null ? "" : serverInfo.RootDatabase;
        }

        private void CancelCommandButton_Click(object sender, EventArgs e)
        {
            ServerConn = null;
            this.Close();
        }

        private void ConnectServerConnection()
        {
            ServerConn.ServerInstance = _serverInfo.ServerInstance;
            ServerConn.LoginSecure = _serverInfo.LoginSecure;
            if (_serverInfo.LoginSecure == false)
            {
                ServerConn.Login = _serverInfo.Login;
                ServerConn.Password = _serverInfo.Password;
            }
            ServerConn.DatabaseName = _serverInfo.RootDatabase;
            ServerConn.SqlExecutionModes = SqlExecutionModes.ExecuteSql;
            ServerConn.Connect();
        }

        private void ConnectSqlConnection()
        {
            // Go ahead and connect
            SqlConn.ConnectionString = _serverInfo.ConnectionStringRootDatabase;
            Retry.ExecuteRetryAction(() =>
                {
                    SqlConn.Open();
                });
        }
        
        private void ConnectCommandButton_Click(object sender, EventArgs e)
        {
            Cursor csr = null;

            try
            {
                csr = this.Cursor;   // Save the old cursor
                this.Cursor = Cursors.WaitCursor;   // Display the waiting cursor

                _ErrorFlag = false; // Assume no error

                if (rbSpecifiedDB.Checked && tbDatabase.Text.Length == 0)
                {
                    MessageBox.Show(Properties.Resources.MessageSpecifyDatabase);
                    tbDatabase.Focus();
                    return;
                }

                if (ServerTypeComboBox.SelectedIndex == 0)
                {
                    _serverInfo.ServerType = ServerTypes.SQLAzure;
                }
                else if (ServerTypeComboBox.SelectedIndex == 1)
                {
                    _serverInfo.ServerType = ServerTypes.SQLAzureFed;
                }
                else
                {
                    _serverInfo.ServerType = ServerTypes.SQLServer;
                }
                
                _serverInfo.ServerInstance = ServerNamesComboBox.Text;
                _serverInfo.RootDatabase = rbSpecifiedDB.Checked ? tbDatabase.Text : "";
                _serverInfo.LoginSecure = WindowsAuthenticationRadioButton.Checked;

                if (WindowsAuthenticationRadioButton.Checked == false)
                {
                    // Use SQL Server authentication
                    if (UserNameTextBox.Text.Length == 0)
                    {
                        MessageBox.Show(Properties.Resources.MessageUserName);
                        UserNameTextBox.Focus();
                        return;
                    }

                    _serverInfo.Password = PasswordTextBox.Text;

                    if (Regex.IsMatch(ServerNamesComboBox.Text, Properties.Resources.SQLAzureCloudName, RegexOptions.IgnoreCase))
                    {
                        string svrName = ServerNamesComboBox.Text.Substring(0, ServerNamesComboBox.Text.IndexOf('.'));
                        string svrExt = "@" + svrName;
                        string usrSvrName = "";
                        int    idx = UserNameTextBox.Text.IndexOf('@');

                        if (idx > 0)
                        {
                            usrSvrName = UserNameTextBox.Text.Substring(idx);
                        }

                        if (!usrSvrName.Equals(svrExt))
                        {
                            if (idx < 1)
                            {
                                // Ok, the user forgot to put "@server" at the end of the user name.  See if they want
                                // us to add it for them.

                                DialogResult dr = MessageBox.Show(CommonFunc.FormatString(Properties.Resources.AzureUserName, svrExt), Properties.Resources.AzureUserNameWarning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                                if (dr == DialogResult.Yes)
                                {
                                    UserNameTextBox.Text = UserNameTextBox.Text + svrExt;
                                }
                            }
                            else
                            {
                                // Ok, to get there, the user added @server to the end of the username, but the @server does not match @server in Server name.
                                // Check to see if the user wants us to fix for them.


                                DialogResult dr = MessageBox.Show(CommonFunc.FormatString(Properties.Resources.ServerNamesDoNotMatch, usrSvrName, svrExt), Properties.Resources.AzureUserNameWarning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                                if (dr == DialogResult.Yes)
                                {
                                    UserNameTextBox.Text = UserNameTextBox.Text.Substring(0, idx) + svrExt;
                                }
                            }
                        }
                    }
                    _serverInfo.Login = UserNameTextBox.Text;
                }

                // Go ahead and connect
                if (SqlConn != null)
                {
                    ConnectSqlConnection();
                }
                else if (ServerConn != null)
                {
                    ConnectServerConnection();
                }
                else
                {
                    if (_serverInfo.ServerType == ServerTypes.SQLAzureFed)
                    {
                        SqlConn = new SqlConnection();
                        ConnectSqlConnection();
                    }
                    else
                    {
                        ServerConn = new ServerConnection();
                        ConnectServerConnection();
                    }
                }
                this.DialogResult = DialogResult.OK;
            }
            catch (ConnectionFailureException ex)
            {
                string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(message, Properties.Resources.Error2, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ErrorFlag = true;
            }
            catch (SmoException ex)
            {
                string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(message, Properties.Resources.Error2, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ErrorFlag = true;
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(message, Properties.Resources.Error2, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ErrorFlag = true;
            }
            finally
            {
                this.Cursor = csr;  // Restore the original cursor
            }
        }

        private void ServerConnect_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_ErrorFlag == true)
            {
                e.Cancel = true;
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }

            // Reset error condition
            _ErrorFlag = false;
        }

        private void GetServerList()
        {
            DataTable dt;

            // Set local server as default
            dt = SmoApplication.EnumAvailableSqlServers(false);
            if (dt.Rows.Count > 0)
            {
                // Load server names into combo box
                foreach (DataRow dr in dt.Rows)
                {
                    ServerNamesComboBox.Items.Add(dr["Name"]);
                }

                // Default to this machine 
                ServerNamesComboBox.SelectedIndex = ServerNamesComboBox.FindStringExact(System.Environment.MachineName);

                // If this machine is not a SQL server 
                // then select the first server in the list
                if (ServerNamesComboBox.SelectedIndex < 0)
                {
                    ServerNamesComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(Properties.Resources.NoSqlServers, Properties.Resources.Error2, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ProcessWindowsAuthentication()
        {
            if (WindowsAuthenticationRadioButton.Checked == true)
            {
                UserNameTextBox.Enabled = false;
                PasswordTextBox.Enabled = false;
            }
            else
            {
                UserNameTextBox.Enabled = true;
                PasswordTextBox.Enabled = true;
            }
        }

        private void ServerNamesComboBox_DropDown(object sender, EventArgs e)
        {
            if (ServerNamesComboBox.Items.Count == 0)
            {
                Cursor csr = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
                GetServerList();
                this.Cursor = csr;
            }
        }

        private void rbMasterDB_CheckedChanged(object sender, EventArgs e)
        {
            SpecifyDatabase();
        }

        private void SpecifyDatabase()
        {
            if (rbMasterDB.Checked == true)
            {
                tbDatabase.Enabled = false;
                _OldDatabase = tbDatabase.Text;
                tbDatabase.Text = "";
            }
            else
            {
                if (ServerTypeComboBox.SelectedIndex != 1 && !_Warned && !_TargetServer)
                {
                    _Warned = true;
                    string warning = Properties.Resources.DBConnect + Environment.NewLine + Environment.NewLine + Properties.Resources.PerformanceWarningAreYouSure;
                    DialogResult dr = MessageBox.Show(warning, Properties.Resources.PerformanceWarning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.No)
                    {
                        rbMasterDB.Checked = true;
                        return;
                    }
                }
                tbDatabase.Enabled = true;
                tbDatabase.Text = _OldDatabase;
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (_TargetServer)
            {
                MessageBox.Show(Properties.Resources.DBConnect + Environment.NewLine + Environment.NewLine + Properties.Resources.DBConnectWarningTarget, Properties.Resources.MessageTitleConnectTarget, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(Properties.Resources.DBConnect, Properties.Resources.MessageTitleConnectSource, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ServerTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ServerTypeComboBox.SelectedIndex)
            {
                case 0:
                    SetupSQLAzureDisplay();
                    break;

                case 1:
                    SetupSQLAzureFedDisplay();
                    break;

                default:
                    SetupSQLServerDisplay();
                    break;
            }
        }

        public void SetupSQLAzureDisplay()
        {
            rbSpecifyUserPassword.Checked = true;
            WindowsAuthenticationRadioButton.Enabled = false;
            rbMasterDB.Enabled = true;
            rbMasterDB.Checked = true;
        }

        public void SetupSQLAzureFedDisplay()
        {
            rbSpecifyUserPassword.Checked = true;
            WindowsAuthenticationRadioButton.Enabled = false;
            rbSpecifiedDB.Checked = true;
            rbMasterDB.Enabled = false;
        }

        public void SetupSQLServerDisplay()
        {
            WindowsAuthenticationRadioButton.Enabled = true;
            if (_serverInfo.LoginSecure)
            {
                WindowsAuthenticationRadioButton.Checked = true;
            }
            else
            {
                rbSpecifyUserPassword.Checked = true;
            }
            rbMasterDB.Enabled = true;
            rbMasterDB.Checked = true;
        }
    }
}