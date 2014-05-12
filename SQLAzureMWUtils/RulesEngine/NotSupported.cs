using System;
using System.Xml.Serialization;

namespace SQLAzureMWUtils
{
    /// <author name="George Huey" />
    /// <history>
    ///     <change date="9/9/2009" user="George Huey">
    ///         Added headers, etc.
    ///     </change>
    /// </history>

    public class NotSupported
    {
        private string _Text;
        private string _NotSubStr;
        private string _SearchReplace;
        private string _ReplaceWith;
        private string _WarningMessage;
        private bool _DisplayWarning;
        private bool _ReplaceString;
        private bool _DefaultMessage;
        private int _SeverityLevel;
        private bool _RemoveCommand;

        [XmlAttribute(DataType = "string", AttributeName = "Text")]
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        [XmlAttribute(DataType = "boolean", AttributeName = "ReplaceString")]
        public bool ReplaceString
        {
            get { return _ReplaceString; }
            set { _ReplaceString = value; }
        }

        [XmlAttribute(DataType = "string", AttributeName = "NotSubStr")]
        public string NotSubStr
        {
            get { return _NotSubStr; }
            set { _NotSubStr = value; }
        }

        [XmlAttribute(DataType = "int", AttributeName = "SeverityLevel")]
        public int SeverityLevel
        {
            get { return _SeverityLevel; }
            set { _SeverityLevel = value; }
        }

        [XmlAttribute(DataType = "string", AttributeName = "SearchReplace")]
        public string SearchReplace
        {
            get { return _SearchReplace; }
            set { _SearchReplace = value; }
        }

        [XmlAttribute(DataType = "string", AttributeName = "ReplaceWith")]
        public string ReplaceWith
        {
            get { return _ReplaceWith; }
            set { _ReplaceWith = value; }
        }

        [XmlAttribute(DataType = "boolean", AttributeName = "DisplayWarning")]
        public bool DisplayWarning
        {
            get { return _DisplayWarning; }
            set { _DisplayWarning = value; }
        }

        [XmlAttribute(DataType = "boolean", AttributeName = "DefaultMessage")]
        public bool DefaultMessage
        {
            get { return _DefaultMessage; }
            set { _DefaultMessage = value; }
        }

        [XmlAttribute(DataType = "string", AttributeName = "WarningMessage")]
        public string WarningMessage
        {
            get { return _WarningMessage; }
            set { _WarningMessage = value; }
        }

        [XmlAttribute(DataType = "boolean", AttributeName = "RemoveCommand")]
        public bool RemoveCommand
        {
            get { return _RemoveCommand; }
            set { _RemoveCommand = value; }
        }

        public NotSupported(string text, string replaceWith, int severityLevel, bool replaceStr, bool displayWarning, bool defaultMessage, string message, bool removeCommand)
        {
            _Text = text;
            _ReplaceWith = replaceWith;
            _SeverityLevel = severityLevel;
            _ReplaceString = replaceStr;
            _DisplayWarning = displayWarning;
            _DefaultMessage = defaultMessage;
            _WarningMessage = message;
            _RemoveCommand = removeCommand;
        }

        public NotSupported()
		{
		}
    }
}