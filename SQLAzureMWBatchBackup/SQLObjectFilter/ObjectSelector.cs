using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWBatchBackup.SQLObjectFilter
{
    public class ObjectSelector
    {
        private SQLObjectType _Roles;
        private SQLObjectType _Views;
        private SQLObjectType _UserDefinedFunctions;
        private SQLObjectType _UserDefinedDataTypes;
        private SQLObjectType _UserDefinedTableTypes;
        private SQLObjectType _StoredProcedures;
        private SQLObjectType _Triggers;
        private SQLObjectType _Schemas;
        private SQLObjectType _SchemaCollections;
        private SQLObjectType _Tables;

        public SQLObjectType Roles
        {
            get
            {
                if (_Roles == null)
                {
                    _Roles = new SQLObjectType();
                }
                return _Roles;
            }

            set
            {
                _Roles = value;
            }
        }

        public SQLObjectType Views
        {
            get
            {
                if (_Views == null)
                {
                    _Views = new SQLObjectType();
                }
                return _Views;
            }

            set
            {
                _Views = value;
            }
        }

        public SQLObjectType UserDefinedFunctions
        {
            get
            {
                if (_UserDefinedFunctions == null)
                {
                    _UserDefinedFunctions = new SQLObjectType();
                }
                return _UserDefinedFunctions;
            }

            set
            {
                _UserDefinedFunctions = value;
            }
        }

        public SQLObjectType UserDefinedDataTypes
        {
            get
            {
                if (_UserDefinedDataTypes == null)
                {
                    _UserDefinedDataTypes = new SQLObjectType();
                }
                return _UserDefinedDataTypes;
            }

            set
            {
                _UserDefinedDataTypes = value;
            }
        }

        public SQLObjectType UserDefinedTableTypes
        {
            get
            {
                if (_UserDefinedTableTypes == null)
                {
                    _UserDefinedTableTypes = new SQLObjectType();
                }
                return _UserDefinedTableTypes;
            }

            set
            {
                _UserDefinedTableTypes = value;
            }
        }

        public SQLObjectType StoredProcedures
        {
            get
            {
                if (_StoredProcedures == null)
                {
                    _StoredProcedures = new SQLObjectType();
                }
                return _StoredProcedures;
            }

            set
            {
                _StoredProcedures = value;
            }
        }

        public SQLObjectType Triggers
        {
            get
            {
                if (_Triggers == null)
                {
                    _Triggers = new SQLObjectType();
                }
                return _Triggers;
            }

            set
            {
                _Triggers = value;
            }
        }

        public SQLObjectType Schemas
        {
            get
            {
                if (_Schemas == null)
                {
                    _Schemas = new SQLObjectType();
                }
                return _Schemas;
            }

            set
            {
                _Schemas = value;
            }
        }

        public SQLObjectType SchemaCollections
        {
            get
            {
                if (_SchemaCollections == null)
                {
                    _SchemaCollections = new SQLObjectType();
                }
                return _SchemaCollections;
            }

            set
            {
                _SchemaCollections = value;
            }
        }

        public SQLObjectType Tables
        {
            get
            {
                if (_Tables == null)
                {
                    _Tables = new SQLObjectType();
                }
                return _Tables;
            }

            set
            {
                _Tables = value;
            }
        }
    }
}
