using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using ReactiveUI;
using ServerGui.ViewModels;

namespace ServerGui.Windows
{
    /// <summary>
    /// Interaction logic for AddInstrumentInteractiveBrokersWindow.xaml
    /// </summary>
    public partial class AddInstrumentInteractiveBrokersWindow : IViewFor<AddInstrumentIbViewModel>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AddInstrumentInteractiveBrokersWindow()
        {
            try
            {
                ViewModel = new AddInstrumentIbViewModel(DialogCoordinator.Instance);
            }
            catch (Exception ex)
            {
                Close();
                MessageBox.Show("Error",$"Error while initializing window. See logs for more details.");
                this.logger.Log(LogLevel.Error, ex.Message);
                return;
            }
            
            DataContext = ViewModel;

            InitializeComponent();

            ShowDialog();
            this.WhenAnyObservable(x => x.ViewModel.CloseCommand).Subscribe(x => Close());
            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        
        private void SymbolTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Search.Execute();
            }
        }

        public AddInstrumentIbViewModel ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (AddInstrumentIbViewModel) value; }
        }
    }
}