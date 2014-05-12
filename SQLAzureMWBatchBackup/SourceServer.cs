using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.Globalization;
using System.Diagnostics;
using SQLAzureMWUtils;
using Microsoft.SqlServer.Management.Common;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using SQLAzureMWBatchBackup.SQLObjectFilter;

namespace SQLAzureMWBatchBackup
{
    public class SourceServer
    {
        private AsyncUpdateStatus _updateStatus;
        private SMOScriptOptions _smoScriptOpts;
        private ObjectSelector _ObjectSelector;

        public SourceServer()
        {
        }

        private bool OkToScript(SQLObjectType sot, string schema, string name)
        {
            bool answer = false;

            if (!sot.Script) return answer;

            if (sot.SQLObjects.Count == 0)
            {
                answer = true;
            }
            else
            {
                foreach (SQLObject so in sot.SQLObjects)
                {
                    SQLObjectAction soa = so.CheckAction(schema, name);
                    if (soa == SQLObjectAction.MatchedAndDoNotScript)
                    {
                        answer = false;
                        break;
                    }
                    else if (soa == SQLObjectAction.MatchedAndScript)
                    {
                        answer = true;
                        break;
                    }
                }
            }
            return answer;
        }

        private SqlSmoObject[] Sort(Database sourceDatabase, SqlSmoObject[] smoObjects)
        {
            if (smoObjects.Count() < 2) return smoObjects;

            DependencyTree dt = null;
            DependencyWalker dw = new DependencyWalker(sourceDatabase.Parent);

            try
            {
                dt = dw.DiscoverDependencies(smoObjects, true);
            }
            catch (Exception ex)
            {
                AsyncNotificationEventArgs eventArgs = new AsyncNotificationEventArgs(NotificationEventFunctionCode.ExecuteSqlOnAzure, 100, "", DateTime.Now.ToString(CultureInfo.CurrentCulture) + Environment.NewLine + ex.Message, Color.Red);
                _updateStatus(eventArgs);
                return smoObjects;
            }

            SqlSmoObject[] sorted = new SqlSmoObject[smoObjects.Count()];
            int index = 0;

            foreach (DependencyCollectionNode d in dw.WalkDependencies(dt))
            {
                if (d.Urn.Type.Equals("UnresolvedEntity")) continue;

                foreach (SqlSmoObject sso in smoObjects)
                {
                    if (sso.Urn.ToString().Equals(d.Urn, StringComparison.CurrentCultureIgnoreCase))
                    {
                        sorted[index++] = sso;
                        break;
                    }
                }
            }
            return sorted;
        }

        private SqlSmoObject[] GetTriggers(Server sourceServer, Database sourceDatabase)
        {
            // Database Triggers
            List<SqlSmoObject> triggers = new List<SqlSmoObject>();

            try
            {
                if (_ObjectSelector.Triggers.Script)
                {
                    if (sourceServer.ConnectionContext.ServerVersion.Major < 10)
                    {
                        if (sourceDatabase.Triggers.Count > 0)
                        {
                            foreach (DatabaseDdlTrigger trig in sourceDatabase.Triggers)
                            {
                                if (!trig.IsSystemObject && !trig.IsEncrypted)
                                {
                                    if (OkToScript(_ObjectSelector.Triggers, "", trig.Name))
                                    {
                                        triggers.Add(trig);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (SmoException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return triggers.ToArray();
        }

        private SqlSmoObject[] GetRoles(Database sourceDatabase)
        {
            List<SqlSmoObject> roles = new List<SqlSmoObject>();
            try
            {
                if (_ObjectSelector.Roles.Script)
                {
                    if (sourceDatabase.Roles.Count > 0)
                    {
                        foreach (DatabaseRole role in sourceDatabase.Roles)
                        {
                            switch (role.Name)
                            {
                                case "db_accessadmin":
                                case "db_backupoperator":
                                case "db_datareader":
                                case "db_datawriter":
                                case "db_ddladmin":
                                case "db_denydatareader":
                                case "db_denydatawriter":
                                case "db_owner":
                                case "db_securityadmin":
                                case "public":
                                    break;

                                default:
                                    if (OkToScript(_ObjectSelector.Roles, "", role.Name))
                                    {
                                        roles.Add(role);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (SmoException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return roles.ToArray();
        }

        private SqlSmoObject[] GetSchemas(Database sourceDatabase)
        {
            List<SqlSmoObject> schemas = new List<SqlSmoObject>();
            try
            {
                if (_ObjectSelector.Schemas.Script)
                {
                    if (sourceDatabase.Schemas.Count > 0)
                    {
                        foreach (Schema sch in sourceDatabase.Schemas)
                        {
                            switch (sch.Name)
                            {
                                case "sys":
                                case "INFORMATION_SCHEMA":
                                case "guest":
                                case "dbo":
                                case "db_securityadmin":
                                case "db_owner":
                                case "db_denydatawriter":
                                case "db_denydatareader":
                                case "db_ddladmin":
                                case "db_datawriter":
                                case "db_datareader":
                                case "db_backupoperator":
                                case "db_accessadmin":
                                    break;

                                default:
                                    if (OkToScript(_ObjectSelector.Schemas, "", sch.Name))
                                    {
                                        schemas.Add(sch);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (SmoException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return schemas.ToArray();
        }

        private SqlSmoObject[] GetSchemaCollections(Server sourceServer, Database sourceDatabase)
        {
            List<SqlSmoObject> schemaCollections = new List<SqlSmoObject>();
            try
            {
                if (_ObjectSelector.SchemaCollections.Script)
                {
                    if (sourceServer.ConnectionContext.DatabaseEngineType != DatabaseEngineType.SqlAzureDatabase) //_SourceServer.ServerType
                    {
                        // XML Schema Collections
                        if (sourceDatabase.XmlSchemaCollections.Count > 0)
                        {
                            foreach (XmlSchemaCollection xsc in sourceDatabase.XmlSchemaCollections)
                            {
                                if (OkToScript(_ObjectSelector.SchemaCollections, xsc.Schema, xsc.Name))
                                {
                                    schemaCollections.Add(xsc);
                                }
                            }
                        }
                    }
                }
            }
            catch (SmoException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return schemaCollections.ToArray();
        }

        private SqlSmoObject[] GetUDTs(Database sourceDatabase)
        {
            List<SqlSmoObject> uddts = new List<SqlSmoObject>();
            try
            {
                if (_ObjectSelector.UserDefinedDataTypes.Script)
                {
                    if (sourceDatabase.UserDefinedDataTypes.Count > 0)
                    {
                        foreach (UserDefinedDataType uddt in sourceDatabase.UserDefinedDataTypes)
                        {
                            if (OkToScript(_ObjectSelector.UserDefinedDataTypes, uddt.Schema, uddt.Name))
                            {
                                uddts.Add(uddt);
                            }
                        }
                    }
                }
            }
            catch (SmoException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return uddts.ToArray();
        }

        private SqlSmoObject[] GetUDTTs(Database sourceDatabase)
        {
            List<SqlSmoObject> udtts = new List<SqlSmoObject>();
            try
            {
                if (_ObjectSelector.UserDefinedTableTypes.Script)
                {
                    if (sourceDatabase.UserDefinedTableTypes.Count > 0)
                    {
                        foreach (UserDefinedTableType tt in sourceDatabase.UserDefinedTableTypes)
                        {
                            if (OkToScript(_ObjectSelector.UserDefinedTableTypes, tt.Schema, tt.Name))
                            {
                                udtts.Add(tt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return udtts.ToArray();
        }

        private SqlSmoObject[] GetSortedObjects(Database sourceDatabase)
        {
            // Ok, we need to get all of the selected objects and put them into one array
            // so that we can get them sorted by dependency.

            bool dataOnly = Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase);
            List<SqlSmoObject> needSorting = new List<SqlSmoObject>();

            try
            {
                // Stored Procedures
                if (!dataOnly && _ObjectSelector.StoredProcedures.Script && sourceDatabase.StoredProcedures.Count > 0)
                {
                    foreach (StoredProcedure sp in sourceDatabase.StoredProcedures)
                    {
                        if (!sp.IsSystemObject && !sp.IsEncrypted)
                        {
                            if (OkToScript(_ObjectSelector.StoredProcedures, sp.Schema, sp.Name))
                            {
                                needSorting.Add(sp);
                            }
                        }
                    }
                }

                // Tables
                if (_ObjectSelector.Tables.Script && sourceDatabase.Tables.Count > 0)
                {
                    foreach (Table tb in sourceDatabase.Tables)
                    {
                        if (!tb.IsSystemObject)
                        {
                            if (OkToScript(_ObjectSelector.Tables, tb.Schema, tb.Name))
                            {
                                needSorting.Add(tb);
                            }
                        }
                    }
                }

                // UDF
                if (!dataOnly && _ObjectSelector.UserDefinedFunctions.Script && sourceDatabase.UserDefinedFunctions.Count > 0)
                {
                    foreach (UserDefinedFunction udf in sourceDatabase.UserDefinedFunctions)
                    {
                        if (!udf.IsSystemObject && !udf.IsEncrypted)
                        {
                            if (OkToScript(_ObjectSelector.UserDefinedFunctions, udf.Schema, udf.Name))
                            {
                                needSorting.Add(udf);
                            }
                        }
                    }
                }

                // Views
                if (!dataOnly && _ObjectSelector.Views.Script && sourceDatabase.Views.Count > 0)
                {
                    foreach (Microsoft.SqlServer.Management.Smo.View vw in sourceDatabase.Views)
                    {
                        if (!vw.IsSystemObject && !vw.IsEncrypted)
                        {
                            if (OkToScript(_ObjectSelector.Views, vw.Schema, vw.Name))
                            {
                                needSorting.Add(vw);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return Sort(sourceDatabase, needSorting.ToArray());
        }

        private void ScriptDrops(SqlSmoObject[] sorted, SqlSmoObject[] smoRoles, SqlSmoObject[] smoSchemas, SqlSmoObject[] smoSchemasCols, SqlSmoObject[] smoUDTs, SqlSmoObject[] smoUDTTs, ScriptDatabase sdb)
        {
            int objCount = sorted.Count<SqlSmoObject>();
            if (objCount == 0) return;

            SqlSmoObject[] smoReverseSorted = new SqlSmoObject[objCount];
            for (int index = 0; index < objCount; index++)
            {
                smoReverseSorted[index] = sorted[objCount - 1 - index];
            }

            if (sorted.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoReverseSorted);
            if (smoUDTs.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoUDTs);
            if (smoUDTTs.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoUDTTs);
            if (smoSchemas.Count<SqlSmoObject>() > 0) sdb.ScriptDrops(smoSchemas);
            if (smoRoles.Count<SqlSmoObject>() > 0)
            {
                SqlSmoObject[] roles = new SqlSmoObject[1];
                foreach (SqlSmoObject role in smoRoles)
                {
                    roles[0] = role;
                    sdb.ScriptDrops(roles);
                }
            }
        }

        public void GenerateScriptFromSourceServer(Server sourceServer, Database sourceDatabase, StreamWriter swTSQL, AsyncUpdateStatus updateStatus, string outputDir, ObjectSelector objectFilter)
        {
            _updateStatus = updateStatus;
            _smoScriptOpts = SMOScriptOptions.CreateFromConfig();
            _ObjectSelector = objectFilter;

            DateTime dtStart = DateTime.Now;
            AsyncNotificationEventArgs eventArgs = new AsyncNotificationEventArgs(NotificationEventFunctionCode.GenerateScriptFromSQLServer, 0, "", CommonFunc.FormatString(Properties.Resources.MessageProcessStarted, dtStart.ToString(CultureInfo.CurrentUICulture), dtStart.ToUniversalTime().ToString(CultureInfo.CurrentUICulture)) + Environment.NewLine, Color.DarkBlue);

            _updateStatus(eventArgs);

            ScriptDatabase sdb = new ScriptDatabase();
            sdb.Initialize(sourceServer, sourceDatabase, _updateStatus, _smoScriptOpts, swTSQL);

            eventArgs.DisplayText = "";
            eventArgs.StatusMsg = Properties.Resources.MessageSorting;
            eventArgs.PercentComplete = 1;
            _updateStatus(eventArgs);

            // Tables, Views, Stored Procedures, and Triggers can all have dependencies.  GetSortedObjects returns
            // these objects in dependency order.

            SqlSmoObject[] smoTriggers = GetTriggers(sourceServer, sourceDatabase);
            SqlSmoObject[] smoRoles = GetRoles(sourceDatabase);
            SqlSmoObject[] smoSchemas = GetSchemas(sourceDatabase);
            SqlSmoObject[] smoSchemaCols = GetSchemaCollections(sourceServer, sourceDatabase);
            SqlSmoObject[] smoUDTs = GetUDTs(sourceDatabase);
            SqlSmoObject[] smoUDTTs = GetUDTTs(sourceDatabase);
            SqlSmoObject[] sorted = GetSortedObjects(sourceDatabase);

            if (Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDrop"), RegexOptions.IgnoreCase) ||
                Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDropCreate"), RegexOptions.IgnoreCase))
            {
                eventArgs.StatusMsg = Properties.Resources.MessageCreatingDropScripts;
                eventArgs.PercentComplete = 2;
                _updateStatus(eventArgs);

                ScriptDrops(sorted, smoRoles, smoSchemas, smoSchemaCols, smoUDTs, smoUDTTs, sdb);
            }

            if (Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSCreate"), RegexOptions.IgnoreCase) ||
                Regex.IsMatch(_smoScriptOpts.ScriptDropCreate, _smoScriptOpts.GetLocalizedStringValue("SOSDropCreate"), RegexOptions.IgnoreCase))
            {
                SourceProcessor sp = new SourceProcessor();
                sp.Initialize(sdb, _smoScriptOpts, _updateStatus, eventArgs, outputDir);

                // Roles, Schemas, XML Schema Collections and UDT have no dependencies.  Thus we process one at a time.

                if (!Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase))
                {
                    if (sp.Process(DatabaseObjectsTypes.Roles, smoRoles, 5)) return;
                    if (sp.Process(DatabaseObjectsTypes.Schemas, smoSchemas, 10)) return;
                    if (sp.Process(DatabaseObjectsTypes.XMLSchemaCollections, smoSchemaCols, 15)) return;
                    if (sp.Process(DatabaseObjectsTypes.UserDefinedDataTypes, smoUDTs, 20)) return;
                    if (sp.Process(DatabaseObjectsTypes.UserDefinedTableTypes, smoUDTTs, 25)) return;
                }
                if (sp.Process(sorted, 30)) return;

                if (!Regex.IsMatch(_smoScriptOpts.ScriptTableAndOrData, _smoScriptOpts.GetLocalizedStringValue("ScriptOptionsTableData"), RegexOptions.IgnoreCase))
                {
                    if (sp.Process(DatabaseObjectsTypes.Triggers, smoTriggers, 95)) return;
                }
            }

            if (swTSQL != null)
            {
                swTSQL.Flush();
                swTSQL.Close();
            }

            DateTime dtEnd = DateTime.Now;
            TimeSpan tsDuration = dtEnd.Subtract(dtStart);
            string sHour = tsDuration.Minutes == 1 ? Properties.Resources.MessageHour : Properties.Resources.MessageHours;
            string sMin = tsDuration.Minutes == 1 ? Properties.Resources.MessageMinute : Properties.Resources.MessageMinutes;
            string sSecs = tsDuration.Seconds == 1 ? Properties.Resources.MessageSecond : Properties.Resources.MessageSeconds;

            eventArgs.StatusMsg = Properties.Resources.Done;
            eventArgs.DisplayColor = Color.DarkCyan;

            if (_smoScriptOpts.CheckCompatibility() == 1)
            {
                eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageFinishedNoAnalysis, dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), tsDuration.Hours + sHour + tsDuration.Minutes.ToString(CultureInfo.CurrentUICulture) + sMin + tsDuration.Seconds.ToString(CultureInfo.CurrentUICulture) + sSecs);
            }
            else
            {
                eventArgs.DisplayText = CommonFunc.FormatString(Properties.Resources.MessageFinishedWithAnalysis, dtEnd.ToString(CultureInfo.CurrentUICulture), dtEnd.ToUniversalTime().ToString(CultureInfo.CurrentUICulture), tsDuration.Hours + sHour + tsDuration.Minutes.ToString(CultureInfo.CurrentUICulture) + sMin + tsDuration.Seconds.ToString(CultureInfo.CurrentUICulture) + sSecs);
            }
            eventArgs.PercentComplete = 100;
            _updateStatus(eventArgs);
        }
    }
}
