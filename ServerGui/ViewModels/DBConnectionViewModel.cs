using ReactiveUI;
using Server.Utils;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace ServerGui.ViewModels
{
    public class DBConnectionViewModel : ReactiveObject
    {
        private bool isSQLAuthenticationChecked;
        private bool isWindowsAuthenticationChecked = Properties.Settings.Default.sqlServerUseWindowsAuthentication;
        private ReactiveCommand<object, Unit> sqlServerTestCommand;
        private ReactiveCommand<object, Unit> sqlServerOkCommand;
        private string sqlServerHost = Properties.Settings.Default.sqlServerHost;
        private string sqlServerUsername;

        public DBConnectionViewModel()
        {
            SqlServerHost = string.IsNullOrWhiteSpace(Properties.Settings.Default.sqlServerHost) ? "localhost\\SQLEXPRESS" : Properties.Settings.Default.sqlServerHost;
        }

        public bool IsWindowsAuthenticationChecked
        {
            get { return isWindowsAuthenticationChecked; }
            set { this.RaiseAndSetIfChanged(ref isWindowsAuthenticationChecked, value); }
        }

        public bool IsSQLAuthenticationChecked
        {
            get { return isSQLAuthenticationChecked; }
            set { this.RaiseAndSetIfChanged(ref isSQLAuthenticationChecked, value); }
        }

        public string SqlServerHost
        {
            get { return sqlServerHost; }
            set { this.RaiseAndSetIfChanged(ref sqlServerHost, value); }
        }

        public string SqlServerUsername
        {
            get { return sqlServerUsername; }
            set { this.RaiseAndSetIfChanged(ref sqlServerUsername, value); }
        }

        public ReactiveCommand<object, Unit> SqlServerOkCommand => sqlServerOkCommand ?? (sqlServerOkCommand = ReactiveCommand.Create<object>(SaveConnection, this.WhenAnyValue(x=>x.IsTestConnectionSuccessull)));

        public ReactiveCommand<object, Unit> SqlServerTestCommand => sqlServerTestCommand ?? (sqlServerTestCommand = ReactiveCommand.Create<object>(TestConnection));

        private bool savedConfig = false;
        private bool isTestConnectionSuccessull;

        public bool SavedConfig
        {
            get { return savedConfig; }
            set { this.RaiseAndSetIfChanged(ref savedConfig, value); }
        }

        public bool IsTestConnectionSuccessull
        {
            get { return isTestConnectionSuccessull; }
            set { this.RaiseAndSetIfChanged(ref isTestConnectionSuccessull,   value); }
        }

        private void TestConnection(object passwordBoxParameter)
        {
            string password = string.Empty;
            if (IsSQLAuthenticationChecked)
            {
                var passwordBox = passwordBoxParameter as PasswordBox;
                password = passwordBox?.Password;
            }

            SqlConnection connection = DBUtils.CreateSqlServerConnection(
                server: SqlServerHost,
                username: SqlServerUsername,
                password: string.IsNullOrWhiteSpace(password) ? "" : password,
                useWindowsAuthentication: IsWindowsAuthenticationChecked,
                noDB: true);

            using (connection)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection failed with error: " + ex.Message);
                    IsTestConnectionSuccessull = false;
                    return;
                }
            }

            IsTestConnectionSuccessull = true;
            MessageBox.Show("Connection succeeded.");
        }

        private void SaveConnection(object passwordBoxParameter)
        {
            string password = string.Empty;
            if (IsSQLAuthenticationChecked)
            {
                var passwordBox = passwordBoxParameter as PasswordBox;
                password = passwordBox?.Password;
            }

            SqlConnection connection = DBUtils.CreateSqlServerConnection(
               server: SqlServerHost,
               username: SqlServerUsername,
               password: string.IsNullOrWhiteSpace(password) ? "" : password,
               useWindowsAuthentication: IsWindowsAuthenticationChecked,
               noDB: true);
            using (connection)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection failed with error: " + ex.Message);
                    return;
                }
            }

            Properties.Settings.Default.sqlServerHost = SqlServerHost;
            Properties.Settings.Default.sqlServerUsername = SqlServerUsername;
            Properties.Settings.Default.sqlServerPassword = EncryptionUtils.Protect(password);
            Properties.Settings.Default.databaseType = "SqlServer";

            Properties.Settings.Default.Save();

            SavedConfig = true;
        }
    }
}