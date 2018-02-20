using Common;
using ReactiveUI;
using ServerGui.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Windows.Forms;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.ExtensionMethods;
using Common.Requests;

namespace ServerGui.ViewModels
{
    public class HistoricalRequestViewModel : BaseViewModel
    {
        private readonly DataRequestClient.DataRequestClient client;
        private readonly SynchronizationContext uiContext;

        private ReactiveList<OHLCBar> data;

        private DateTime endDate;

        private bool isRthOnly;

        private bool isSaveToLocalStorage;

        private BarSize selectedBarsize = BarSize.OneDay;

        private DataLocation selectedDataLocation;

        private DateTime startDate;

        private Instrument theInstrument;

        public HistoricalRequestViewModel(Instrument instrument)
        {
            var r = new Random(); //we have to randomize the name of the client, can't reuse the identity
            uiContext = SynchronizationContext.Current;
            string datasourceName = instrument.Datasource.Name;
            //historicalBroker.DataSources.TryGetValue(datasourceName, out  historicalDataSource);

            Title = $"Historical Data Request for {instrument.Symbol} @ {datasourceName}";

            client = new DataRequestClient.DataRequestClient(
                $"DataRequestClient-{r.Next()}",
                "localhost",
                Settings.Default.RealTimeDataServerRequestPort,
                Settings.Default.RealTimeDataServerPublishPort,
                Settings.Default.HistoricalServerPort

               );

            client.HistoricalDataReceived += Client_HistoricalDataReceived;
            client.Error += Client_Error;
            client.Connect();

            Data = new ReactiveList<OHLCBar>();
            RequestDataTooltip = $"Button was disabled because no active connection to {datasourceName} was found.";
            TheInstrument = instrument;

            new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, 0);
            new DateTime(DateTime.Now.AddYears(-5).Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, 0);
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            //var isConnectedToDataSource = this.WhenAny(x => x.historicalDataSource.Connected, x => x.Value == true);
            //TODO problem with ThreadAccess on IB when property of connected changed
            //TODO use message pattern from mvvm light
            RequestDataCommand = ReactiveCommand.Create(RequestData);// isConnectedToDataSource);
            ExportCommand = ReactiveCommand.Create(ExportData);

           
        }

        public ReactiveCommand<Unit, Unit> ExportCommand { get; set; }

        public ReactiveCommand<Unit, Unit> RequestDataCommand { get; set; }

        public DateTime EndDate
        {
            get { return endDate; }
            set { this.RaiseAndSetIfChanged(ref endDate, value); }
        }

        public DateTime StartDate
        {
            get { return startDate; }
            set { this.RaiseAndSetIfChanged(ref startDate, value); }
        }

        public Instrument TheInstrument
        {
            get { return theInstrument; }
            set { this.RaiseAndSetIfChanged(ref theInstrument, value); }
        }

        public string RequestDataTooltip { get; }

        public ReactiveList<OHLCBar> Data
        {
            get { return data; }
            set { this.RaiseAndSetIfChanged(ref data, value); }
        }

        public string StatusLabelText { get; set; }

        public DataLocation SelectedDataLocation
        {
            get { return selectedDataLocation; }
            set { this.RaiseAndSetIfChanged(ref selectedDataLocation, value); }
        }

        public bool IsRthOnly
        {
            get { return isRthOnly; }
            set { this.RaiseAndSetIfChanged(ref isRthOnly, value); }
        }

        public bool IsSaveToLocalStorage
        {
            get { return isSaveToLocalStorage; }
            set { this.RaiseAndSetIfChanged(ref isSaveToLocalStorage, value); }
        }

        public BarSize SelectedBarsize
        {
            get { return selectedBarsize; }
            set { this.RaiseAndSetIfChanged(ref selectedBarsize, value); }
        }

        public string Title { get; set; }

        private void Client_Error(object sender, ErrorArgs e) => StatusLabelText = e.ErrorMessage;

        private void Client_HistoricalDataReceived(object sender, HistoricalDataEventArgs e)
        {
            StatusLabelText = $"Loaded {e.Data.Count} Bars";

            //find largest significant decimal by sampling the prices at the start and end of the series
            var decPlaces = new List<int>();
            for (var i = 0; i < Math.Min(20, e.Data.Count); i++)
            {
                decPlaces.Add(e.Data[i].Open.CountDecimalPlaces());
                decPlaces.Add(e.Data[e.Data.Count - 1 - i].Close.CountDecimalPlaces());
            }

            //set the column format to use that number so we don't get any useless trailing 0s
            var decimalPlaces = 5;
            if (decPlaces.Count > 0)
                decimalPlaces = decPlaces.Max();

            foreach (var bar in e.Data)
            {
                var newBar = DataUtils.BarWithRoundedPrices(bar, decimalPlaces);

                uiContext.Send(x => Data.Add(newBar), null);
            }
        }

        private new void Dispose()
        {
            client.HistoricalDataReceived -= Client_HistoricalDataReceived;
            client.Error -= Client_Error;
            client.Dispose();
            base.Dispose();
        }

        private void RequestData()
        {
            Data.Clear();

            var historicalDataRequest = new HistoricalDataRequest(
                TheInstrument,
                SelectedBarsize,
                StartDate,
                EndDate,
                SelectedDataLocation,
                IsSaveToLocalStorage,
                IsRthOnly);

            client.RequestHistoricalData(historicalDataRequest);
        }

        private void ExportData()
        {
            var dialog = new SaveFileDialog
            {
                Filter = @"CSV File (*.csv)|*.csv",
                FileName =
                    $"{TheInstrument.Symbol} {SelectedBarsize} {DateTime.Now:ddMMyyyy}-{DateTime.Now:ddMMyyyy}"
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var filePath = dialog.FileName;
                Data.ToCSVFile(filePath);
            }
        }
    }
}