namespace SQLAzureMWUtils
{
    public enum NotificationEventFunctionCode
    {
        // 0 = BCPUploadData, 1 = ExecuteSQLonAzure, 2 = ParseFile, 4 = , 5 =
        BcpUploadData = 0,
        ExecuteSqlOnAzure = 1,
        ParseFile = 2,
        GenerateScriptFromSQLServer = 3,
        AnalysisOutput = 4,
        SqlOutput = 5,
        BcpDownloadData = 6
    }
}