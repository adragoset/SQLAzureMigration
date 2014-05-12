using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SQLAzureMWUtils
{
    public class AsyncBCPJobEventArgs : EventArgs
    {
        public RichTextBox ResultsTextBox { get; set; }
        public CommandStatus Status { get; set; }
        public string DisplayText { get; set; }
        public Color DisplayColor { get; set; }
        public TabPage CallingTagPage { get; set; }
        public bool ClearRTB { get; set; }
        public int CurrentThreadIndex { get; set; }

        public AsyncBCPJobEventArgs(TabPage callingTaPage, CommandStatus status, RichTextBox rtb, string displayText, Color displayColor)
        {
            CallingTagPage = callingTaPage;
            Status = status;
            ResultsTextBox = rtb;
            DisplayText = displayText;
            DisplayColor = displayColor;
        }
    }
}
