using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQLAzureMWUtils
{
    public partial class BCPCommandEditor : Form
    {
        public long NumberOfRows
        {
            get
            {
                return Convert.ToInt64(tbNumOfRows.Text);
            }
        }

        public string BCPCommand
        {
            get
            {
                return tbBCPCommand.Text;
            }
        }

        public BCPCommandEditor(long numOfRows, string bcpCmd)
        {
            InitializeComponent();
            tbBCPCommand.Text = bcpCmd;
            tbNumOfRows.Text = numOfRows.ToString();
        }
    }
}
