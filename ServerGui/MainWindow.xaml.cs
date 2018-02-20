
using Common.Logging.NLog;
using Common;
using MahApps.Metro.Controls;
using NLog;
using NLog.Targets;
using ReactiveUI;
using ServerGui.UserControls;
using ServerGui.ViewModels;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ServerGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IViewFor<MainWindowViewModel>
    {
        public ObservableCollection<Instrument> Instruments { get; set; }

        public MainWindow()
        {
            Common.Logging.LogManager.Adapter = new NLogLoggerFactoryAdapter(new Common.Logging.Configuration.NameValueCollection());

            //make sure we can connect to the database
            DBUtils.CheckDBConnection();

            //set the log directory
            SetLogDirectory();

            //set the connection string
            DBUtils.SetConnectionString();

            Locator.CurrentMutable.Register(() => new MyDBContext(), typeof(IMyDbContext));

            InitializeComponent();
            ViewModel = new MainWindowViewModel((MenuItem)Resources["InstrumentSetSessionMenu"], (MenuItem)Resources["InstrumentTagMenu"]);
            DataContext = ViewModel;

            //TODO use message pattern from mvvm light for sending information through multiple layers
            this.WhenAnyValue(x => x.ViewModel.TagAddedRemovedOrChanged, x => x == true).Subscribe(_ => BuildTagContextMenu());
            this.WhenAnyValue(x => x.ViewModel.SessionAddedRemovedOrChanged, x => x == true).Subscribe(_ => BuildSetSessionTemplateMenu());
            this.WhenAnyValue(x => x.ViewModel.NewTagWasSet, x => x == true)
                .Subscribe(_ => RefreshGrid());

            string layoutFile = AppDomain.CurrentDomain.BaseDirectory + "GridLayout.xml";
            if (File.Exists(layoutFile))
            {
                try
                {
                    InstrumentsGrid.DeserializeLayout(File.ReadAllText(layoutFile));
                }
                catch
                {
                }
            }
            Task.Run(() => CheckLogs());

            //Log unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

            BuildTagContextMenu();
            BuildSetSessionTemplateMenu();

            //this.Events().MouseDoubleClick.Where(x=>x.OriginalSource.).InvokeCommand(ViewModel.EditOnDoubleClickCommand);
        }

        private void RefreshGrid()
        {
            IEnumerable instrumentsGridItemsSource = InstrumentsGrid.ItemsSource;
            if (instrumentsGridItemsSource != null)
            {
                CollectionViewSource.GetDefaultView(instrumentsGridItemsSource).Refresh();
                ViewModel.NewTagWasSet = false;
            }
        }

        private void CheckLogs()
        {
            var target = (MemoryTarget)LogManager.Configuration.AllTargets.Single(x => x.Name == "myTarget");
            int lastCount = 0;
            while (true)
            {
                while (target.Logs.Count() <= lastCount)
                {
                    Thread.Sleep(100);
                }
                var count = target.Logs.Count();
                var difference = count - lastCount;
                lastCount = count;

                IEnumerable<string> sources = target.Logs.Skip(Math.Max(0, target.Logs.Count() - difference));
                foreach (string source in sources)
                {
                    Dispatcher.BeginInvoke((Action)(() => ViewModel.LogMessages.Add(source)));
                }
            }
        }

        private void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Error((Exception)e.ExceptionObject, "Unhandled exception");
        }

        //creates a ViewModel.DbContext menu to set tags on instruments
        private void BuildTagContextMenu()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.TagMenuItem.Items.Clear();

                foreach (Tag t in ViewModel.DbContext.Tags.ToList())
                {
                    var button = new MenuItem
                    {
                        Header = t.Name,
                        Tag = t.ID,
                        IsCheckable = true,
                        Style = (Style)Resources["TagCheckStyle"],
                        Command = ViewModel.SetTagCommand
                    };

                    button.CommandParameter = button;
                    ViewModel.TagMenuItem.Items.Add(button);
                }
                ViewModel.TagMenuItem.Items.Add(Resources["NewTagMenuItem"]);
                ViewModel.TagMenuItem.SubmenuOpened += (o, e) => ViewModel.OnInstrumentTagSubmenuOpen();
            })
            ;
        }

        private void SetLogDirectory()
        {
            if (Directory.Exists(Properties.Settings.Default.logDirectory))
            {
                ((FileTarget)LogManager.Configuration.FindTargetByName("default")).FileName = Properties.Settings.Default.logDirectory + "ServerLog.log";
            }
        }

        private void BuildSetSessionTemplateMenu()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.SessionMenuItem.Items.Clear();

                foreach (var sessionTemplate in ViewModel.DbContext.SessionTemplates.ToList())
                {
                    var button = new MenuItem
                    {
                        Header = sessionTemplate.Name,
                        Tag = sessionTemplate.ID,
                        IsChecked = false,
                        Command = ViewModel.SetSessionCommand
                    };

                    button.CommandParameter = button;

                    ViewModel.SessionMenuItem.Items.Add(button);
                }

                //ViewModel.SessionMenuItem.Items.Add(Resources["NewSessionMenuItem"]);
                ViewModel.SessionMenuItem.SubmenuOpened += (o, e) => ViewModel.OnSessionSubmenuOpen();
            });
        }

        //the application is closing, shut down all the servers and stuff
        private void DXWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //save grid layout
            using (StreamWriter file = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "GridLayout.xml"))
            {
                InstrumentsGrid.SerializeLayout(file);
            }

            //then take down the client, the servers, and the brokers
            ViewModel.Client.Dispose();

            ViewModel.RealTimeServer.StopServer();
            ViewModel.RealTimeServer.Dispose();

            ViewModel.DataServer.StopServer();
            ViewModel.DataServer.Dispose();

            ViewModel.RealTimeBroker.Dispose();
            ViewModel.HistoricalBroker.Dispose();

            ViewModel.EquityUpdateServer.Dispose();
            ViewModel.MessagesServer.Dispose();
        }

        //exiting the application
        private void BtnExit_ItemClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        //add a new tag from the ViewModel.DbContext menu and then apply it to the selected instruments
        private void NewTagTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var newTagTextBox = (TextBox)sender;

            string newTagName = newTagTextBox.Text;
            if (ViewModel.DbContext.Tags.Any(x => x.Name == newTagName)) return; //tag already exists

            //add the tag
            var newTag = new Tag { Name = newTagName };
            ViewModel.DbContext.Tags.Add(newTag);

            //apply the tag to the selected instruments
            foreach (Instrument i in ViewModel.SelectedInstruments)
            {
                ViewModel.DbContext.Instruments.Attach(i);
                i.Tags.Add(newTag);
            }

            ViewModel.DbContext.SaveChanges();

            //update the tag menu
            var allTags = ViewModel.DbContext.Tags.ToList();
            BuildTagContextMenu();

            newTagTextBox.Text = "";

            CollectionViewSource.GetDefaultView(InstrumentsGrid.ItemsSource).Refresh();
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainWindowViewModel)value; }
        }

        public MainWindowViewModel ViewModel { get; set; }
    }
}