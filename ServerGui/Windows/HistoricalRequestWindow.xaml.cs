using Common.EntityModels;
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
    public partial class HistoricalRequestWindow : MetroWindow, IViewFor<HistoricalRequestViewModel>
    {
        public HistoricalRequestWindow(Instrument instrument)
        {
            InitializeComponent();
            ViewModel = new HistoricalRequestViewModel(instrument);
            DataContext = ViewModel;

            this.WhenAnyObservable(x => x.ViewModel.CloseCommand).Subscribe(x => Close());

            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (HistoricalRequestViewModel)value; }
        }

        public HistoricalRequestViewModel ViewModel { get; set; }
    }
}