using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMW
{
    public static class WizardSteps
    {
        public const int SelectProcess = 0;
        public const int SelectDatabaseSource = 1;
        public const int SelectObjectsToScript = 2;
        public const int ScriptWizardSummary = 3;
        public const int ResultsSummary = 4;
        public const int SetupTargetConnection = 5;
        public const int TargetResults = 6;
    }
}
