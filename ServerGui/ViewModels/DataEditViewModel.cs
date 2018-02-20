using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Forms;
using Common;
using Common.EntityModels;
using Common.Enums;
using Common.ExtensionMethods;
using Common.Utils;
using ReactiveUI;

namespace ServerGui.ViewModels
{
    public class DataEditViewModel : BaseViewModel
    {
        private readonly TimeZoneInfo timeZoneInfo;

        private ReactiveList<OHLCBar> data;

        private DateTime endDate;

        private DateTime endTime;

        private bool isRthOnly;

        private bool isSaveToLocalStorage;
        private BarSize loadedBarsize;
        private Timezone loadedTimeZone;

        private string numberOfDigits = "5";

        private IList<OHLCBar> selectedBars = new List<OHLCBar>();

        private BarSize selectedBarsize = BarSize.OneDay;


        private DataLocation selectedDataLocation;

        private DateTime startDate;

        private DateTime startTime;

        private Instrument theInstrument;

        public DataEditViewModel(Instrument instrument)
        {
          
                TheInstrument = instrument;
            

            var timezone = TheInstrument.Exchange == null ? "UTC" : TheInstrument.Exchange.Timezone;
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            
            //grab the data info
            using (var localStorage = DataStorageFactory.Get())
            {
                var storageInfo = localStorage.GetStoredDataInfo(TheInstrument.ID);

                if (storageInfo.Count == 0) //if it doesn't have any data, we just exit
                {
                    MessageBox.Show("This instrument has no data.");
                    CloseCommand.Execute();
                }
                else
                {
                    BarSizes = new ReactiveList<BarSize>();
                    foreach (var s in storageInfo)
                    {
                        BarSizes.Add(s.Frequency);
                    }
                }
            }

            Data = new ReactiveList<OHLCBar>();


            TheInstrument = instrument;

            StartTime = new DateTime(DateTime.Now.AddYears(-5).Year, 1, 1, 0, 0, 0, 0);
            StartDate = new DateTime(DateTime.Now.AddYears(-5).Year, 1, 1, 0, 0, 0, 0);

            EndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            
            LoadDataCommand = ReactiveCommand.Create(LoadData);
            ExportCommand = ReactiveCommand.Create(ExportData);
            
            
            DeleteCommand = ReactiveCommand.Create(Delete);
            SaveCommand = ReactiveCommand.Create(Save);

        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }

        public ReactiveList<BarSize> BarSizes { get; set; }

        public IList<OHLCBar> SelectedBars
        {
            get { return selectedBars; }
            set { this.RaiseAndSetIfChanged(ref selectedBars, value); }
        }

        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }
        
        public ReactiveCommand<Unit, Unit> ExportCommand { get; set; }

        public ReactiveCommand<Unit, Unit> LoadDataCommand { get; set; }

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

        public DateTime EndTime
        {
            get { return endTime; }
            set { this.RaiseAndSetIfChanged(ref endTime, value); }
        }

        public DateTime StartTime
        {
            get { return startTime; }
            set { this.RaiseAndSetIfChanged(ref startTime, value); }
        }

        public Instrument TheInstrument
        {
            get { return theInstrument; }
            set { this.RaiseAndSetIfChanged(ref theInstrument, value); }
        }

        public ReactiveList<OHLCBar> Data
        {
            get { return data; }
            set { this.RaiseAndSetIfChanged(ref data, value); }
        }

        public string StatusLabelText { get; set; }

        public Timezone SelectedTimezone { get; set; }

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

        public string NumberOfDigits
        {
            get { return numberOfDigits; }
            set { this.RaiseAndSetIfChanged(ref numberOfDigits, value); }
        }

        public string Title { get; set; }

        private void Save()
        {
            using (var localStorage = DataStorageFactory.Get())
            {
                try
                {
                    localStorage.UpdateData(Data.ToList(), TheInstrument, loadedBarsize, true);
                    MessageBox.Show("Successfully updated data.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while updating data: " + ex.Message);
                }
            }
        }


        private void Delete()
        {
            //get the selected bars
            var rows = SelectedBars;
            if (rows == null || rows.Count == 0) return;

            var result = MessageBox.Show($"Are you sure you want to delete {rows.Count} rows?",
                "Delete Rows", MessageBoxButtons.YesNo);
            if (result == DialogResult.No) return;

            var toDelete = rows.ToList();

            //data is stored in the db at exchange time. But we may have loaded it at UTC or local.
            //If so, we must convert it back.
            foreach (var b in toDelete)
                if (loadedTimeZone == Timezone.Utc)
                    b.DateTimeClose = TimeZoneInfo.ConvertTimeFromUtc(b.DateTimeClose, timeZoneInfo);
                else if (loadedTimeZone == Timezone.Local)
                    b.DateTimeClose = TimeZoneInfo.ConvertTime(b.DateTimeClose, TimeZoneInfo.Local, timeZoneInfo);


            using (var localStorage = DataStorageFactory.Get())
            {
                localStorage.DeleteData(TheInstrument, loadedBarsize, toDelete);
            }

            foreach (var bar in toDelete)
                Data.Remove(bar);
        }

        


        

        private void Closing()
        {
        }

        private void LoadData()
        {
            Data.Clear();

            //grab the data
            using (var localStorage = DataStorageFactory.Get())
            {
                if (BarSizes.Count == 1) SelectedBarsize = BarSizes.FirstOrDefault();
                var bars = localStorage.GetData(TheInstrument, StartTime, EndTime, SelectedBarsize);

                //find largest significant decimal by sampling the prices at the start and end of the series
                var decPlaces = new List<int>();
                for (var i = 0; i < Math.Min(bars.Count, 20); i++)
                {
                    decPlaces.Add(bars[i].Open.CountDecimalPlaces());
                    decPlaces.Add(bars[bars.Count - 1 - i].Close.CountDecimalPlaces());
                }


                foreach (var bar in bars)
                {
                    //do any required time zone coversions
                    if (SelectedTimezone == Timezone.Utc)
                        bar.DateTimeClose = TimeZoneInfo.ConvertTimeToUtc(bar.DateTimeClose, timeZoneInfo);
                    else if (SelectedTimezone == Timezone.Local)
                        bar.DateTimeClose = TimeZoneInfo.ConvertTime(bar.DateTimeClose, timeZoneInfo, TimeZoneInfo.Local);

                    var newBar = DataUtils.BarWithRoundedPrices(bar, decPlaces.Max());
                    Data.Add(newBar);
                }
                loadedBarsize = SelectedBarsize;
                loadedTimeZone = SelectedTimezone;
            }

            StatusLabelText = $"Loaded {Data.Count} Bars";
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