using Common;
using Common.Collections;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.ExtensionMethods;
using Common.Interfaces;
using Common.Requests;
using Common.Utils;
using DataAccess;
//using DataSource.InteractiveBrokers;
using MahApps.Metro.Controls.Dialogs;

using ReactiveUI;
using Server.DataBrokers;
using Server.Repositories;
using Server.Servers;
using ServerGui.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ServerGui.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private int progressBarMaximum;
        private int progressBarValue;
        private ReactiveList<Instrument> selectedInstruments = new ReactiveList<Instrument>();
        private bool tagAddedRemovedOrChanged;
        private bool windowOpened;
        private bool sessionAddedRemovedOrChanged;
        private int selectedInstrumentCount;
        private bool newTagWasSet;
        public ReactiveList<string> LogMessages { get; set; } = new ReactiveList<string>();
        //private Instrument selectedInstrument;
        
        public MainWindowViewModel(MenuItem sessionMenuItem, MenuItem tagMenuItem)
        {
            SessionMenuItem = sessionMenuItem;
            TagMenuItem = tagMenuItem;
            MappingConfiguration.Register();
            CreateBrokers();
            CreateAndStartServers();

            ActiveStreams = RealTimeBroker.ActiveStreams;
            
            
            DbContext.Database.Initialize(false);

         
            Seed.SeedDatasources(DbContext);

            if (!DbContext.Exchanges.Any())
            {
                Seed.DoSeed();
            }
            //create data db if it doesn't exist
            var dataContext = new DataDBContext();
            dataContext.Database.Initialize(false);
            dataContext.Dispose();
       
            

            //we also need a client to make historical data requests with
            CreateAndStartClient();

            GetInstruments();
            CreateCommands();
        }

        #region ConstructorMethods

        private void CreateAndStartServers()
        {
            RealTimeServer = new RealTimeDataServer(Settings.Default.RealTimeDataServerRequestPort, RealTimeBroker);
            DataServer = new HistoricalDataServer(Settings.Default.HistoricalServerPort, HistoricalBroker);
            MessagesServer = new MessagesServer(Settings.Default.MessagesServerPushPort);
            EquityUpdateServer = new EquityUpdateServer(Settings.Default.EquityUpdateServerRouterPort);
            InstrumentRequestServer = new RequestResponseServer(Settings.Default.InstrumetnUpdateRequestSocketPort);

            RealTimeServer.StartServer();
            DataServer.StartServer();
            MessagesServer.StartServer();
            InstrumentRequestServer.StartServer();
            //not using poller.Async, need to spawn thread
            Task.Factory.StartNew(EquityUpdateServer.StartServer, TaskCreationOptions.LongRunning);
        }

        public RequestResponseServer InstrumentRequestServer { get; set; }

        private void CreateBrokers()
        {
            var localStorage = DataStorageFactory.Get();
            RealTimeBroker = new RealTimeDataBroker(localStorage,
                new IRealTimeDataSource[]
                {
                    new IB(Settings.Default.ibClientHost,
                        Settings.Default.ibClientPort,
                        Settings.Default.rtdClientIBID)
                });
            HistoricalBroker = new HistoricalDataBroker(localStorage,
                new IHistoricalDataSource[]
                {
                    new IB(Settings.Default.ibClientHost,
                        Settings.Default.ibClientPort,
                        Settings.Default.histClientIBID),
                });
          
        }

        private void CreateAndStartClient()
        {
            Client = new DataRequestClient.DataRequestClient(
                "SERVERCLIENT",
                "localhost",
                Settings.Default.RealTimeDataServerRequestPort,
                Settings.Default.RealTimeDataServerPublishPort,
                Settings.Default.HistoricalServerPort
            );
            Client.Connect();
            Client.HistoricalDataReceived += Client_HistoricalDataReceived;
        }

        

        private void CreateCommands()
        {
            SelectedInstrumentChanged = ReactiveCommand.Create<IList>(SelectedInstrumentChangeHandler);
            //this.WhenAnyValue(x=>x.SelectedInstrument).Subscribe()
            var isJustOneSelectedInstrument = this.WhenAny(x => x.SelectedInstrumentCount, x => x.Value == 1);
            SetTagCommand = ReactiveCommand.Create<MenuItem>(SetTag);
            SetSessionCommand = ReactiveCommand.Create<MenuItem>(SetSession, isJustOneSelectedInstrument);
            ExchangesCommand = ReactiveCommand.Create(OpenExchangesWindow);
            SessionTemplatesCommand = ReactiveCommand.Create(OpenSessionTemplatesWindow);
           
            TagsCommand = ReactiveCommand.Create(OpenTagsWindow);
            SettingsCommand = ReactiveCommand.Create(OpenSettingsWindow);

            //IsConnectedToIB= this.WhenAny(x=>x.HistoricalBroker.DataSources["Interactive Brokers"].Connected, x=>x.Value==true);
            //TODO problem with ThreadAccess on IB when property of connected changed
            AddInstrumentIbCommand = ReactiveCommand.Create(AddInstrumentIb);//,IsConnectedToIB);
            UpdateHistoricalDataCommand = ReactiveCommand.Create<BarSize>(UpdateHistoricalData);
            CheckForUpdatesCommand = ReactiveCommand.Create(CheckForUpdates);
            NewDataRequestCommand = ReactiveCommand.Create(NewDataRequest, isJustOneSelectedInstrument);
            var isAtLeastOneSelectedInstrument = this.WhenAny(x => x.SelectedInstrumentCount, x => x.Value > 0);
            ClearDataCommand = ReactiveCommand.Create(ClearData, isAtLeastOneSelectedInstrument);
            DeleteInstrumentCommand = ReactiveCommand.Create(DeleteInstrument, isAtLeastOneSelectedInstrument);

            EditDataCommand = ReactiveCommand.Create(EditData, isJustOneSelectedInstrument);
            ImportDataCommand = ReactiveCommand.Create(ImportData, isJustOneSelectedInstrument);

            RequestNewDataStreamCommand = ReactiveCommand.Create(RequestNewDataStream, isJustOneSelectedInstrument);
        }

        public IObservable<bool> IsConnectedToIB { get; set; }

        private void NewDataRequest()
        {
            var window = new Windows.HistoricalRequestWindow(SelectedInstruments.First());
            window.Show();
        }

        public ReactiveCommand<Unit, Unit> NewDataRequestCommand { get; set; }

        private void RequestNewDataStream()
        {
            Client.RequestRealTimeData(new RealTimeDataRequest { Instrument = selectedInstruments.First(), Frequency = BarSize.OneMinute, SaveToLocalStorage = true });
        }

        public int SelectedInstrumentCount
        {
            get { return selectedInstrumentCount; }
            set { this.RaiseAndSetIfChanged(ref selectedInstrumentCount, value); }
        }

        private void SelectedInstrumentChangeHandler(IList selectedItems)
        {
            SelectedInstruments.Clear();
            SelectedInstruments.AddRange(selectedItems.Cast<Instrument>().ToList());
            SelectedInstrumentCount = SelectedInstruments.Count;
        }

        private void DeleteInstrument()
        {
            if (selectedInstruments.Count == 0) return;

            if (selectedInstruments.Count == 1)
            {
                var inst = selectedInstruments[0];
                MessageBoxResult res = MessageBox.Show(
                    $"Are you sure you want to delete {inst.Symbol} @ {inst.Datasource.Name}?",
                    "Delete", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No) return;
            }
            else
            {
                MessageBoxResult res = MessageBox.Show(
                    $"Are you sure you want to delete {selectedInstruments.Count} instruments?",
                    "Delete", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No) return;
            }

            List<Instrument> instruments = selectedInstruments.ToList();
            try
            {
                DbContext.Instruments.RemoveRange(instruments);
                Instruments.RemoveAll(instruments);
                DbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                Logger.Error(() => $"Exception: {exception.InnerException?.InnerException?.Message}");
                MessageBox.Show($"Deleting Instrument was unsuccessful. Please see logs for more info.",
                    "Error", MessageBoxButton.OK);
                Instruments.AddRange(instruments);
            }
        }

        private void ClearData()
        {
            if (selectedInstruments.Count == 0) return;

            if (selectedInstruments.Count == 1)
            {
                var inst = selectedInstruments[0];
                MessageBoxResult res = MessageBox.Show(
                    $"Are you sure you want to delete all data from {inst.Symbol} @ {inst.Datasource.Name}?",
                    "Delete", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No) return;
            }
            else
            {
                MessageBoxResult res = MessageBox.Show(
                    $"Are you sure you want to delete all data from {selectedInstruments.Count} instruments?",
                    "Delete", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No) return;
            }

            using (var storage = DataStorageFactory.Get())
            {
                foreach (Instrument i in selectedInstruments)
                {
                    try
                    {
                        storage.DeleteAllInstrumentData(i);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

            StatusBarLabel = "Instrument data deleted";
        }

        private void EditData()
        {
            var window = new Windows.DataEditWindow(selectedInstruments.FirstOrDefault());
            window.ShowDialog();
        }

        private void ImportData()
        {
            var selectedInstrument = selectedInstruments[0];
            var window = new Windows.DataImportWindow(selectedInstrument);
            window.ShowDialog();
        }

        private void CheckForUpdates()
        {
            UpdateHelper.InstallUpdateSyncWithInfo();
        }

        private void AddInstrumentIb()
        {
            var window = new Windows.AddInstrumentInteractiveBrokersWindow();

            if (window.ViewModel?.AddedInstruments != null)
            {
                foreach (Instrument i in window.ViewModel.AddedInstruments)
                {
                    Instruments.Add(i);
                }

                window.Close();
            }
        }

        private void GetInstruments()
        {
            Instruments = new ReactiveList<Instrument>();

            var instrumentRepo = new InstrumentRepository(DbContext);
            var instrumentList = instrumentRepo.FindInstruments().Result;

            foreach (Instrument i in instrumentList)
            {
                Instruments.Add(i);
            }
        }

        #endregion ConstructorMethods

        #region Commands

        public ReactiveCommand<Unit, Unit> DeleteInstrumentCommand { get; set; }

        public ReactiveCommand<Unit, Unit> CloneInstrumentCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CheckForUpdatesCommand { get; set; }

        public ReactiveCommand<Unit, Unit> AboutCommand { get; set; }

        public ReactiveCommand<Unit, Unit> RequestNewDataStreamCommand { get; set; }

        public ReactiveCommand<Unit, Unit> ClearDataCommand { get; set; }

        public ReactiveCommand<Unit, Unit> EditDataCommand { get; set; }

        public ReactiveCommand<Unit, Unit> ImportDataCommand { get; set; }

        public ReactiveList<Instrument> SelectedInstruments

        {
            get { return selectedInstruments; }
            set
            {
                //selectedInstruments = selectedInstruments ?? new List<Instrument>();

                this.RaiseAndSetIfChanged(ref selectedInstruments, value);
            }
        }

        public ReactiveCommand<Unit, Unit> SettingsCommand { get; set; }

        public ReactiveCommand<Unit, Unit> TagsCommand { get; set; }

        public ReactiveCommand<Unit, Unit> ScheduledJobCommand { get; set; }

       

        public ReactiveCommand<Unit, Unit> SessionTemplatesCommand { get; set; }

        public ReactiveCommand<MenuItem, Unit> SetSessionCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ExchangesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> AddInstrumentIbCommand { get; set; }
        public ReactiveCommand<BarSize, Unit> UpdateHistoricalDataCommand { get; set; }
        public ReactiveList<Instrument> Instruments { get; set; }
        public ReactiveCommand<MenuItem, Unit> SetTagCommand { get; set; }

        public ReactiveCommand<Unit, Unit> EditOnDoubleClickCommand { get; set; }
        public ReactiveCommand<IList, Unit> SelectedInstrumentChanged { get; set; }

        #endregion Commands

        #region Properties

        public DataRequestClient.DataRequestClient Client { get; private set; }

        public List<Tag> AllTags
        {
            get
            {
                using (var ctx = new MyDBContext())
                {
                    return ctx.Tags.ToList();
                }
            }
        }

        public MenuItem SessionMenuItem { get; set; }
        public RealTimeDataServer RealTimeServer { get; set; }
        public HistoricalDataServer DataServer { get; set; }

        public int ProgressBarMaximum
        {
            get { return progressBarMaximum; }
            set { this.RaiseAndSetIfChanged(ref progressBarMaximum, value); }
        }

        public int ProgressBarValue
        {
            get { return progressBarValue; }
            set { this.RaiseAndSetIfChanged(ref progressBarValue, value); }
        }

        public string StatusBarLabel { get; set; }

        //public EconomicReleaseBroker EconomicReleaseBroker { get; set; }
        public MessagesServer MessagesServer { get; set; }

        public EquityUpdateServer EquityUpdateServer { get; set; }

        public ConcurrentNotifierBlockingList<RealTimeStreamInfo> ActiveStreams { get; set; }

        public string AddIBInstrumentTooltip { get; } =
            "Button was disabled because no active connection to Interactive Brokers was found.";

        public RealTimeDataBroker RealTimeBroker { get; set; }

        public HistoricalDataBroker HistoricalBroker { get; set; }
        public MenuItem TagMenuItem { get; set; }

        #endregion Properties

        private void UpdateHistoricalData(BarSize barSize)
        {
            var frequency = barSize; //(BarSize) ((MenuItem) sender).Tag;

            int requestCount = 0;

            using (var localStorage = DataStorageFactory.Get())
            {
                foreach (var instrument in selectedInstruments)
                {
                    var storageInfo = localStorage.GetStoredDataInfo(instrument.ID);
                    if (storageInfo.Any(x => x.Frequency == frequency))
                    {
                        var relevantStorageInfo = storageInfo.First(x => x.Frequency == frequency);
                        Client.RequestHistoricalData(new HistoricalDataRequest(
                            instrument,
                            frequency,
                            relevantStorageInfo.LatestDate +
                            frequency.ToTimeSpan(),
                            DateTime.Now,
                            DataLocation.ExternalOnly,
                            true));
                        requestCount++;
                    }
                }
            }

            if (ProgressBarValue >= ProgressBarMaximum)
            {
                ProgressBarMaximum = requestCount;
                ProgressBarValue = 0;
            }
            else
            {
                ProgressBarMaximum += requestCount;
            }
        }

        //tag menu is opening, populate it with all tags and set the appropriate checkbox values
        public void OnInstrumentTagSubmenuOpen()
        {
            if (selectedInstruments.Count == 1)
            {
                var instrument = selectedInstruments.First();
                //set checkboxes on the selected tags

                foreach (MenuItem btn in TagMenuItem.Items)
                {
                    if (btn.Tag == null || instrument.Tags == null) continue;

                    btn.IsChecked = instrument.Tags.Any(x => x.ID == (int)btn.Tag);
                    btn.IsEnabled = true;
                }
            }
            else
            {
                foreach (MenuItem btn in TagMenuItem.Items)
                {
                    if (btn.Tag == null) continue;

                    int tagCount =
                        selectedInstruments.Count(x => x.Tags != null && x.Tags.Any(y => y.ID == (int)btn.Tag));
                    if (tagCount == 0 || tagCount == selectedInstruments.Count)
                    {
                        btn.IsEnabled = true;
                        btn.IsChecked = tagCount == selectedInstruments.Count;
                    }
                    else //if tags have different values among the selected instruments, just disable the button
                    {
                        btn.IsEnabled = false;
                    }
                }
            }
        }

        public void OnSessionSubmenuOpen()
        {
            if (selectedInstruments.Count == 1)
            {
                var instrument = selectedInstruments.First();
                //set checkboxes on the selected tags

                foreach (MenuItem btn in SessionMenuItem.Items)
                {
                    if (btn.Tag == null || instrument.Sessions == null) continue;

                    btn.IsChecked = instrument.SessionTemplateID==(int)btn.Tag;
                    btn.IsEnabled = true;
                }
            }
            
        }

        private void SetTag(MenuItem item)
        {
            foreach (Instrument instrument in selectedInstruments)
            {
                DbContext.Instruments.Attach(instrument);

                if (item.IsChecked)
                {
                    var tag = DbContext.Tags.First(x => x.ID == (int)item.Tag);
                    DbContext.Tags.Attach(tag);
                    instrument.Tags.Add(tag);
                   // Instruments.First(x => x.ID == instrument.ID).Tags.Add(tag);
                }
                else
                {
                    item.IsChecked = false;
                    var tmpTag = instrument.Tags.First(x => x.ID == (int)item.Tag);
                    DbContext.Tags.Attach(tmpTag);
                    instrument.Tags.Remove(tmpTag);
                }
            }

            NewTagWasSet = true;
            DbContext.SaveChanges();
        }

        public bool NewTagWasSet
        {
            get { return newTagWasSet; }
            set
            {
                newTagWasSet = value;
                this.RaisePropertyChanged();
            }
        }

        private void SetSession(MenuItem item)
        {
            var templateSessions =
                DbContext.TemplateSessions.Where(x => x.TemplateID == (int)item.Tag).ToList();
            var selectedInstrument = selectedInstruments.First();
            //DbContext.Instruments.Attach(selectedInstrument);
            selectedInstrument.SessionsSource = SessionsSource.Template;
            selectedInstrument.SessionTemplateID = (int)item.Tag;

            if (selectedInstrument.Sessions == null)
                selectedInstrument.Sessions = new List<InstrumentSession>();

            //Remove any old sessions
            var tmpSessions = new List<InstrumentSession>(selectedInstrument.Sessions);
            foreach (var isession in tmpSessions)
            {
                DbContext.InstrumentSessions.Attach(isession);
                DbContext.InstrumentSessions.Remove(isession);
            }

            selectedInstrument.Sessions.Clear();

            //Add the new sessions
            foreach (var ts in templateSessions)
                selectedInstrument.Sessions.Add(ts.ToInstrumentSession());

            DbContext.SaveChanges();
        }

        private void Client_HistoricalDataReceived(object sender, HistoricalDataEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressBarValue++;
                    if (ProgressBarValue >= ProgressBarMaximum)
                    {
                        ProgressBarValue = 0;
                        ProgressBarMaximum = 0;
                        StatusBarLabel = "Historical data update complete";
                    }
                    else
                    {
                        StatusBarLabel =
                            $"Rcvd {e.Data.Count} bars of {e.Request.Instrument.Symbol} @ {e.Request.Frequency}";
                    }
                }
            );
        }

        #region OpeningNewMethods

        private static void OpenSettingsWindow()
        {
            var settingsWindow = new Windows.SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void OpenTagsWindow()
        {
            TagAddedRemovedOrChanged = false;

            var window = new Windows.TagsWindow();
            window.Show();
            windowOpened = true;
            window.Events().Closing.Subscribe(x => this.windowOpened = false);
            Task.Run(() =>
            {
                var addedRemovedOrChanged = false;

                while (windowOpened)
                {
                    if (window.ViewModel.TagAddedRemovedOrChanged)
                    {
                        addedRemovedOrChanged = true;
                    }

                    new ManualResetEvent(false).WaitOne(500);
                }

                TagAddedRemovedOrChanged = addedRemovedOrChanged;//mainwindow is subscribed to this value and will rebuild the tag context menu
            });
        }

        public bool TagAddedRemovedOrChanged
        {
            get { return tagAddedRemovedOrChanged; }
            set { this.RaiseAndSetIfChanged(ref tagAddedRemovedOrChanged, value); }
        }

        
       
        private void OpenSessionTemplatesWindow()
        {
            SessionAddedRemovedOrChanged = false;
            var window = new Windows.SessionTemplatesWindow();
            window.ShowDialog();
            window.WhenAny(x => x.ViewModel.SessionAddedRemovedOrChanged, x => x).Subscribe(_ => SessionAddedRemovedOrChanged = true);
        }

        public bool SessionAddedRemovedOrChanged
        {
            get { return sessionAddedRemovedOrChanged; }
            set
            {
                this.RaiseAndSetIfChanged(ref sessionAddedRemovedOrChanged, value);
            }
        }

        private static void OpenExchangesWindow()
        {
            var window = new Windows.ExchangesWindow();
            window.ShowDialog();
        }

        #endregion OpeningNewMethods
    }
}