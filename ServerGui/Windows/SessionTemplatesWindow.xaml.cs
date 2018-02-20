using MahApps.Metro.Controls;
using ReactiveUI;
using ServerGui.ViewModels;
using System.Windows;

namespace ServerGui.Windows
{
    /// <summary>
    ///     Interaction logic for SessionTemplatesWindow.xaml
    /// </summary>
    public partial class SessionTemplatesWindow : MetroWindow, IViewFor<SessionTemplatesViewModel>
    {
        public SessionTemplatesWindow()
        {
            InitializeComponent();
            ViewModel = new SessionTemplatesViewModel();

            DataContext = ViewModel;
            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (SessionTemplatesViewModel)value; }
        }

        public SessionTemplatesViewModel ViewModel { get; set; }
    }
}