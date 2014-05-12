using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace SQLAzureMWUtils
{
    public class CheckCompatibilityList : StringConverter
    {
        public CheckCompatibilityList()
		{
		}

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { Properties.Resources.ScriptOptionsOverride, Properties.Resources.ScriptOptionsUseDefault, Properties.Resources.ScriptOptionsOverrideNot });
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public class TargetServerOptions : StringConverter
    {
        public TargetServerOptions()
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { Properties.Resources.ServerType_SQLAzure, Properties.Resources.ServerType_SQLAzureFed, Properties.Resources.ServerType_SQLServer });
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public class SqlScriptingOptions : StringConverter
    {
        public SqlScriptingOptions()
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { Properties.Resources.SOSCreate, Properties.Resources.SOSDropCreate, Properties.Resources.SOSDrop });
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public class ScriptingDataOptions : StringConverter
    {
        public ScriptingDataOptions()
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { Properties.Resources.ScriptOptionsTableSchema, Properties.Resources.ScriptOptionsTableSchemaData, Properties.Resources.ScriptOptionsTableData });
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    internal sealed class LocalizedCategoryAttribute : CategoryAttribute
    {
        private static global::System.Resources.ResourceManager resourceMan;
        internal static global::System.Resources.ResourceManager RM
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    resourceMan = new global::System.Resources.ResourceManager("SQLAzureMWUtils.Properties.Resources", Assembly.GetExecutingAssembly());
                }
                return resourceMan;
            }
        }

        internal LocalizedCategoryAttribute(string categoryValue) : base(categoryValue)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return RM.GetString(value, CultureInfo.CurrentUICulture);
        }
    }

    internal sealed class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public string localizedDisplayName;
        private static global::System.Resources.ResourceManager resourceMan;
        internal static global::System.Resources.ResourceManager RM
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    resourceMan = new global::System.Resources.ResourceManager("SQLAzureMWUtils.Properties.Resources", Assembly.GetExecutingAssembly());
                }
                return resourceMan;
            }
        }

        internal LocalizedDisplayNameAttribute(string name) : base(name)
        {
            string ldn = RM.GetString(name, CultureInfo.CurrentUICulture);
            if (ldn != null)
            {
                localizedDisplayName = ldn;
            }
            else
            {
                localizedDisplayName = name;
            }
        }

        public override string DisplayName
        {
            get
            {
                return localizedDisplayName;
            }
        }
    }

    internal sealed class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private string localizedDescription;
        private static global::System.Resources.ResourceManager resourceMan;
        internal static global::System.Resources.ResourceManager RM
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    resourceMan = new global::System.Resources.ResourceManager("SQLAzureMWUtils.Properties.Resources", Assembly.GetExecutingAssembly());
                }
                return resourceMan;
            }
        }

        internal LocalizedDescriptionAttribute(string description) : base(description)
        {
            string des = RM.GetString(description, CultureInfo.CurrentUICulture);
            if (des != null)
            {
                localizedDescription = des;
            }
            else
            {
                localizedDescription = description;
            }
        }

        public override string Description
        {
            get
            {
                return localizedDescription;
            }
        }
    }

    internal sealed class LocalizedDefaultValueAttribute : DefaultValueAttribute
    {
        private string localizedValue;
        private static global::System.Resources.ResourceManager resourceMan;
        internal static global::System.Resources.ResourceManager RM
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    resourceMan = new global::System.Resources.ResourceManager("SQLAzureMWUtils.Properties.Resources", Assembly.GetExecutingAssembly());
                }
                return resourceMan;
            }
        }

        internal LocalizedDefaultValueAttribute(string value) : base(value)
        {
            string val = RM.GetString(value, CultureInfo.CurrentUICulture);
            if (val != null)
            {
                localizedValue = val;
            }
            else
            {
                localizedValue = value;
            }
        }

        public override object Value
        {
            get
            {
                return localizedValue;
            }
        }
    }

    public class SMOScriptOptions 
    {
        private static global::System.Resources.ResourceManager resourceMan;
        public bool Migrate = true;

        private bool _scriptDefaults = true;
        private bool _scriptHeaders = false;
        private bool _includeIfNotExists = true;

        private string _scriptDropCreate = Properties.Resources.SOSCreate;
        private string _TargetServer = Properties.Resources.ServerType_SQLAzure;

        private string _scriptTableAndData = Properties.Resources.ScriptOptionsTableSchemaData;
        private bool _scriptCheckConstraints = true;
        private bool _scriptCollation = true;
        private bool _scriptForeignKeys = true;
        private bool _scriptPrimaryKeys = true;
        private bool _scriptUniqueKeys = true;
        private bool _scriptIndexes = true;
        private bool _scriptTableTriggers = true;

        private string _compatibilityChecks = Properties.Resources.ScriptOptionsOverride;
        private bool _ActiveDirectorySP = false;
        private bool _BackupandRestoreTable = false;
        private bool _ChangeDataCapture = false;
        private bool _ChangeDataCaptureTable = false;
        private bool _DatabaseEngineSP = false;
        private bool _DatabaseMailSP = false;
        private bool _DatabaseMaintenancePlan = false;
        private bool _DataControl = false;
        private bool _DistributedQueriesSP = false;
        private bool _FullTextSearchSP = false;
        private bool _GeneralExtendedSPs = false;
        private bool _GeneralTSQL = true;
        private bool _IntegrationServicesTable = false;
        private bool _LogShipping = false;
        private bool _MetadataFunction = false;
        private bool _OLEAutomationSP = false;
        private bool _OLEDBTable = false;
        private bool _ProfilerSP = false;
        private bool _ReplicationSP = false;
        private bool _ReplicationTable = false;
        private bool _RowsetFunction = false;
        private bool _SecurityFunction = false;
        private bool _SecuritySP = false;
        private bool _SQLMailSP = false;
        private bool _SQLServerAgentSP = false;
        private bool _SQLServerAgentTable = false;
        private bool _SystemCatalogView = false;
        private bool _SystemFunction = false;
        private bool _SystemStatisticalFunction = false;
        private bool _Unclassified = false;

        public SMOScriptOptions()
        {
            resourceMan = new global::System.Resources.ResourceManager("SQLAzureMWUtils.Properties.Resources", Assembly.GetExecutingAssembly());
        }

        [TypeConverterAttribute(typeof(TargetServerOptions)), LocalizedCategoryAttribute("ScriptOptionsGeneral"), LocalizedDisplayNameAttribute("TargetServer"), LocalizedDefaultValueAttribute("ServerType_SQLAzure"), LocalizedDescriptionAttribute("TargetServerDesc")]
        public string TargetServer
        {
            get { return _TargetServer; }
            set { _TargetServer = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsGeneral"), LocalizedDisplayNameAttribute("SOSDefaults"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSDefaultsDesc")]
        public bool ScriptDefaults
        {
            get { return _scriptDefaults; }
            set { _scriptDefaults = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsGeneral"), LocalizedDisplayNameAttribute("SOSHeaders"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSHeadersDesc")]
        public bool ScriptHeaders
        {
            get { return _scriptHeaders; }
            set { _scriptHeaders = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsGeneral"), LocalizedDisplayNameAttribute("SOSNotExists"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSNotExistsDesc")]
        public bool IncludeIfNotExists
        {
            get { return _includeIfNotExists; }
            set { _includeIfNotExists = value; }
        }

        [TypeConverterAttribute(typeof(SqlScriptingOptions)), LocalizedCategoryAttribute("ScriptOptionsGeneral"), LocalizedDisplayNameAttribute("SOSDropCreate"), LocalizedDefaultValueAttribute("SOSCreate"), LocalizedDescriptionAttribute("SOSDropCreateDesc")]
        public string ScriptDropCreate
        {
            get { return _scriptDropCreate; }
            set { _scriptDropCreate = value; }
        }

        [TypeConverterAttribute(typeof(ScriptingDataOptions)), LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("ScriptOptionsTableDataDisplayName"), LocalizedDefaultValueAttribute("ScriptOptionsTableSchemaData"), LocalizedDescriptionAttribute("ScriptOptionsTableDataDesc")]
        public string ScriptTableAndOrData
        {
            get { return _scriptTableAndData; }
            set
            {
                if (value != null)
                {
                    _scriptTableAndData = value;
                }
                else
                {
                    _scriptTableAndData = "";
                }
            }
        }

        public bool ScriptTable()
        {
            if (_scriptTableAndData.Equals(Properties.Resources.ScriptOptionsTableData, StringComparison.Ordinal))
            {
                return false;
            }
            return true;
        }

        public bool ScriptTableData()
        {
            if (_scriptTableAndData.Equals(Properties.Resources.ScriptOptionsTableSchema, StringComparison.Ordinal))
            {
                return false;
            }
            return true;
        }

        [LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("SOSCheckDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSCheckDesc")]
        public bool ScriptCheckConstraints
        {
            get { return _scriptCheckConstraints; }
            set { _scriptCheckConstraints = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("SOSScriptCollationDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSScriptCollationDesc")]
        public bool ScriptCollation
        {
            get { return _scriptCollation; }
            set { _scriptCollation = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("SOSIndexesDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSIndexesDesc")]
        public bool ScriptIndexes
        {
            get { return _scriptIndexes; }
            set { _scriptIndexes = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("SOSForeignKeysDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSForeignKeysDesc")]
        public bool ScriptForeignKeys
        {
            get { return _scriptForeignKeys; }
            set { _scriptForeignKeys = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("SOSPrimaryKeysDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSPrimaryKeysDesc")]
        public bool ScriptPrimaryKeys
        {
            get { return _scriptPrimaryKeys; }
            set { _scriptPrimaryKeys = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("SOSUniqueKeysDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSUniqueKeysDesc")]
        public bool ScriptUniqueKeys
        {
            get { return _scriptUniqueKeys; }
            set { _scriptUniqueKeys = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTableView"), LocalizedDisplayNameAttribute("SOSTriggersDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSTriggersDesc")]
        public bool ScriptTableTriggers
        {
            get { return _scriptTableTriggers; }
            set { _scriptTableTriggers = value; }
        }

        [TypeConverterAttribute(typeof(CheckCompatibilityList)), LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSCompatibilityDisplayName"), LocalizedDefaultValueAttribute("ScriptOptionsOverride"), LocalizedDescriptionAttribute("SOSCompatibilityDesc")]
        public string CompatibilityChecks
        {
            get { return _compatibilityChecks; }
            set { _compatibilityChecks = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSADSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSADSPDesc")]
        public bool ActiveDirectorySP
        {
            get { return _ActiveDirectorySP; }
            set { _ActiveDirectorySP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSBackupRestoreDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSBackupRestoreDesc")]
        public bool BackupandRestoreTable
        {
            get { return _BackupandRestoreTable; }
            set { _BackupandRestoreTable = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSCDCDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSCDCDesc")]
        public bool ChangeDataCapture
        {
            get { return _ChangeDataCapture; }
            set { _ChangeDataCapture = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSCDCTDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSCDCTDesc")]
        public bool ChangeDataCaptureTable
        {
            get { return _ChangeDataCaptureTable; }
            set { _ChangeDataCaptureTable = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSDBEngineSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("Database Engine Stored Procedures.")]
        public bool DatabaseEngineSP
        {
            get { return _DatabaseEngineSP; }
            set { _DatabaseEngineSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSDBMailSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSDBMailSPDesc")]
        public bool DatabaseMailSP
        {
            get { return _DatabaseMailSP; }
            set { _DatabaseMailSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSDBMaintDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSDBMaintDesc")]
        public bool DatabaseMaintenancePlan
        {
            get { return _DatabaseMaintenancePlan; }
            set { _DatabaseMaintenancePlan = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSDBDCDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSDBDCDesc")]
        public bool DataControl
        {
            get { return _DataControl; }
            set { _DataControl = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSDQSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSDQSPDesc")]
        public bool DistributedQueriesSP
        {
            get { return _DistributedQueriesSP; }
            set { _DistributedQueriesSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSFTSSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSFTSSPDisplayName")]
        public bool FullTextSearchSP
        {
            get { return _FullTextSearchSP; }
            set { _FullTextSearchSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSGenExtSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSGenExtSPDesc")]
        public bool GeneralExtendedSPs
        {
            get { return _GeneralExtendedSPs; }
            set { _GeneralExtendedSPs = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSGenTSQLDisplayName"), DefaultValueAttribute(true), LocalizedDescriptionAttribute("SOSGenTSQLDesc")]
        public bool GeneralTSQL
        {
            get { return _GeneralTSQL; }
            set { _GeneralTSQL = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSISTDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSISTDesc")]
        public bool IntegrationServicesTable
        {
            get { return _IntegrationServicesTable; }
            set { _IntegrationServicesTable = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSLSDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSLSDesc")]
        public bool LogShipping
        {
            get { return _LogShipping; }
            set { _LogShipping = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSMFDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSMFDesc")]
        public bool MetadataFunction
        {
            get { return _MetadataFunction; }
            set { _MetadataFunction = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSOLESPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSOLESPDesc")]
        public bool OLEAutomationSP
        {
            get { return _OLEAutomationSP; }
            set { _OLEAutomationSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSOLEDBDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSOLEDBDesc")]
        public bool OLEDBTable
        {
            get { return _OLEDBTable; }
            set { _OLEDBTable = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSProfilerSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSProfilerSPDesc")]
        public bool ProfilerSP
        {
            get { return _ProfilerSP; }
            set { _ProfilerSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSReplSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSReplSPDesc")]
        public bool ReplicationSP
        {
            get { return _ReplicationSP; }
            set { _ReplicationSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSReplTabDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSReplTabDisplayName")]
        public bool ReplicationTable
        {
            get { return _ReplicationTable; }
            set { _ReplicationTable = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSRowsetDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSRowsetDesc")]
        public bool RowsetFunction
        {
            get { return _RowsetFunction; }
            set { _RowsetFunction = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSSecurityDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSSecurityDesc")]
        public bool SecurityFunction
        {
            get { return _SecurityFunction; }
            set { _SecurityFunction = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSSecuritySPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSSecuritySPDisplayName")]
        public bool SecuritySP
        {
            get { return _SecuritySP; }
            set { _SecuritySP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSMailSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSMailSPDesc")]
        public bool SQLMailSP
        {
            get { return _SQLMailSP; }
            set { _SQLMailSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSAgentSPDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSAgentSPDesc")]
        public bool SQLServerAgentSP
        {
            get { return _SQLServerAgentSP; }
            set { _SQLServerAgentSP = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSAgentTDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSAgentTDesc")]
        public bool SQLServerAgentTable
        {
            get { return _SQLServerAgentTable; }
            set { _SQLServerAgentTable = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSCatalogVDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSCatalogVDisplayName")]
        public bool SystemCatalogView
        {
            get { return _SystemCatalogView; }
            set { _SystemCatalogView = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSSysFunDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSSysFunDesc")]
        public bool SystemFunction
        {
            get { return _SystemFunction; }
            set { _SystemFunction = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSSysStatDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSSysStatDesc")]
        public bool SystemStatisticalFunction
        {
            get { return _SystemStatisticalFunction; }
            set { _SystemStatisticalFunction = value; }
        }

        [LocalizedCategoryAttribute("ScriptOptionsTSQLCompatibility"), LocalizedDisplayNameAttribute("SOSUnclassifiedDisplayName"), DefaultValueAttribute(false), LocalizedDescriptionAttribute("SOSUnclassifiedDesc")]
        public bool Unclassified
        {
            get { return _Unclassified; }
            set { _Unclassified = value; }
        }

        public int CheckCompatibility()
        {
            if (_TargetServer.Equals(Properties.Resources.ServerType_SQLServer, StringComparison.Ordinal) || _compatibilityChecks.Equals(Properties.Resources.ScriptOptionsOverrideNot, StringComparison.Ordinal))
            {
                return 1;
            }

            if (_compatibilityChecks.Equals(Properties.Resources.ScriptOptionsOverride, StringComparison.Ordinal))
            {
                return 0;
            }
            return 2;
        }

        public bool IsTargetServerFederation()
        {
            return _TargetServer.Equals(Properties.Resources.ServerType_SQLAzureFed, StringComparison.OrdinalIgnoreCase);
        }

        public string GetAppConfigCompabilitySetting()
        {
            LocalizedDefaultValueAttribute cc = new LocalizedDefaultValueAttribute(ConfigurationManager.AppSettings["CompatibilityChecks"]);
            return (string)cc.Value;
        }

        public string GetLocalizedStringValue(string keyName)
        {
            LocalizedDefaultValueAttribute localized = new LocalizedDefaultValueAttribute(keyName);
            return (string)localized.Value;
        }

        public static SMOScriptOptions CreateFromConfig()
        {
            var smoScriptOpts = new SMOScriptOptions();
            LocalizedDefaultValueAttribute target = new LocalizedDefaultValueAttribute(CommonFunc.GetAppSettingsStringValue("TargetServerType"));
            if (target != null && target.Value.ToString().Length > 0)
            {
                smoScriptOpts.TargetServer = (string)target.Value;
            }

            smoScriptOpts.ScriptDefaults = CommonFunc.GetAppSettingsBoolValue("ScriptDefaults");
            smoScriptOpts.ScriptHeaders = CommonFunc.GetAppSettingsBoolValue("ScriptHeaders");
            smoScriptOpts.IncludeIfNotExists = CommonFunc.GetAppSettingsBoolValue("IncludeIfNotExists");

            LocalizedDefaultValueAttribute sdc = new LocalizedDefaultValueAttribute(ConfigurationManager.AppSettings["ScriptDropCreate"]);
            smoScriptOpts.ScriptDropCreate = (string)sdc.Value;

            smoScriptOpts.ScriptCheckConstraints = CommonFunc.GetAppSettingsBoolValue("ScriptCheckConstraints");
            smoScriptOpts.ScriptCollation = CommonFunc.GetAppSettingsBoolValue("ScriptCollation");
            smoScriptOpts.ScriptForeignKeys = CommonFunc.GetAppSettingsBoolValue("ScriptForeignKeys");
            smoScriptOpts.ScriptPrimaryKeys = CommonFunc.GetAppSettingsBoolValue("ScriptPrimaryKeys");
            smoScriptOpts.ScriptUniqueKeys = CommonFunc.GetAppSettingsBoolValue("ScriptUniqueKeys");
            smoScriptOpts.ScriptIndexes = CommonFunc.GetAppSettingsBoolValue("ScriptIndexes");

            LocalizedDefaultValueAttribute stad = new LocalizedDefaultValueAttribute(ConfigurationManager.AppSettings["ScriptTableAndOrData"]);
            smoScriptOpts.ScriptTableAndOrData = (string)stad.Value;

            if (smoScriptOpts.ScriptTableAndOrData.Length == 0)
            {
                if (CommonFunc.GetAppSettingsBoolValue("ScriptData"))
                {
                    smoScriptOpts.ScriptTableAndOrData = Properties.Resources.ScriptOptionsTableSchemaData;
                }
                else
                {
                    smoScriptOpts.ScriptTableAndOrData = Properties.Resources.ScriptOptionsTableSchema;
                }
            }

            smoScriptOpts.ScriptTableTriggers = CommonFunc.GetAppSettingsBoolValue("ScriptTableTriggers");

            smoScriptOpts.ActiveDirectorySP = CommonFunc.GetAppSettingsBoolValue("ActiveDirectorySP");
            smoScriptOpts.BackupandRestoreTable = CommonFunc.GetAppSettingsBoolValue("BackupandRestoreTable");
            smoScriptOpts.ChangeDataCapture = CommonFunc.GetAppSettingsBoolValue("ChangeDataCapture");
            smoScriptOpts.ChangeDataCaptureTable = CommonFunc.GetAppSettingsBoolValue("ChangeDataCaptureTable");

            LocalizedDefaultValueAttribute cc = new LocalizedDefaultValueAttribute(ConfigurationManager.AppSettings["CompatibilityChecks"]);
            smoScriptOpts.CompatibilityChecks = (string)cc.Value;

            smoScriptOpts.DatabaseEngineSP = CommonFunc.GetAppSettingsBoolValue("DatabaseEngineSP");
            smoScriptOpts.DatabaseMailSP = CommonFunc.GetAppSettingsBoolValue("DatabaseMailSP");
            smoScriptOpts.DatabaseMaintenancePlan = CommonFunc.GetAppSettingsBoolValue("DatabaseMaintenancePlan");
            smoScriptOpts.DataControl = CommonFunc.GetAppSettingsBoolValue("DataControl");
            smoScriptOpts.DistributedQueriesSP = CommonFunc.GetAppSettingsBoolValue("DistributedQueriesSP");
            smoScriptOpts.FullTextSearchSP = CommonFunc.GetAppSettingsBoolValue("FullTextSearchSP");
            smoScriptOpts.GeneralExtendedSPs = CommonFunc.GetAppSettingsBoolValue("GeneralExtendedSPs");
            smoScriptOpts.GeneralTSQL = CommonFunc.GetAppSettingsBoolValue("GeneralTSQL");
            smoScriptOpts.IntegrationServicesTable = CommonFunc.GetAppSettingsBoolValue("IntegrationServicesTable");
            smoScriptOpts.LogShipping = CommonFunc.GetAppSettingsBoolValue("LogShipping");
            smoScriptOpts.MetadataFunction = CommonFunc.GetAppSettingsBoolValue("MetadataFunction");
            smoScriptOpts.OLEAutomationSP = CommonFunc.GetAppSettingsBoolValue("OLEAutomationSP");
            smoScriptOpts.OLEDBTable = CommonFunc.GetAppSettingsBoolValue("OLEDBTable");
            smoScriptOpts.ProfilerSP = CommonFunc.GetAppSettingsBoolValue("ProfilerSP");
            smoScriptOpts.ReplicationSP = CommonFunc.GetAppSettingsBoolValue("ReplicationSP");
            smoScriptOpts.ReplicationTable = CommonFunc.GetAppSettingsBoolValue("ReplicationTable");
            smoScriptOpts.RowsetFunction = CommonFunc.GetAppSettingsBoolValue("RowsetFunction");
            smoScriptOpts.SecurityFunction = CommonFunc.GetAppSettingsBoolValue("SecurityFunction");
            smoScriptOpts.SecuritySP = CommonFunc.GetAppSettingsBoolValue("SecuritySP");
            smoScriptOpts.SQLMailSP = CommonFunc.GetAppSettingsBoolValue("SQLMailSP");
            smoScriptOpts.SQLServerAgentSP = CommonFunc.GetAppSettingsBoolValue("SQLServerAgentSP");
            smoScriptOpts.SQLServerAgentTable = CommonFunc.GetAppSettingsBoolValue("SQLServerAgentTable");
            smoScriptOpts.SystemCatalogView = CommonFunc.GetAppSettingsBoolValue("SystemCatalogView");
            smoScriptOpts.SystemFunction = CommonFunc.GetAppSettingsBoolValue("SystemFunction");
            smoScriptOpts.SystemStatisticalFunction = CommonFunc.GetAppSettingsBoolValue("SystemStatisticalFunction");
            smoScriptOpts.Unclassified = CommonFunc.GetAppSettingsBoolValue("Unclassified");

            return smoScriptOpts;
        }
    }
}
