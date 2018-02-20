using Common.EntityModels;
using MahApps.Metro.Controls;
using ReactiveUI;
using ServerGui.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace ServerGui.Windows
{
    /// <summary>
    ///     Interaction logic for EditSessionTemplateWindow.xaml
    /// </summary>
    public partial class EditSessionTemplateWindow : MetroWindow, IViewFor<EditSessionViewModel>
    {
        public bool TemplateAdded;

        public EditSessionTemplateWindow(SessionTemplate template)
        {
            InitializeComponent();

            ViewModel = new EditSessionViewModel(template);

            this.WhenAnyObservable(x => x.ViewModel.AddSessionCommand)
                .Subscribe(_ => { CollectionViewSource.GetDefaultView(SessionsGrid.ItemsSource).Refresh(); });
            this.WhenAnyObservable(x => x.ViewModel.RemoveSessionCommand)
                .Subscribe(_ => { CollectionViewSource.GetDefaultView(SessionsGrid.ItemsSource).Refresh(); });
            this.WhenAnyObservable(x => x.ViewModel.ModifyCommand)
                .Subscribe(_ =>
                {
                    CollectionViewSource.GetDefaultView(SessionsGrid.ItemsSource).Refresh();
                    new ManualResetEvent(false).WaitOne(500);
                    Close();
                });

            this.WhenAnyObservable(x => x.ViewModel.CloseCommand).Subscribe(x => Close());

            DataContext = ViewModel;

            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        public SessionTemplate TheTemplate { get; set; }

        public EditSessionViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (EditSessionViewModel)value; }
        }
    }
}