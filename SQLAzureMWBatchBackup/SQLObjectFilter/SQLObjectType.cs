using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SQLAzureMWBatchBackup.SQLObjectFilter
{
    public class SQLObjectType
    {
        private bool _Script;
        private List<SQLObject> _SQLObjects;

        [XmlAttribute(DataType = "boolean", AttributeName = "script")]
        public bool Script
        {
            get { return _Script; }
            set { _Script = value; }
        }

        public List<SQLObject> SQLObjects
        {
            get
            {
                if (_SQLObjects == null)
                {
                    _SQLObjects = new List<SQLObject>();
                }
                return _SQLObjects;
            }

            set
            {
                _SQLObjects = value;
            }
        }
    }
}
