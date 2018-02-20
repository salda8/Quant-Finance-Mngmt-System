using Common.EntityModels;
using MahApps.Metro.Controls;
using ReactiveUI;
using ServerGui.ViewModels;
using System.Windows;

namespace ServerGui.Windows
{
    /// <summary>
    /// Interaction logic for DataImportWindow.xaml
    /// </summary>
    public partial class DataImportWindow : MetroWindow, IViewFor<DataImportViewModel>
    {
        public DataImportWindow(Instrument instrument)
        {
            InitializeComponent();

            ViewModel = new DataImportViewModel(instrument);

            DataContext = ViewModel;

            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (DataImportViewModel)value; }
        }

        public DataImportViewModel ViewModel { get; set; }
    }
}