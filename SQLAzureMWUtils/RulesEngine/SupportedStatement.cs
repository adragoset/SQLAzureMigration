using System;
using System.Text;
using System.Xml.Serialization;

namespace SQLAzureMWUtils
{
    /// <author name="George Huey" />
    /// <history>
    ///     <change date="9/9/2009" user="George Huey">
    ///         Added headers, etc.
    ///     </change>
    /// </history>

    public class SupportedStatement
    {
        private string _Text;

        [XmlAttribute(DataType = "string", AttributeName = "Text")]
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        public SupportedStatement(string text)
        {
            _Text = text;
        }

        public SupportedStatement()
        {
        }
    }
}
