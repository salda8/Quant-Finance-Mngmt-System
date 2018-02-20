using System;
using MahApps.Metro.Controls;
using ReactiveUI;
using ServerGui.ViewModels;

namespace ServerGui.Windows
{
    /// <summary>
    /// Interaction logic for DBConnectionWindow.xaml
    /// </summary>
    public partial class DBConnectionWindow : MetroWindow, IViewFor<DBConnectionViewModel>
    {
        public DBConnectionWindow()
        {
            InitializeComponent();

            ViewModel = new DBConnectionViewModel();
            DataContext = ViewModel;

            //ViewModel.SqlServerOkCommand.IsExecuting.Subscribe(x => Close());
            ViewModel.WhenAnyObservable(x => x.SqlServerOkCommand).Subscribe(x => Close());
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (DBConnectionViewModel)value; }
        }

        public DBConnectionViewModel ViewModel { get; set; }
    }
}