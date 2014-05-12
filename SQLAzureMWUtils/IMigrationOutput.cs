using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    public interface IMigrationOutput
    {
        void StatusUpdateHandler(AsyncNotificationEventArgs args);
    }
}
