using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public class AsyncTabPageEventArgs : EventArgs
    {
        public bool RemoveTabs { get; set; }
        public bool Show { get; set; }
        public int CurrentTabIndex { get; set; }
        public string DisplayText { get; set; }

        public AsyncTabPageEventArgs(int tabIndex, string displayText)
        {
            Show = true;
            RemoveTabs = false;
            CurrentTabIndex = tabIndex;
            DisplayText = displayText;
        }
    }
}
