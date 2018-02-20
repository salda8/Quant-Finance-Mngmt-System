using System.Collections.ObjectModel;
using MahApps.Metro.Controls;
using ReactiveUI;
using Server.Utils;
using ServerGui.Properties;
using ServerGui.ViewModels;

namespace ServerGui.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow, IViewFor<SettingsViewModel>
    {
        public ObservableCollection<string> EconomicReleaseDataSources { get; set; } = new ObservableCollection<string>();
        public string SelectedDefaultEconomicReleaseDatasource { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
            SqlServerPassword.Password = EncryptionUtils.Unprotect(Settings.Default.sqlServerPassword);
            ViewModel = new SettingsViewModel();
            DataContext = ViewModel;

            
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (SettingsViewModel)value; }
        }

        public SettingsViewModel ViewModel { get; set; }
    }
}