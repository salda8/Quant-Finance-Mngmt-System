using Common.EntityModels;
using MahApps.Metro.Controls;
using ReactiveUI;
using ServerGui.ViewModels;
using System;
using System.Windows;
using System.Windows.Data;

namespace ServerGui.Windows
{
    /// <summary>
    ///     Interaction logic for EditExchangeWindow.xaml
    /// </summary>
    public partial class EditExchangeWindow : MetroWindow, IViewFor<EditExchangesViewModel>
    {
        public EditExchangeWindow(ExchangesViewModel exchange, Exchange selectedExchange = null, bool isModify = false)
        {
            InitializeComponent();
            if (!isModify)
                exchange.SelectedExchange = null;
            ViewModel = new EditExchangesViewModel(exchange, selectedExchange);
            
            this.WhenAnyObservable(x => x.ViewModel.SaveCommand).Subscribe(x => Close());

            this.WhenAnyObservable(x => x.ViewModel.CloseCommand).Subscribe(x => Close());

            DataContext = ViewModel;
            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (EditExchangesViewModel)value; }
        }

        public EditExchangesViewModel ViewModel { get; set; }
    }
}