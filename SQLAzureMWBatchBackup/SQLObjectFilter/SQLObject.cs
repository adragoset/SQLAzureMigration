using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace SQLAzureMWBatchBackup.SQLObjectFilter
{
    public class SQLObject
    {
        private bool _Script;
        private bool _RegexExpression;
        private string _name;
        private string _schema;

        [XmlAttribute(DataType = "boolean", AttributeName = "script")]
        public bool Script
        {
            get { return _Script; }
            set { _Script = value; }
        }

        [XmlAttribute(DataType = "boolean", AttributeName = "regex")]
        public bool RegexExpression
        {
            get { return _RegexExpression; }
            set { _RegexExpression = value; }
        }

        [XmlAttribute(DataType = "string", AttributeName = "schema")]
        public string Schema
        {
            get
            {
                if (_schema == null)
                {
                    _schema = "";
                }
                return _schema;
            }

            set { _schema = value; }
        }

        [XmlAttribute(DataType = "string", AttributeName = "name")]
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = "";
                }
                return _name;
            }

            set { _name = value; }
        }

        public SQLObjectAction CheckAction(string schema, string name)
        {
            SQLObjectAction answer = SQLObjectAction.DidNotMatch;
            bool schemaMatch = false;

            if (_RegexExpression)
            {
                // if schema is not specified, then we will consider that as a don't care
                // what schema is, just accept whatever comes our way

                if (schema.Length == 0 || Schema.Length == 0)
                {
                    schemaMatch = true;
                }
                else
                {
                    if (Regex.IsMatch(schema, Schema))
                    {
                        schemaMatch = true;
                    }
                }

                if (schemaMatch && Regex.IsMatch(name, Name))
                {
                    if (Script)
                    {
                        answer = SQLObjectAction.MatchedAndScript;
                    }
                    else
                    {
                        answer = SQLObjectAction.MatchedAndDoNotScript;
                    }
                }
            }
            else
            {
                // if schema is not specified, then we will consider that as a don't care
                // what schema is, just accept whatever comes our way

                if (schema.Length == 0 || Schema.Length == 0)
                {
                    schemaMatch = true;
                }
                else
                {
                    if (Schema.Equals(schema, StringComparison.CurrentCultureIgnoreCase))
                    {
                        schemaMatch = true;
                    }
                }

                if (schemaMatch && Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Script)
                    {
                        answer = SQLObjectAction.MatchedAndScript;
                    }
                    else
                    {
                        answer = SQLObjectAction.MatchedAndDoNotScript;
                    }
                }
            }
            return answer;
        }
    }
}
