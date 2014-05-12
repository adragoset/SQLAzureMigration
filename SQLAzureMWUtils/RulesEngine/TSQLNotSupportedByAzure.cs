using System;
using System.Collections;

namespace SQLAzureMWUtils
{
    public class TSQLNotSupportedByAzure : IConfigurationData
    {
        private string _defaultMessage;
        private SupportedStatementList _Skip;
        private NotSupportedTable _Table;
        private NotSupportedIndex _Index;
        private NotSupportedViews _Views;
        private NotSupportedSchema _Schema;
        private NotSupportedList _TSQL;
        private NotSupportedList _ActiveDirectorySP;
        private NotSupportedList _BackupandRestoreTable;
        private NotSupportedList _ChangeDataCapture;
        private NotSupportedList _ChangeDataCaptureTable;
        private NotSupportedList _DatabaseEngineSP;
        private NotSupportedList _DatabaseMailSP;
        private NotSupportedList _DatabaseMaintenancePlan;
        private NotSupportedList _DataControl;
        private NotSupportedList _DistributedQueriesSP;
        private NotSupportedList _FullTextSearchSP;
        private NotSupportedList _GeneralExtendedSPs;
        private NotSupportedList _IntegrationServicesTable;
        private NotSupportedList _LogShipping;
        private NotSupportedList _MetadataFunction;
        private NotSupportedList _OLEAutomationSP;
        private NotSupportedList _OLEDBTable;
        private NotSupportedList _ProfilerSP;
        private NotSupportedList _ReplicationSP;
        private NotSupportedList _ReplicationTable;
        private NotSupportedList _RowsetFunction;
        private NotSupportedList _SecurityFunction;
        private NotSupportedList _SecuritySP;
        private NotSupportedList _SQLMailSP;
        private NotSupportedList _SQLServerAgentSP;
        private NotSupportedList _SQLServerAgentTable;
        private NotSupportedList _SystemCatalogView;
        private NotSupportedList _SystemFunction;
        private NotSupportedList _SystemStatisticalFunction;
        private NotSupportedList _Unclassified;

        public string DefaultMessage
        {
            get { return _defaultMessage; }
            set { _defaultMessage = value; }
        }

        public NotSupportedSchema Schema
        {
            get { return _Schema; }
            set { _Schema = value; }
        }

        public SupportedStatementList Skip
        {
            get { return _Skip; }
            set { _Skip = value; }
        }

        public NotSupportedTable Table
        {
            get { return _Table; }
            set { _Table = value; }
        }

        public NotSupportedViews View
        {
            get { return _Views; }
            set { _Views = value; }
        }

        public NotSupportedIndex Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        public NotSupportedList GeneralTSQL
        {
            get { return _TSQL; }
            set { _TSQL = value; }
        }

        public NotSupportedList ActiveDirectorySP
        {
            get { return _ActiveDirectorySP; }
            set { _ActiveDirectorySP = value; }
        }

        public NotSupportedList BackupandRestoreTable
        {
            get { return _BackupandRestoreTable; }
            set { _BackupandRestoreTable = value; }
        }

        public NotSupportedList ChangeDataCapture
        {
            get { return _ChangeDataCapture; }
            set { _ChangeDataCapture = value; }
        }

        public NotSupportedList ChangeDataCaptureTable
        {
            get { return _ChangeDataCaptureTable; }
            set { _ChangeDataCaptureTable = value; }
        }

        public NotSupportedList DatabaseEngineSP
        {
            get { return _DatabaseEngineSP; }
            set { _DatabaseEngineSP = value; }
        }

        public NotSupportedList DatabaseMailSP
        {
            get { return _DatabaseMailSP; }
            set { _DatabaseMailSP = value; }
        }

        public NotSupportedList DatabaseMaintenancePlan
        {
            get { return _DatabaseMaintenancePlan; }
            set { _DatabaseMaintenancePlan = value; }
        }

        public NotSupportedList DataControl
        {
            get { return _DataControl; }
            set { _DataControl = value; }
        }

        public NotSupportedList DistributedQueriesSP
        {
            get { return _DistributedQueriesSP; }
            set { _DistributedQueriesSP = value; }
        }

        public NotSupportedList FullTextSearchSP
        {
            get { return _FullTextSearchSP; }
            set { _FullTextSearchSP = value; }
        }

        public NotSupportedList GeneralExtendedSPs
        {
            get { return _GeneralExtendedSPs; }
            set { _GeneralExtendedSPs = value; }
        }

        public NotSupportedList IntegrationServicesTable
        {
            get { return _IntegrationServicesTable; }
            set { _IntegrationServicesTable = value; }
        }

        public NotSupportedList LogShipping
        {
            get { return _LogShipping; }
            set { _LogShipping = value; }
        }

        public NotSupportedList MetadataFunction
        {
            get { return _MetadataFunction; }
            set { _MetadataFunction = value; }
        }

        public NotSupportedList OLEAutomationSP
        {
            get { return _OLEAutomationSP; }
            set { _OLEAutomationSP = value; }
        }

        public NotSupportedList OLEDBTable
        {
            get { return _OLEDBTable; }
            set { _OLEDBTable = value; }
        }

        public NotSupportedList ProfilerSP
        {
            get { return _ProfilerSP; }
            set { _ProfilerSP = value; }
        }

        public NotSupportedList ReplicationSP
        {
            get { return _ReplicationSP; }
            set { _ReplicationSP = value; }
        }

        public NotSupportedList ReplicationTable
        {
            get { return _ReplicationTable; }
            set { _ReplicationTable = value; }
        }

        public NotSupportedList RowsetFunction
        {
            get { return _RowsetFunction; }
            set { _RowsetFunction = value; }
        }

        public NotSupportedList SecurityFunction
        {
            get { return _SecurityFunction; }
            set { _SecurityFunction = value; }
        }

        public NotSupportedList SecuritySP
        {
            get { return _SecuritySP; }
            set { _SecuritySP = value; }
        }

        public NotSupportedList SQLMailSP
        {
            get { return _SQLMailSP; }
            set { _SQLMailSP = value; }
        }

        public NotSupportedList SQLServerAgentSP
        {
            get { return _SQLServerAgentSP; }
            set { _SQLServerAgentSP = value; }
        }

        public NotSupportedList SQLServerAgentTable
        {
            get { return _SQLServerAgentTable; }
            set { _SQLServerAgentTable = value; }
        }

        public NotSupportedList SystemCatalogView
        {
            get { return _SystemCatalogView; }
            set { _SystemCatalogView = value; }
        }

        public NotSupportedList SystemFunction
        {
            get { return _SystemFunction; }
            set { _SystemFunction = value; }
        }

        public NotSupportedList SystemStatisticalFunction
        {
            get { return _SystemStatisticalFunction; }
            set { _SystemStatisticalFunction = value; }
        }

        public NotSupportedList Unclassified
        {
            get { return _Unclassified; }
            set { _Unclassified = value; }
        }

        public TSQLNotSupportedByAzure()
		{
		}
    }
}
