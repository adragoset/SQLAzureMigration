using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.Drawing;
using System.Collections.Specialized;
using System.Globalization;

namespace SQLAzureMWUtils
{
    public enum DatabaseObjectsTypes
    {
        Roles,
        Schemas,
        Triggers,
        UserDefinedDataTypes,
        UserDefinedTableTypes,
        XMLSchemaCollections
    }

    public class SourceProcessor
    {
        private SMOScriptOptions _smoScriptOpts;
        private AsyncUpdateStatus _updateStatus;
        private AsyncNotificationEventArgs _eventArgs;
        private ScriptDatabase _sdb;
        private string _outputDir;

        public void Initialize(ScriptDatabase sdb, SMOScriptOptions options, AsyncUpdateStatus updateStatus, AsyncNotificationEventArgs eventArgs, string outputDir)
        {
            _sdb = sdb;
            _smoScriptOpts = options;
            _updateStatus = updateStatus;
            _eventArgs = eventArgs;
            _outputDir = outputDir;
        }

        public bool Process(SqlSmoObject[] objects, int start)
        {
            if (objects == null) return AsyncProcessingStatus.CancelProcessing;

            SqlSmoObject[] sso = new SqlSmoObject[1];
            StringCollection tableForeignKey = new StringCollection();
            StringCollection bcpOutputCommands = new StringCollection();
            StringCollection bcpTargetCommands = new StringCollection();
            double step = ((100 - start) / (double)objects.Count());
            int loopCnt = 0;

            foreach (SqlSmoObject obj in objects)
            {
                loopCnt++;

                _eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.ScriptingObj, obj.ToString());
                _eventArgs.PercentComplete = (int)(loopCnt * step);
                _updateStatus(_eventArgs);

                sso[0] = obj;
                switch (obj.Urn.Type)
                {
                     case "Table":
                        _sdb.ScriptTables(sso, ref tableForeignKey, ref bcpOutputCommands, ref bcpTargetCommands, ref _outputDir);
                        break;

                    case "UserDefinedFunction":
                        _sdb.ScriptUDF(sso);
                        break;

                    case "View":
                        _sdb.ScriptViews(sso);
                        break;

                    case "StoredProcedure":
                        _sdb.ScriptProcedures(sso);
                        break;

                    case "DdlTrigger":
                        _sdb.ScriptTriggers(sso);
                        break;
                }

                if (AsyncProcessingStatus.CancelProcessing)
                {
                    _eventArgs.StatusMsg = Properties.Resources.MessageCanceled;
                    _eventArgs.DisplayColor = Color.DarkCyan;
                    _eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                    _eventArgs.PercentComplete = 100;
                    _updateStatus(_eventArgs);
                    return AsyncProcessingStatus.CancelProcessing;
                }
            }

            _eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.ForiegnKeys);
            _updateStatus(_eventArgs);

            // Ok, due to dependency issues, we need to move the foreign key constraints to the end.
            // But there is some small problem in that SQL Server allows for a DBA to have circular dependencies.  So
            // in order to solve this problem, we need to upload the data to the circular dependent tables first
            // with no foreign keys, add foreign key constraints, and then upload the rest of the data.  The
            // reason that the data it not all uploaded before the foreign key constraints is to avoid performance
            // issues with adding foreign key constraints after the fact.

            if (tableForeignKey.Count > 0 && _smoScriptOpts.ScriptForeignKeys == true)
            {
                _sdb.ScriptStringCollection(ref tableForeignKey, Color.Black);
            }

            if (bcpTargetCommands.Count > 0)
            {
                _sdb.ScriptStringCollection(ref bcpTargetCommands, Color.Green);
            }

            if (bcpOutputCommands.Count > 0)
            {
                _sdb.OutputAnalysisLine(Environment.NewLine + Properties.Resources.BCPOutputSummary, Color.Green);
                foreach (string bcpOutputCommand in bcpOutputCommands)
                {
                    _sdb.OutputAnalysisLine(bcpOutputCommand, Color.Green);
                }
            }
            return AsyncProcessingStatus.CancelProcessing;
        }

        public bool Process(DatabaseObjectsTypes objectType, SqlSmoObject[] objectList, int percent)
        {
            if (objectList != null && objectList.Count<SqlSmoObject>() > 0)
            {
                _eventArgs.StatusMsg = CommonFunc.FormatString(Properties.Resources.ScriptingObj, objectType);
                _eventArgs.PercentComplete = percent;
                _updateStatus(_eventArgs);

                switch (objectType)
                {
                    case DatabaseObjectsTypes.Triggers:
                        _sdb.ScriptTriggers(objectList);
                        break;

                    case DatabaseObjectsTypes.Roles:
                        _sdb.ScriptRoles(objectList);
                        break;

                    case DatabaseObjectsTypes.Schemas:
                        _sdb.ScriptSchemas(objectList);
                        break;

                    case DatabaseObjectsTypes.XMLSchemaCollections:
                        _sdb.ScriptXMLSchemaCollections(objectList);
                        break;

                    case DatabaseObjectsTypes.UserDefinedDataTypes:
                        _sdb.ScriptUDT(objectList);
                        break;

                    case DatabaseObjectsTypes.UserDefinedTableTypes:
                        _sdb.ScriptUDTT(objectList);
                        break;
                }
            }

            if (AsyncProcessingStatus.CancelProcessing)
            {
                _eventArgs.DisplayColor = Color.DarkCyan;
                _eventArgs.StatusMsg = Properties.Resources.MessageCanceled;
                _eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageCanceledProcessing, DateTime.Now.ToString(CultureInfo.CurrentCulture)) + Environment.NewLine;
                _eventArgs.PercentComplete = 100;
                _updateStatus(_eventArgs);
            }
            return AsyncProcessingStatus.CancelProcessing;
        }
    }
}
