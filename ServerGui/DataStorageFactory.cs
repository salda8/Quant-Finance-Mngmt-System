using Common;
using Common.Interfaces;
using DataStorage.SqlServer;

namespace ServerGui
{
    public static class DataStorageFactory
    {
        public static IDataStorage Get() => new SqlServerStorage(DBUtils.GetSqlServerConnectionString(Properties.Settings.Default.dataDatabaseName, useWindowsAuthentication: Properties.Settings.Default.sqlServerUseWindowsAuthentication));
    }
}