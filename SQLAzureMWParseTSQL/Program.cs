using System;
using System.Configuration;
using System.IO;
using SQLAzureMWUtils;

namespace SQLAzureMWParseTSQL
{
    /// <summary>
    /// This is the SQLAzureMWParseTSQL.
    /// </summary>
    /// <devdoc>
    /// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    /// PARTICULAR PURPOSE.
    /// </devdoc>
    /// <author name="Josh Maletz" />
    /// <history>
    ///     <change date="10/7/2010" user="Josh Maletz">
    ///         Added headers, etc.
    ///     </change>
    /// </history>
    class Program
    {
        static void Main(string[] args)
        {
            _FileToProcess = ConfigurationManager.AppSettings["FileToProcess"];
            _OutputResultsFile = ConfigurationManager.AppSettings["OutputResultsFile"];
            _NoConsoleLogging = CommonFunc.GetAppSettingsBoolValue("NoConsoleLogging");

            for (int index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "-i":
                    case "/i":
                        _FileToProcess = args[++index];
                        break;

                    case "-o":
                    case "/o":
                        _OutputResultsFile = args[++index];
                        try
                        {
                            using (var writer = new StreamWriter(File.Create(_OutputResultsFile)))
                            {
                                writer.WriteLine();
                                writer.WriteLine("-- SQLAzureMWParseTSQL TSQL Output File:");
                                writer.WriteLine(string.Format("-- Generated at: {0}", DateTime.Now));
                                writer.WriteLine();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Sorry, error opening output file " + _OutputResultsFile + ".  Error: " + ex.Message);
                            return;
                        }
                        break;

                    case "-ncl":
                    case "/ncl":
                        _NoConsoleLogging = true;
                        break;

                    default:
                        FailDueToInvalidPrerequisites();
                        return;
                }
            }

            if (ValidatePrerequisites())
            {
                Process();
            }
        }

        private static bool ValidatePrerequisites()
        {
            if (string.IsNullOrEmpty(_FileToProcess) || string.IsNullOrEmpty(_OutputResultsFile))
            {
                FailDueToInvalidPrerequisites();
                return false;
            }

            return true;
        }

        private static void FailDueToInvalidPrerequisites()
        {
            Console.WriteLine(_argValues);
            return;
        }

        private static void Process()
        {
            IMigrationOutput output = new ConsoleMigrationOutput(_OutputResultsFile, !_NoConsoleLogging);
            var migrator = new TsqlFileMigrator(output);
            migrator.ParseFile(_FileToProcess, true);
        }

        private static string _FileToProcess = "";
        private static string _OutputResultsFile = "";
        private static bool _NoConsoleLogging = false;
        private static string _argValues = Environment.NewLine +
                               "All parameters for SQLAzureMWParseTSQL can be found in the config file." + Environment.NewLine +
                               "If you want, you can override the config file by specifying the parameters" + Environment.NewLine +
                               "you want to override." + Environment.NewLine +
                               Environment.NewLine +
                               "usage: SQLAzureMWParseTSQL -i old.sql -o new.sql -l migration.log" + Environment.NewLine + Environment.NewLine +
                               "[-i TSQL input file]" + Environment.NewLine +
                               "[-o TSQL output file]" + Environment.NewLine +
                               "[-ncl No Console Logging]" + Environment.NewLine +
                               "Note that the above args override the values in the application config file." + Environment.NewLine;
    }
}
