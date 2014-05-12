using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWBatchBackup.SQLObjectFilter
{
    public enum SQLObjectAction
    {
        MatchedAndScript,
        MatchedAndDoNotScript,
        DidNotMatch
    }
}
