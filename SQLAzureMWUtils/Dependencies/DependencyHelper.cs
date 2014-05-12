using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SQLAzureMWUtils
{
    public static class DependencyHelper
    {
        private static List<Dependency> _dependencies = null;

        public static List<Dependency> Dependencies
        {
            get
            {
                if (_dependencies == null)
                {
                    try
                    {
                        string file = CommonFunc.GetAppSettingsStringValue("DependencyFile");
                        if (file.Length > 0)
                        {
                            string dependsOn = CommonFunc.GetTextFromFile(file);
                            if (dependsOn.Length > 0)
                            {
                                _dependencies = (List<Dependency>)CommonFunc.DeserializeXmlString(dependsOn, typeof(List<Dependency>));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(CommonFunc.FormatString(Properties.Resources.DependencyCheckerFileNotFound, ex.Message));
                    }

                    if (_dependencies == null)
                    {
                        _dependencies = new List<Dependency>();
                    }
                }
                return _dependencies;
            }
        }

        public static bool CheckDependencies(ref string message)
        {
            // Create an instance of dependent classes in a new AppDomain
            try
            {
                AppDomain newAppDomain = AppDomain.CreateDomain("DependencyChecker");
                foreach (Dependency depOn in Dependencies)
                {
                    newAppDomain.CreateInstance(depOn.Assembly, depOn.Type);
                }
            }
            catch (Exception ex)
            {
                Assembly assem = Assembly.GetEntryAssembly();
                AssemblyName assemName = assem.GetName();

                message = CommonFunc.FormatString(Properties.Resources.ErrorMissingDependencies, assemName.Name, ex.Message);
                return false;
            }
            return true;
        }
    }
}
