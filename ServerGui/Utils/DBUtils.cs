using Server.Utils;
using ServerGui.Properties;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace ServerGui
{
    public static class DBUtils
    {
        public static void SetConnectionString()
        {
            SetSqlServerConnectionString("server", Settings.Default.allPurposeDatabaseName);
            SetSqlServerConnectionString("data", Settings.Default.dataDatabaseName);

            ConfigurationManager.RefreshSection("connectionStrings");
        }

        private static void SetSqlServerConnectionString(string appConfigKeyName, string databaseName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettings conSettings = config.ConnectionStrings.ConnectionStrings[appConfigKeyName];

            ////this is an extremely dirty hack that allows us to change the connection string at runtime
            //var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            //fi.SetValue(conSettings, false);

            conSettings.ConnectionString = GetSqlServerConnectionString(
                databaseName,
                Settings.Default.sqlServerHost,
                Settings.Default.sqlServerUsername,
                EncryptionUtils.Unprotect(Settings.Default.sqlServerPassword),
                useWindowsAuthentication: Settings.Default.sqlServerUseWindowsAuthentication);
            conSettings.ProviderName = "System.Data.SqlClient";

            config.Save();
        }

        public static SqlConnection CreateSqlServerConnection(string database = "server", string server = null, string username = null, string password = null, bool noDB = false, bool useWindowsAuthentication = true)
        {
            string connectionString = GetSqlServerConnectionString(database, server, username, password, noDB, useWindowsAuthentication);
            return new SqlConnection(connectionString);
        }

        internal static string GetSqlServerConnectionString(string database = "server", string server = null, string username = null, string password = null, bool noDB = false, bool useWindowsAuthentication = true)
        {
            string connectionString = $"Data Source={server ?? Settings.Default.sqlServerHost};";

            if (!noDB)
            {
                connectionString += $"Initial Catalog={database};";
            }

            if (!useWindowsAuthentication) //user/pass authentication
            {
                if (password == null)
                {
                    try
                    {
                        password = EncryptionUtils.Unprotect(Settings.Default.sqlServerPassword);
                    }
                    catch
                    {
                        password = "";
                    }
                }
                connectionString += $"User ID={username};Password={password};";
            }
            else //windows authentication
            {
                connectionString += "Integrated Security=True;";
            }
            return connectionString;
        }

        public static void CheckDBConnection()
        {
            //try to establish a database connection. If not possible, prompt the user to enter details
            var connection = DBUtils.CreateSqlServerConnection(noDB: true, useWindowsAuthentication: Settings.Default.sqlServerUseWindowsAuthentication);
           
            using (connection)
            {
                try
                {
                    
                    connection.Open();
                }
                catch (Exception)
                {
                    var dbDetailsWindow = new Windows.DBConnectionWindow();
                    dbDetailsWindow.ShowDialog();
                }
            }
          
        }
    }
}