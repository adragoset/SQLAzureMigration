using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SQLAzureMWUtils;
using System.Threading;
using System.Drawing;
using System.Globalization;
using System.Configuration;

namespace SQLAzureMW
{
    public partial class ScriptWizard : Form, IMigrationOutput
    {
        public Thread[] _ta;
        public Thread _ThreadManager;
        public List<BCPJobInfo> BCPJobs = new List<BCPJobInfo>();
        public AsyncUpdateTab UpdateTabHandler;
        private TabPage[] _tabPages;

        private void Initialize_BCPJobs()
        {
            this.FormClosing += Form1_FormClosing;
            this.tabCtlUploadStatus.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.tabCtlUploadStatus.DrawItem += new DrawItemEventHandler(ChangeTabColor);
            this.UpdateTabHandler += UpdateTab_Handler;
            ResetBCPFailedButtions(CommandStatus.Success);
        }

        private void UpdateTab_Handler(AsyncTabPageEventArgs args)
        {
            if (this.InvokeRequired)
            {
                AsyncUpdateTab updateStatus = new AsyncUpdateTab(UpdateTab_Handler);
                this.Invoke(updateStatus, new object[] { args });
            }
            else
            {
                if (args.RemoveTabs)
                {
                    for (int index = tabCtlUploadStatus.TabCount - 1; index > 0; index--)
                    {
                        tabCtlUploadStatus.TabPages.RemoveAt(index);
                    }
                }
                else if (!args.Show)
                {
                    tabCtlUploadStatus.TabPages.Remove(_tabPages[args.CurrentTabIndex]);
                }
                else
                {
                    _tabPages[args.CurrentTabIndex].Text = args.DisplayText;
                    tabCtlUploadStatus.TabPages.Add(_tabPages[args.CurrentTabIndex]);
                }
            }
        }

        private void AsyncQueueJobHandler(AsyncQueueBCPJobArgs args)
        {
            if (this.InvokeRequired)
            {
                AsyncQueueBCPJob queueJobHandler = new AsyncQueueBCPJob(AsyncQueueJobHandler);
                this.Invoke(queueJobHandler, new object[] { args });
            }
            else
            {
                BCPJobs.Add(args.JobInfo);
                if (_ThreadManager == null)
                {
                    int threadCount = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfBCPThreads"]);
                    _ta = new Thread[threadCount];
                    _tabPages = new TabPage[threadCount];

                    for (int index = 0; index < _ta.Count(); index++)
                    {
                        _tabPages[index] = AddTab();
                    }

                    ThreadStart ts = new System.Threading.ThreadStart(ThreadManager);
                    _ThreadManager = new System.Threading.Thread(ts);
                    _ThreadManager.CurrentCulture = CultureInfo.CurrentCulture;
                    _ThreadManager.CurrentUICulture = CultureInfo.CurrentUICulture;
                    _ThreadManager.Start();
                }
            }
        }

        private bool BCPJobsDone()
        {
            for (int index = 0; index < BCPJobs.Count(); index++)
            {
                if (BCPJobs[index].JobStatus != CommandStatus.Success && BCPJobs[index].JobStatus != CommandStatus.Skip)
                {
                    return false;
                }
            }
            return true;
        }

        private BCPJobInfo GetNextWaitingJob()
        {
            for (int index = 0; index < BCPJobs.Count(); index++)
            {
                if (BCPJobs[index].JobStatus == CommandStatus.Waiting)
                {
                    BCPJobs[index].JobStatus = CommandStatus.InProcess;
                    return BCPJobs[index];
                }
            }
            return null;
        }

        public void ThreadManager()
        {
            AsyncTabPageEventArgs args = new AsyncTabPageEventArgs(0, "");
            while (true)
            {
                for (int index = 0; index < _ta.Count(); index++)
                {
                    args.CurrentTabIndex = index;
                    if (_ta[index] != null)
                    {
                        if (_ta[index].ThreadState == System.Threading.ThreadState.Stopped)
                        {
                            BCPCommandCtrl par = (BCPCommandCtrl) _tabPages[index].Controls[0];
                            if (par.CurrentJobInfo.JobStatus == CommandStatus.Success || par.CurrentJobInfo.JobStatus == CommandStatus.Skip)
                            {
                                 args.Show = false;
                                UpdateTabHandler(args);
                                _ta[index] = null;
                            }
                        }
                    }

                    args.Show = true;

                    if (_ta[index] == null)
                    {
                        BCPJobInfo jobInfo = GetNextWaitingJob();
                        if (jobInfo != null)
                        {
                            args.DisplayText = jobInfo.TableName;
                            args.CurrentTabIndex = index;
                            jobInfo.CurrentThreadIndex = index;

                            UpdateTabHandler(args);

                            BCPCommandCtrl par = (BCPCommandCtrl)_tabPages[index].Controls[0];
                            par.CurrentJobInfo = jobInfo;

                            ThreadStart ts = new System.Threading.ThreadStart(delegate() { StartUpload(par); });
                            _ta[index] = new Thread(ts);
                            _ta[index].CurrentCulture = CultureInfo.CurrentCulture;
                            _ta[index].CurrentUICulture = CultureInfo.CurrentUICulture;
                            _ta[index].Start();
                        }
                    }
                }
                Thread.Sleep(500);    // Sleep for awhile and then look for more data to process
                if (AsyncProcessingStatus.FinishedAddingJobs && BCPJobsDone()) break;
            }
            args.RemoveTabs = true;
            UpdateTabHandler(args);
            AsyncProcessingStatus.FinishedProcessingJobs = true;
        }

        public void StartUpload(object obj)
        {
            BCPCommandCtrl bcc = (BCPCommandCtrl)obj;
            bcc.ExecuteBCPCommand();
        }

        private void BCPAsyncUpdateResultsHandler(AsyncBCPJobEventArgs args)
        {
            if (this.InvokeRequired)
            {
                AsyncBCPJobUpdateStatus updateStatus = new AsyncBCPJobUpdateStatus(BCPAsyncUpdateResultsHandler);
                this.Invoke(updateStatus, new object[] { args });
            }
            else
            {
                string[] titleInfo = args.CallingTagPage.Text.Split("|".ToArray());
                args.CallingTagPage.Text = titleInfo[0] + "|" + args.Status.ToString();
                if (args.Status == CommandStatus.Success || args.Status == CommandStatus.Skip)
                {
                    rtbTargetResults.AppendText(Environment.NewLine);
                    args.ResultsTextBox.SelectAll();
                    args.ResultsTextBox.Copy();
                    rtbTargetResults.Paste();
                    rtbTargetResults.ScrollToCaret();
                }
                else
                {
                    if (args.ClearRTB)
                    {
                        args.ResultsTextBox.Clear();
                    }

                    if (args.Status == CommandStatus.Failed)
                    {
                        if (args.CallingTagPage == tabCtlUploadStatus.SelectedTab)
                        {
                            ResetBCPFailedButtions(args.Status);
                        }
                    }
                    args.ResultsTextBox.SelectionColor = args.DisplayColor;
                    args.ResultsTextBox.AppendText(args.DisplayText);
                    args.ResultsTextBox.ScrollToCaret();
                }
            }
        }

        private TabPage AddTab()
        {
            TabPage tp = new TabPage();
            tp.Location = tabResults.Location;
            tp.Name = "tab" + tabCtlUploadStatus.Controls.Count.ToString();
            tp.Padding = tabResults.Padding;
            tp.Size = tabResults.Size;
            tp.TabIndex = tabCtlUploadStatus.Controls.Count;
            tp.Text = "";
            tp.UseVisualStyleBackColor = true;
            tp.Controls.Add(GetBCPCommandCtrl());
            tp.Dock = System.Windows.Forms.DockStyle.Fill;
            return tp;
        }

        private void ChangeTabColor(object sender, DrawItemEventArgs e)
        {
            Color foreColor = System.Drawing.SystemColors.ControlText;
            string tabName = this.tabCtlUploadStatus.TabPages[e.Index].Text;
            string[] titleInfo = tabCtlUploadStatus.TabPages[e.Index].Text.Split("|".ToArray());

            // some logic to determine the color of the text - basically if the name contains an asterix make the text red, then make a weak effort to remove the asterix from what we actually show  
            if (titleInfo.Count() > 1)
            {
                if (titleInfo[1].Equals(CommandStatus.Success.ToString()))
                {
                    foreColor = Color.Green;
                }
                else if (titleInfo[1].Equals(CommandStatus.Failed.ToString()))
                {
                    foreColor = Color.Red;
                }
                else if (titleInfo[1].Equals(CommandStatus.InProcess.ToString()))
                {
                    foreColor = Color.Blue;
                }
            }
            tabName = titleInfo[0];
            DrawTabText(this.tabCtlUploadStatus, e, foreColor, tabName);
        }

        private BCPCommandCtrl GetBCPCommandCtrl()
        {
            BCPCommandCtrl bcc = new BCPCommandCtrl();
            bcc.UpdateBCPJobStatus = new AsyncBCPJobUpdateStatus(BCPAsyncUpdateResultsHandler);
            bcc.Dock = System.Windows.Forms.DockStyle.Fill;
            bcc.Location = new System.Drawing.Point(3, 3);
            bcc.Name = "ctrl" + tabCtlUploadStatus.Controls.Count.ToString();
            bcc.Size = new System.Drawing.Size(tabResults.Size.Width - 10, tabResults.Size.Height - 10);
            bcc.TabIndex = 0;
            return bcc;
        }

        public static void DrawTabText(TabControl tabControl, DrawItemEventArgs e, string caption)
        {
            Color backColor = (Color)System.Drawing.SystemColors.Control;
            Color foreColor = (Color)System.Drawing.SystemColors.ControlText;
            DrawTabText(tabControl, e, backColor, foreColor, caption);
        }

        public static void DrawTabText(TabControl tabControl, DrawItemEventArgs e, System.Drawing.Color foreColor, string caption)
        {
            Color backColor = (Color)System.Drawing.SystemColors.Control;
            DrawTabText(tabControl, e, backColor, foreColor, caption);
        }

        public static void DrawTabText(TabControl tabControl, DrawItemEventArgs e, System.Drawing.Color backColor, System.Drawing.Color foreColor, string caption)
        {
            #region setup

            Font tabFont;
            Brush foreBrush = new SolidBrush(foreColor);
            Rectangle r = e.Bounds;
            Brush backBrush = new SolidBrush(backColor);
            string tabName = tabControl.TabPages[e.Index].Text;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;

            #endregion

            #region drawing
            e.Graphics.FillRectangle(backBrush, r);


            r = new Rectangle(r.X, r.Y + 3, r.Width, r.Height - 3);
            if (e.Index == tabControl.SelectedIndex)
            {
                tabFont = new Font(e.Font, FontStyle.Italic);
                tabFont = new Font(tabFont, FontStyle.Bold);
            }
            else
            {
                tabFont = e.Font;
            }

            e.Graphics.DrawString(caption, tabFont, foreBrush, r, sf);
            #endregion

            #region cleanup
            sf.Dispose();
            if (e.Index == tabControl.SelectedIndex)
            {
                tabFont.Dispose();
                backBrush.Dispose();
            }
            else
            {
                backBrush.Dispose();
                foreBrush.Dispose();
            }
            #endregion
        }

        private void ResetBCPFailedButtions(CommandStatus status)
        {
            if (status == CommandStatus.Failed)
            {
                btnRetry.Visible = true;
                btnSkip.Visible = true;
                btnEdit.Visible = true;
                btnCancelTargetProcessing.Visible = false;
                btnSaveTargetResults.Visible = false;
            }
            else
            {
                btnRetry.Visible = false;
                btnSkip.Visible = false;
                btnEdit.Visible = false;
                btnCancelTargetProcessing.Visible = true;
                btnSaveTargetResults.Visible = true;
            }
        }

        private void tabCtlUploadStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabCtlUploadStatus.SelectedIndex == 0)
            {
                ResetBCPFailedButtions(CommandStatus.Success);
            }
            else
            {
                try
                {
                    BCPCommandCtrl par = (BCPCommandCtrl)tabCtlUploadStatus.TabPages[tabCtlUploadStatus.SelectedIndex].Controls[0];
                    BCPJobInfo jobInfo = par.CurrentJobInfo;
                    ResetBCPFailedButtions(jobInfo.JobStatus);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ResetBCPFailedButtions(CommandStatus.Success);
                }
            }
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            BCPCommandCtrl par = (BCPCommandCtrl)tabCtlUploadStatus.TabPages[tabCtlUploadStatus.SelectedIndex].Controls[0];
            BCPJobInfo jobInfo = par.CurrentJobInfo;

            ThreadStart ts = new System.Threading.ThreadStart(delegate() { StartUpload(par); });
            _ta[par.CurrentJobInfo.CurrentThreadIndex] = new Thread(ts);
            _ta[par.CurrentJobInfo.CurrentThreadIndex].Start();
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            BCPCommandCtrl cmdCtrl = (BCPCommandCtrl)tabCtlUploadStatus.TabPages[tabCtlUploadStatus.SelectedIndex].Controls[0];
            cmdCtrl.CurrentJobInfo.JobStatus = CommandStatus.Skip;
            AsyncBCPJobEventArgs args = new AsyncBCPJobEventArgs(tabCtlUploadStatus.TabPages[tabCtlUploadStatus.SelectedIndex], CommandStatus.Skip, cmdCtrl.Results, "", Color.Red);
            BCPAsyncUpdateResultsHandler(args);
            cmdCtrl.NotifyJobIsFinished();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            BCPCommandCtrl cmdCtrl = (BCPCommandCtrl)tabCtlUploadStatus.TabPages[tabCtlUploadStatus.SelectedIndex].Controls[0];
            BCPJobInfo jobInfo = cmdCtrl.CurrentJobInfo;
            BCPCommandEditor editor = new BCPCommandEditor(jobInfo.NumberOfRows, jobInfo.BCPUploadCommand);
            if (editor.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
            {
                jobInfo.BCPUploadCommand = editor.BCPCommand;
                jobInfo.NumberOfRows = editor.NumberOfRows;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            AsyncProcessingStatus.FinishedProcessingJobs = true;
            CancelAsyncProcesses();
        }
    }
}
