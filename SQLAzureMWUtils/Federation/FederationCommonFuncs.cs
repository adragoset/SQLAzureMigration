using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Threading;

namespace SQLAzureMWUtils
{
    public static class FederationCommonFuncs
    {
        public static FederationMemberDistribution GetRootTables(TargetServerInfo targetServer)
        {
            string sqlQuery = "SELECT s.name, t.name FROM sys.tables t" +
                              "  JOIN sys.schemas s ON t.schema_id = s.schema_id" +
                              "  JOIN sys.dm_db_partition_stats p ON t.object_id=p.object_id" +
                              " WHERE p.index_id=1" +
                              " ORDER BY s.name, t.name;";
            FederationMemberDistribution md = new FederationMemberDistribution();
            md.FedType = "root";
            md.Low = "";
            md.High = "";
            md.DatabaseName = targetServer.RootDatabase;

            try
            {
                using (SqlConnection connection = new SqlConnection(targetServer.ConnectionStringRootDatabase))
                {
                    Retry.ExecuteRetryAction(() =>
                        {
                            connection.Open();
                            using (SqlDataReader sdr = SqlHelper.ExecuteReader(connection, CommandType.Text, sqlQuery))
                            {
                                if (sdr != null && !sdr.IsClosed)
                                {
                                    while (sdr.Read())
                                    {
                                        FederationTableInfo ti = new FederationTableInfo();
                                        ti.Schema = sdr.GetString(0);
                                        ti.Table = sdr.GetString(1);
                                        ti.FederatedColumn = "";
                                        md.Tables.Add(ti);
                                    }
                                    sdr.Close();
                                }
                            }
                            connection.Close();
                        });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return md;
        }

        public static string GetUseFederation(string federationName, ref FederationMemberDistribution member)
        {
            string tick = "";
            if (member.FedType.Equals("uniqueidentifier"))
            {
                tick = "'";
            }
            return "USE FEDERATION [" + federationName + "] (" + member.DistrubutionName + "=" + tick + member.Low + tick + ") WITH RESET, FILTERING=OFF";
        }

        public static string GetFederationMemberDatabaseName(string FederationName, FederationMemberDistribution member, string ConnectionStringRootDatabase)
        {
            string databaseName = "";
            string use = GetUseFederation(FederationName, ref member);

            Retry.ExecuteRetryAction(() =>
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionStringRootDatabase))
                    {
                        Retry.ExecuteRetryAction(() =>
                            {
                                connection.Open();
                                SqlHelper.ExecuteNonQuery(connection, CommandType.Text, use);
                                databaseName = (string)SqlHelper.ExecuteScalar(connection, CommandType.Text, "SELECT DB_NAME()").ExecuteScalarReturnValue;
                            },
                            () =>
                            {
                                connection.Close();
                            });
                    }
                });
            return databaseName;
        }
    }
}
