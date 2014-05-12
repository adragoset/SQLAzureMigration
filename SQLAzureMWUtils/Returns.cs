using System;
using System.Data;
using System.Xml;

namespace SQLAzureMWUtils
{
    /// <summary>
    /// Summary description for Returns.
    /// </summary>
    public class DatasetResults
    {
        public DataSet DataSet { get; set; }   // Resulting DataSet from SQL query
        public int SpReturnValue { get; set; } // Stored procedure return value (if stored procedure called)

        public DatasetResults()
        {
        }
    }

    public class NonQueryResults
    {
        public int ExecuteResult { get; set; } // Number of rows affected by the ExecuteNonQuery command
        public int SpReturnValue { get; set; } // Stored procedure return value (if stored procedure called)

        public NonQueryResults()
        {
        }
    }

    public class ScalarResults
    {
        public object ExecuteScalarReturnValue { get; set; }
        public int SpReturnValue { get; set; } // Stored procedure return value (if stored procedure called)

        public ScalarResults()
        {
        }
    }

    public class XmlSqlDataReader
    {
        public XmlReader XmlReader;
        public int SpReturnValue; // Stored procedure return value (if stored procedure called)

        public XmlSqlDataReader()
        {
        }
    }
}