using Common.EntityModels;
using Common.Enums;
using Common.Utils;

using Krs.Ats.IBNet;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using Server.Repositories;
using ServerGui.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using LogLevel = NLog.LogLevel;

namespace ServerGui.ViewModels
{
    public class AddInstrumentIbViewModel : BaseViewModel
    {
        private readonly IBClient client;
        private readonly IDialogCoordinator dialogService;
        private readonly Dictionary<string, Exchange> exchanges;

        private readonly Datasource thisDs;

        private string currency = "USD";

        private DateTime? expirationDate;

        private bool includeExpired;

        private string multiSymbolText;

        private int nextRequestId;

        private bool searchUnderway;

        private string selectedExchange = "NYMEX";

        private InstrumentType selectedType = InstrumentType.Future;

        private string status;

        private double? strike;

        private string symbol;

        public AddInstrumentIbViewModel(IDialogCoordinator dialogService)
        {
            this.dialogService = dialogService;
            CreateCommands();

            var r = new Random();
            client = new IBClient();

            //random connection id for this one...
            client.Connect(Settings.Default.ibClientHost, Settings.Default.ibClientPort,
                           r.Next(1000, 200000));

            AddedInstruments = new HashSet<Instrument>();

            client.ContractDetails += _client_ContractDetails;
            client.ContractDetailsEnd += _client_ContractDetailsEnd;

            Observable
                .FromEventPattern<ConnectionClosedEventArgs>(client, "ConnectionClosed")
                .Subscribe(e => Logger.Warn("IB Instrument Adder connection closed."));

            Observable
                .FromEventPattern<NextValidIdEventArgs>(client, "NextValidId")
                .Subscribe(e => nextRequestId = e.EventArgs.OrderId);

            Observable
                .FromEventPattern<ErrorEventArgs>(client, "Error")
                .Subscribe(e =>
                {
                    if (e.EventArgs.ErrorMsg !=
                        "No security definition has been found for the request")
                        Logger.Error($"{e.EventArgs.ErrorCode} - {e.EventArgs.ErrorMsg}");

                    Status = e.EventArgs.ErrorMsg;
                    SearchUnderway = false;
                });

            Exchanges = new ObservableCollection<string> { "All" };
            exchanges = new Dictionary<string, Exchange>();

            thisDs = DbContext.Datasources.First(x => x.Name == "Interactive Brokers");

            foreach (Exchange e in DbContext.Exchanges)
            {
                Exchanges.Add(e.Name);
                exchanges.Add(e.Name, e);
            }

            Instruments = new ObservableCollection<Instrument>();
            InstrumentTypes = new ObservableCollection<InstrumentType>();

            //list the available types from our enum
            IEnumerable<InstrumentType> values = MyUtils.GetEnumValues<InstrumentType>();
            foreach (InstrumentType val in values)
            {
                InstrumentTypes.Add(val);
            }
        }

        public HashSet<Instrument> AddedInstruments { get; }

        public ReactiveCommand<IList, Unit> AddSelectedInstruments { get; private set; }

        public string Currency
        {
            get { return currency; }
            set { this.RaiseAndSetIfChanged(ref currency, value); }
        }

        public ObservableCollection<string> Exchanges { get; set; }

        public DateTime? ExpirationDate
        {
            get { return expirationDate; }
            set { this.RaiseAndSetIfChanged(ref expirationDate, value); }
        }

        public bool IncludeExpired
        {
            get { return includeExpired; }
            set { this.RaiseAndSetIfChanged(ref includeExpired, value); }
        }

        public ObservableCollection<Instrument> Instruments { get; set; }

        public ObservableCollection<InstrumentType> InstrumentTypes { get; set; }

        /// <summary>
        ///     Used to add multiple symbols in a batch.
        /// </summary>
        public string MultiSymbolText
        {
            get { return multiSymbolText; }
            set { this.RaiseAndSetIfChanged(ref multiSymbolText, value); }
        }

        public ReactiveCommand<Unit, Unit> Search { get; private set; }

        public bool SearchUnderway
        {
            get { return searchUnderway; }
            private set { this.RaiseAndSetIfChanged(ref searchUnderway, value); }
        }

        public string SelectedExchange
        {
            get { return selectedExchange; }
            set { this.RaiseAndSetIfChanged(ref selectedExchange, value); }
        }

        public InstrumentType SelectedType
        {
            get { return selectedType; }
            set { this.RaiseAndSetIfChanged(ref selectedType, value); }
        }

        public string Status
        {
            get { return status; }
            set { this.RaiseAndSetIfChanged(ref status, value); }
        }

        public double? Strike
        {
            get { return strike; }
            set { this.RaiseAndSetIfChanged(ref strike, value); }
        }

        public string Symbol
        {
            get { return symbol; }
            set { this.RaiseAndSetIfChanged(ref symbol, value); }
        }

        private void _client_ContractDetails(object sender, ContractDetailsEventArgs e)
        {
            Instrument instrument = ContractToInstrument(e);
            if (instrument == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() => Instruments.Add(instrument));
        }

        private void _client_ContractDetailsEnd(object sender, ContractDetailsEndEventArgs e)
        {
            SearchUnderway = false; //re-enables the search commands
            Status = Instruments.Count + " contracts arrived";
        }

        private Instrument ContractToInstrument(ContractDetailsEventArgs e)
        {
            Instrument instrument = TwsUtils.ContractDetailsToInstrument(e.ContractDetails);
            instrument.Datasource = thisDs;
            instrument.DatasourceID = thisDs.ID;
            if (e.ContractDetails.Summary.Exchange != null &&
                exchanges.ContainsKey(e.ContractDetails.Summary.Exchange))
            {
                instrument.Exchange = exchanges[e.ContractDetails.Summary.Exchange];
                instrument.ExchangeID = instrument.Exchange.ID;
            }
            else
            {
                Logger.Error("Could not find exchange in database: " +
                             e.ContractDetails.Summary.Exchange);
                return null;
            }

            return instrument;
        }

        private void CreateCommands()
        {
            AddSelectedInstruments = ReactiveCommand.Create<IList>(ExecuteAddSelectedInstruments);

            Search = ReactiveCommand.Create(ExecuteSearch,
                                            this.WhenAny(x => x.SearchUnderway, x => !x.Value)
                                                .ObserveOnDispatcher());
        }

        private void ExecuteAddSelectedInstruments(IList selectedInstruments)
        {
            if (selectedInstruments == null)
            {
                throw new ArgumentNullException(nameof(selectedInstruments));
            }
            if (selectedInstruments.Count == 0) {return;}

            var instrumentSource = new InstrumentRepository(DbContext);

            var count = selectedInstruments.Cast<Instrument>().Count(newInstrument => TryAddInstrument(instrumentSource, newInstrument));

            Status = $"{count}/{selectedInstruments.Count} instruments added.";
        }

        private void ExecuteSearch()
        {
            Instruments.Clear();
            SendContractDetailsRequest(Symbol);
        }

        private void SendContractDetailsRequest(string conctactSymbol)
        {
            var contract = new Contract
            {
                Symbol = conctactSymbol,
                SecurityType = TwsUtils.SecurityTypeConverter(SelectedType),
                Exchange = SelectedExchange == "All"
                                              ? ""
                                              : SelectedExchange,
                IncludeExpired = IncludeExpired,
                Currency = Currency
            };

            if (ExpirationDate.HasValue)
                contract.Expiry = ExpirationDate.Value.ToString("yyyyMM");

            if (Strike.HasValue)
                contract.Strike = Strike.Value;

            SearchUnderway = true; //disables the search commands
            client.RequestContractDetails(nextRequestId, contract);
        }

        private bool TryAddInstrument(InstrumentRepository instrumentSource,
                                      Instrument newInstrument, bool showDialogs = true)
        {
            try
            {
                if (instrumentSource.AddInstrument(newInstrument).Result.ID!=0)
                {
                    //if (AddedInstruments.Contains(newInstrument)) return false;

                    AddedInstruments.Add(newInstrument);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (showDialogs)
                    dialogService.ShowMessageAsync(this, "Error", ex.Message);

                Logger.Log(LogLevel.Warn, ex, "Error adding instrument");
            }
            return false;
        }
    }
}