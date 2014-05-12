using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SQLAzureMWUtils;

namespace SQLAzureMW
{
    public partial class AdvancedSettings : Form
    {
        public SMOScriptOptions GetOptions
        {
            get { return (SMOScriptOptions) pgOptions.SelectedObject; }
        }

        public AdvancedSettings()
        {
            InitializeComponent();
        }

        public AdvancedSettings(SMOScriptOptions sso)
        {
            InitializeComponent();
            pgOptions.SelectedObject = sso;
        }
    }
}
