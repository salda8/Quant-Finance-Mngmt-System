using NLog;
using NLog.Targets;
using ReactiveUI;
using Server.Utils;
using ServerGui.Properties;
using System;
using System.IO;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace ServerGui.ViewModels
{
    public class SettingsViewModel : ReactiveObject
    {
        private int realTimeDataServerPublishPort = Settings.Default.RealTimeDataServerPublishPort;
        private int realTimeDataServerRequestPort = Settings.Default.RealTimeDataServerRequestPort;
        private int historicalServerPort = Settings.Default.HistoricalServerPort;
        private int messagesServerPullPort = Settings.Default.MessagesServerPullPort;
        private int messagesServerPushPort = Settings.Default.MessagesServerPushPort;
        private int equityUpdateServerRouterPort = Settings.Default.EquityUpdateServerRouterPort;
        private string logFolder = Settings.Default.logDirectory;
        private string ibClientHost = Settings.Default.ibClientHost;
        private int ibClientPort = Settings.Default.ibClientPort;
        private int ibHistClientID = Settings.Default.histClientIBID;
        private int ibRTDClientID = Settings.Default.rtdClientIBID;
        private bool isWindowsAuthenticationChecked = Settings.Default.sqlServerUseWindowsAuthentication;
        private bool isSQLAuthenticationChecked = !Settings.Default.sqlServerUseWindowsAuthentication;
        private string sqlServerHost = Settings.Default.sqlServerHost;
        private string sqlServerUsername = Settings.Default.sqlServerUsername;
        //private string sqlPassword = EncryptionUtils.Unprotect(Settings.Default.sqlServerPassword);
        private ReactiveCommand<object, Unit> saveCommand;

        public int RealTimeDataServerPublishPort
        {
            get { return realTimeDataServerPublishPort; }
            set { this.RaiseAndSetIfChanged(ref realTimeDataServerPublishPort, value); }
        }

        public int RealTimeDataServerRequestPort
        {
            get { return realTimeDataServerRequestPort; }
            set { this.RaiseAndSetIfChanged(ref realTimeDataServerRequestPort, value); }
        }

        public int HistoricalServerPort
        {
            get { return historicalServerPort; }
            set { this.RaiseAndSetIfChanged(ref historicalServerPort, value); }
        }

        public int MessagesServerPullPort
        {
            get { return messagesServerPullPort; }
            set { this.RaiseAndSetIfChanged(ref messagesServerPullPort, value); }
        }

        public int MessagesServerPushPort
        {
            get { return messagesServerPushPort; }
            set { this.RaiseAndSetIfChanged(ref messagesServerPushPort, value); }
        }

        public int EquityUpdateServerRouterPort
        {
            get { return equityUpdateServerRouterPort; }
            set { this.RaiseAndSetIfChanged(ref equityUpdateServerRouterPort, value); }
        }

        public string LogFolder
        {
            get { return logFolder; }
            set { this.RaiseAndSetIfChanged(ref logFolder, value); }
        }

        public string IbClientHost
        {
            get { return ibClientHost; }
            set { this.RaiseAndSetIfChanged(ref ibClientHost, value); }
        }

        public int IbClientPort
        {
            get { return ibClientPort; }
            set { this.RaiseAndSetIfChanged(ref ibClientPort, value); }
        }

        public int IbHistClientID
        {
            get { return ibHistClientID; }
            set { this.RaiseAndSetIfChanged(ref ibHistClientID, value); }
        }

        public int IbRTDClientID
        {
            get { return ibRTDClientID; }
            set { this.RaiseAndSetIfChanged(ref ibRTDClientID, value); }
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

        //public string SqlPassword
        //{
        //    get { return sqlPassword; }
        //    set { this.RaiseAndSetIfChanged(ref sqlPassword, value); }
        //}

        public ReactiveCommand<object, Unit> SaveCommand => saveCommand ?? (saveCommand = ReactiveCommand.Create<object>(Save));

        private void Save(object passwordBoxParameter)
        {
            Settings.Default.RealTimeDataServerPublishPort = RealTimeDataServerPublishPort;

            Settings.Default.RealTimeDataServerRequestPort = RealTimeDataServerRequestPort;

            Settings.Default.HistoricalServerPort = HistoricalServerPort;

            Settings.Default.MessagesServerPullPort = MessagesServerPullPort;
            Settings.Default.MessagesServerPushPort = MessagesServerPushPort;
            Settings.Default.EquityUpdateServerRouterPort = EquityUpdateServerRouterPort;

            Settings.Default.ibClientHost = IbClientHost;
            Settings.Default.ibClientPort = ibClientPort;
            Settings.Default.histClientIBID = ibHistClientID;
            Settings.Default.rtdClientIBID = ibRTDClientID;
            
            try
            {
                if (!Directory.Exists(LogFolder))
                {
                    Directory.CreateDirectory(LogFolder);
                }
                Settings.Default.logDirectory = LogFolder;
                ((FileTarget)LogManager.Configuration.FindTargetByName("default")).FileName = LogFolder + "Log.log";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting log directory: " + ex.Message);
            }

            Settings.Default.databaseType = "SqlServer";

            string password = string.Empty;
            if (IsSQLAuthenticationChecked)
            {
                var passwordBox = passwordBoxParameter as PasswordBox;
                password = passwordBox?.Password;
            }


            Settings.Default.sqlServerUseWindowsAuthentication = isWindowsAuthenticationChecked;
            Settings.Default.sqlServerHost = SqlServerHost;
            Settings.Default.sqlServerUsername = SqlServerUsername;
            Settings.Default.sqlServerPassword = EncryptionUtils.Protect(password);
            Settings.Default.Save();

            ////Data jobs
            //Properties.Settings.Default.updateJobEmailHost = UpdateJobEmailHost.Text;
            //if (int.TryParse(UpdateJobEmailPort.Text, out int port))
            //{
            //    Properties.Settings.Default.updateJobEmailPort = port;
            //}

            //Properties.Settings.Default.updateJobEmailUsername = UpdateJobEmailUsername.Text;
            //Properties.Settings.Default.updateJobEmailSender = UpdateJobEmailSender.Text;
            //Properties.Settings.Default.updateJobEmail = UpdateJobEmail.Text;
            //Properties.Settings.Default.updateJobEmailPassword = EncryptionUtils.Protect(UpdateJobEmailPassword.Password);

            //if (int.TryParse(UpdateJobTimeout.Text, out int timeout) && timeout > 0)
            //{
            //    Properties.Settings.Default.updateJobTimeout = timeout;
            //}

            //Properties.Settings.Default.updateJobReportOutliers = UpdateJobAbnormalities.IsChecked.Value;
            //Properties.Settings.Default.updateJobTimeouts = UpdateJobTimeouts.IsChecked.Value;
            //Properties.Settings.Default.updateJobReportErrors = UpdateJobDatasourceErrors.IsChecked.Value;
            //Properties.Settings.Default.updateJobReportNoData = UpdateJobNoData.IsChecked.Value;
        }
    }
}