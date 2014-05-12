using System;
using System.Collections;

namespace SQLAzureMWUtils
{
    public class NotSupportedViews
    {
        private NotSupportedList _ViewStatement;

        public NotSupportedList ViewStatement
        {
            get { return _ViewStatement; }
            set { _ViewStatement = value; }
        }
    }
}
