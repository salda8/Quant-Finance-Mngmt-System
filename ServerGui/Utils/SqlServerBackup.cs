using System.Data.SqlClient;

namespace ServerGui
{
    public static class SqlServerBackup
    {
        public static void Backup(string connectionString, string database, string file)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("", conn);
                cmd.CommandText = string.Format(
                                        @"USE {0};
                                        GO
                                        BACKUP DATABASE {0}
                                        TO DISK = '{1}'
                                           WITH FORMAT,
                                              NAME = 'QDMS Server backup of {0}';
                                        GO",
                                        database,
                                        file);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Restore(string connectionString, string database, string file)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("", conn) { CommandText = $@"USE master
                                        GO
                                        RESTORE DATABASE {database}
                                            FROM DISK = '{file}'
                                        WITH REPLACE
                                        GO" };
                cmd.ExecuteNonQuery();
            }
        }
    }
}