
using MahApps.Metro.Controls;
using ReactiveUI;
using ServerGui.ViewModels;
using System;
using System.Windows;

namespace ServerGui.Windows
{
    /// <summary>
    ///     Interaction logic for DataEditWindow.xaml
    /// </summary>
    public partial class DataEditWindow : MetroWindow, IViewFor<DataEditViewModel>
    {
        public DataEditWindow(Instrument instrument)
        {
            InitializeComponent();
            ViewModel = new DataEditViewModel(instrument);
            DataContext = ViewModel;

            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
            this.WhenAnyObservable(x => x.ViewModel.CloseCommand).Subscribe(x => Close());
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (DataEditViewModel)value; }
        }

        public DataEditViewModel ViewModel { get; set; }
    }
}