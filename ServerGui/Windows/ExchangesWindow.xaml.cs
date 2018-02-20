using MahApps.Metro.Controls;
using ReactiveUI;
using ServerGui.ViewModels;
using System.Windows;

namespace ServerGui.Windows
{
    /// <summary>
    ///     Interaction logic for ExchangesWindow.xaml
    /// </summary>
    public partial class ExchangesWindow : MetroWindow, IViewFor<ExchangesViewModel>
    {
        public ExchangesWindow()
        {
            InitializeComponent();
            ViewModel = new ExchangesViewModel();
            DataContext = ViewModel;
            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        public ExchangesViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get; set; }
    }
}