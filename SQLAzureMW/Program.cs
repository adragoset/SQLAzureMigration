using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml.XPath;
using SQLAzureMWUtils;

namespace SQLAzureMW
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                string message = "";
                if (DependencyHelper.CheckDependencies(ref message))
                {
                    Application.Run(new ScriptWizard());
                }
                else
                {
                    MessageBox.Show(message, "Dependencies");
                }
            }
            catch (Exception ex)
            {
               MessageBox.Show("Sorry!  SQLAzureMW encountered an error.  Please see additional information to resolve." + Environment.NewLine + ex.ToString());
            }
        }
    }
}
