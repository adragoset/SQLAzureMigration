using System;
using System.IO;

namespace SQLAzureMWUtils
{
    /// <summary>
    /// This is the ConsoleMigrationOutput class.
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
    public class ConsoleMigrationOutput : IMigrationOutput
    {
        public string OutputFile { get; private set; }
        public bool ShouldWriteToConsole { get; private set; }

        public ConsoleMigrationOutput(string outputFile, bool shouldWriteToConsole)
        {
            OutputFile = outputFile;
            ShouldWriteToConsole = shouldWriteToConsole;
        }

        public void StatusUpdateHandler(AsyncNotificationEventArgs args)
        {
            WriteToFile(args);
            WriteToConsole(args);
        }

        private void WriteToFile(AsyncNotificationEventArgs args)
        {
            if (!string.IsNullOrEmpty(OutputFile))
            {
                if (args.FunctionCode == NotificationEventFunctionCode.SqlOutput)
                {
                    File.AppendAllText(OutputFile, args.DisplayText);
                }
            }
        }

        private void WriteToConsole(AsyncNotificationEventArgs args)
        {
            if (ShouldWriteToConsole)
            {
                Console.Write(args.DisplayText);
            }
        }
    }
}
