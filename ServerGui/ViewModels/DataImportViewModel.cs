using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using Common;
using Common.EntityModels;
using Common.Enums;
using Common.ExtensionMethods;
using Common.Utils;
using LumenWorks.Framework.IO.Csv;
using ReactiveUI;

namespace ServerGui.ViewModels
{
    public class DataImportViewModel : BaseViewModel
    {
        private readonly Instrument _instrument;

        private ReactiveList<OHLCBar> bars = new ReactiveList<OHLCBar>();

        private SolidColorBrush dateFormatBorderBrush;

        private string dateFormatText;

        private SolidColorBrush dateTimeFormatBorderBrush;

        private string dateTimeFormatText;

        private string fileContents;

        private string filePath;

        private bool isFilePathTextBoxEnabled;

        private bool isOverwrite;
        private string priceMultiplier;

        private BarSize selectedBarSize = BarSize.OneDay;

        private string selectedDelimeter = ",";
        private Timezone selectedTimezone;

        private string startingLine = "1";

        private string timeFormat;

        private SolidColorBrush timeFormatBorderBrush;
        private string volumeMultiplier;
        private bool hasHeaders = true;

        public DataImportViewModel(Instrument instrument)
        {
            _instrument = instrument;

            Title += " - " + _instrument.Symbol;

            MinDT = new DateTime(1950, 1, 1);
            MaxDT = DateTime.Now;

            SelectFileCommand = ReactiveCommand.Create(SelectFile);
            ImportDataCommand = ReactiveCommand.Create(ImportBtn_Click);

            //ImportedData = new ReactiveList<>
        }

        public ReactiveCommand<Unit, Unit> ImportDataCommand { get; set; }

        public ReactiveCommand<Unit, Unit> SelectFileCommand { get; set; }

        public SolidColorBrush DateTimeFormatBorderBrush
        {
            get { return dateTimeFormatBorderBrush; }
            set { this.RaiseAndSetIfChanged(ref dateTimeFormatBorderBrush, value); }
        }

        public SolidColorBrush DateFormatBorderBrush
        {
            get { return dateFormatBorderBrush; }
            set { this.RaiseAndSetIfChanged(ref dateFormatBorderBrush, value); }
        }

        public SolidColorBrush TimeFormatBorderBrush
        {
            get { return timeFormatBorderBrush; }
            set { this.RaiseAndSetIfChanged(ref timeFormatBorderBrush, value); }
        }

        public string DateFormatText
        {
            get { return dateFormatText; }
            set { this.RaiseAndSetIfChanged(ref dateFormatText, value); }
        }

        public string TimeFormat
        {
            get { return timeFormat; }
            set { this.RaiseAndSetIfChanged(ref timeFormat, value); }
        }

        public string DateTimeFormatText
        {
            get { return dateTimeFormatText; }
            set { this.RaiseAndSetIfChanged(ref dateTimeFormatText, value); }
        }

        public ReactiveList<OHLCBar> Bars
        {
            get { return bars; }
            set { this.RaiseAndSetIfChanged(ref bars, value); }
        }

        public string StartingLine
        {
            get { return startingLine; }
            set { this.RaiseAndSetIfChanged(ref startingLine, value); }
        }

        public DataTable Data { get; set; }

        public string FileContents
        {
            get { return fileContents; }
            set { this.RaiseAndSetIfChanged(ref fileContents, value); }
        }

        public string FilePath
        {
            get { return filePath; }
            set { this.RaiseAndSetIfChanged(ref filePath, value); }
        }

        public BarSize SelectedBarSize
        {
            get { return selectedBarSize; }
            set { this.RaiseAndSetIfChanged(ref selectedBarSize, value); }
        }

        public bool IsFilePathTextBoxEnabled
        {
            get { return isFilePathTextBoxEnabled; }
            set { this.RaiseAndSetIfChanged(ref isFilePathTextBoxEnabled, value); }
        }

        public string VolumeMultiplier
        {
            get { return volumeMultiplier; }
            set { this.RaiseAndSetIfChanged(ref volumeMultiplier, value); }
        }

        public string PriceMultiplier
        {
            get { return priceMultiplier; }
            set { this.RaiseAndSetIfChanged(ref priceMultiplier, value); }
        }

        public Timezone SelectedTimezone
        {
            get { return selectedTimezone; }
            set { this.RaiseAndSetIfChanged(ref selectedTimezone, value); }
        }

        public DateTime MaxDT { get; set; }

        public DateTime MinDT { get; set; }

        public string Title { get; set; }

        public bool IsOverwrite
        {
            get { return isOverwrite; }
            set { this.RaiseAndSetIfChanged(ref isOverwrite, value); }
        }

        public string SelectedDelimeter
        {
            get { return selectedDelimeter; }
            set { this.RaiseAndSetIfChanged(ref selectedDelimeter, value); }
        }

        public bool HasHeaders
        {
            get { return hasHeaders; }
            set { hasHeaders = value; }
        }

        private void SelectFile()
        {
            var dlg = new OpenFileDialog { Filter = "CSV Files (*.csv;*.txt)|*.csv;*.txt|All files (*.*)|*.*" };

            DialogResult result = dlg.ShowDialog();

            if (result != DialogResult.OK) return;

            IsFilePathTextBoxEnabled = true;
            FilePath = dlg.FileName;

            //open the file
            try
            {
                //var builder = new StringBuilder();
                //using (var sr = new StreamReader(dlg.FileName))
                //{
                //    for (var i = 0; i < 1000; i++)
                //    {
                //        builder.AppendLine(sr.ReadLine());
                //        if (sr.EndOfStream) break;
                //    }
                //}
                //FileContents = builder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not read file: " + ex.Message);
                return;
            }

            NewParse();
            //parse the data
            //DoParse();
        }

        private void NewParse()
        {
            var builder = new StringBuilder();
            try
            {
                using (
                    var csv = new CachedCsvReader(new StreamReader(FilePath), HasHeaders,
                        Convert.ToChar(SelectedDelimeter)))
                {
                    builder.AppendLine(csv.GetCurrentRawData());

                    string[] fieldHeaders = csv.GetFieldHeaders();
                    while (csv.ReadNextRecord())
                    {
                        var bar = new OHLCBar() {};

                        for (int index = 0; index < csv.FieldCount; index++)
                        {
                            var columnName = fieldHeaders[0].ToLower();
                            
                            ParseHelper.ParseBarProperty(columnName, csv, index, bar, DateFormatText);

                        }
                        Bars.Add(bar);
                    }
                }
            }
            catch (InvalidDateTimeFormatException)
            {

            }
            catch (Exception ex)
            {
                Logger.Log(NLog.LogLevel.Error, ex);
            }

            FileContents = builder.ToString();


        }

        

        private void DoFormatColoring()
        {
            if (Data.Columns.Contains("Date"))
            {
                var sample = (string)Data.Rows[0]["DateTime"];
                var correctFormat = DateTime.TryParseExact(sample, DateFormatText,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime res);
                DateFormatBorderBrush = correctFormat
                                            ? Brushes.Green
                                            : Brushes.Red;
            }
            else
                DateFormatBorderBrush = Brushes.Gray;

            if (Data.Columns.Contains("Time"))
            {
                var sample = (string)Data.Rows[0]["DateTime"];
                var correctFormat = DateTime.TryParseExact(sample, TimeFormat,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime res);
                TimeFormatBorderBrush = correctFormat
                                            ? Brushes.Green
                                            : Brushes.Red;
            }
            else
                TimeFormatBorderBrush = Brushes.Gray;

            if (Data.Columns.Contains("DateTime"))
            {
                var sample = (string)Data.Rows[0]["DateTime"];
                var correctFormat = DateTime.TryParseExact(sample, DateTimeFormatText,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime res);
                DateTimeFormatBorderBrush = correctFormat
                                                ? Brushes.Green
                                                : Brushes.Red;
            }
            else
                DateTimeFormatBorderBrush = Brushes.Gray;
        }

        private void ImportBtn_Click()
        {
            var sw = new Stopwatch();
            sw.Start();
            //check that we've got the relevant data needed
            if (!Data.Columns.Contains("Date") && !Data.Columns.Contains("DateTime"))
            {
                MessageBox.Show("Must have a date column.");
                return;
            }

            if (SelectedBarSize < BarSize.OneDay && !Data.Columns.Contains("DateTime") &&
                !Data.Columns.Contains("Time"))
            {
                MessageBox.Show("Must have time column at this frequency");
                return;
            }

            if (!Data.Columns.Contains("Open") ||
                !Data.Columns.Contains("High") ||
                !Data.Columns.Contains("Low") ||
                !Data.Columns.Contains("Close"))
            {
                MessageBox.Show("Must have all OHLC columns.");
                return;
            }

            //make sure the timezone is set, and get it
            if (string.IsNullOrEmpty(_instrument.Exchange.Timezone))
            {
                MessageBox.Show("Instrument's exchange has no set timezone, can't import.");
                return;
            }

            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(_instrument.Exchange.Timezone);

            //get the multipliers
            var parseWorked = decimal.TryParse(PriceMultiplier, out decimal priceMultiplier);
            if (!parseWorked) priceMultiplier = 1;
            parseWorked = int.TryParse(VolumeMultiplier, out int volumeMultiplier);
            if (!parseWorked) volumeMultiplier = 1;

            //lines to skip
            parseWorked = int.TryParse(StartingLine, out int toSkip);
            if (!parseWorked) toSkip = 1;

            //get the frequency
            var frequency = SelectedBarSize;

            //separator
            var separator = SelectedDelimeter.ToCharArray();

            var bars = new List<OHLCBar>();

            var columns = new string[Data.Columns.Count];
            for (var i = 0; i < Data.Columns.Count; i++)
                columns[i] = Data.Columns[i].ColumnName;

            //determining time: if the freq is >= one day, then the time is simply the session end for this day
            var sessionEndTimes = new Dictionary<int, TimeSpan>();

            //1 day and up: we can load it all in one go with no trouble, also may require adjustment
            var periodicSaving = frequency < BarSize.OneDay;
            OHLCBar bar;
            var barsCount = 0;
            using (var sr = new StreamReader(FilePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    barsCount++;
                    if (barsCount < toSkip) continue;

                    try
                    {
                        bar = ParseLine(line.Split(separator), columns, priceMultiplier,
                                        volumeMultiplier);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Importing error: " + ex.Message);
                        return;
                    }

                    //only add the bar if it falls within the specified date range
                    if (bar.DateTimeClose >= MinDT && bar.DateTimeClose <= MaxDT)
                        bars.Add(bar);

                    //with 30 bars, we make a check to ensure that the user has entered the correct frequency
                    if (bars.Count == 30)
                    {
                        //the reason we have to use a bunch of bars and look for the most frequent timespan between them
                        //is that session breaks, daily breaks, weekends, etc. can have different timespans despite the
                        //correct frequency being chosen
                        var secDiffs = new List<int>();
                        for (var i = 1; i < bars.Count; i++)
                        {
                            secDiffs.Add(
                                (int)Math.Round((bars[i].DateTimeClose - bars[i - 1].DateTimeClose).TotalSeconds));
                        }

                        var mostFrequent = secDiffs.MostFrequent();
                        if ((int)Math.Round(frequency.ToTimeSpan().TotalSeconds) != mostFrequent)
                        {
                            MessageBox.Show("You appear to have selected the wrong frequency.");
                            return;
                        }
                    }

                    if (periodicSaving && bars.Count > 1000)
                    {
                        //convert to exchange timezone
                        ConvertTimeZone(bars, tzInfo);

                        //low frequencies, < 1 day. No adjustment required and inserting data at intervals instead of all at once
                        using (var storage = DataStorageFactory.Get())
                        {
                            try
                            {
                                storage.AddData(bars, _instrument, frequency, IsOverwrite, false);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error: " + ex.Message);
                            }
                        }
                        bars.Clear();
                    }
                }
            }

            if (bars.Count == 0) return;

            //convert to exchange timezone
            ConvertTimeZone(bars, tzInfo);

            //if only the date column is set, we need to get the session info and generate the closing time ourselves
            if (frequency >= BarSize.OneDay && !Data.Columns.Contains("Time") &&
                !Data.Columns.Contains("DateTime"))
            {
                //get the closing time for every day of the week
                var dotwValues = MyUtils.GetEnumValues<DayOfTheWeek>();

                foreach (var d in dotwValues)
                {
                    if (_instrument.Sessions.Any(x => x.ClosingDay == d && x.IsSessionEnd))
                    {
                        var endTime =
                            _instrument.Sessions.First(x => x.ClosingDay == d && x.IsSessionEnd)
                                       .ClosingTime;
                        sessionEndTimes.Add((int)d, endTime);
                    }
                    else
                        sessionEndTimes.Add((int)d, TimeSpan.FromSeconds(0));
                }

                foreach (var t in bars)
                {
                    var dayOfWeek = t.DateTimeClose.DayOfWeek.ToInt();
                    t.DateTimeClose = t.DateTimeClose.Date + sessionEndTimes[dayOfWeek];
                }
            }

           

            //sort by date
            if (frequency >= BarSize.OneDay)
                bars.Sort((x, y) => x.DateTimeClose.CompareTo(y.DateTimeClose));

            //try to import
            using (var storage = DataStorageFactory.Get())
            {
                try
                {
                    storage.AddData(bars,
                                    _instrument,
                                    frequency,
                                    IsOverwrite,
                                    frequency >= BarSize.OneDay);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            sw.Stop();
            MessageBox.Show($"Imported {barsCount} bars in {sw.ElapsedMilliseconds} ms.");
        }

        private OHLCBar ParseLine(string[] items, string[] columns, decimal priceMultiplier,
                                  int volumeMultiplier)
        {
            var bar = new OHLCBar();
            TimeSpan? closingTime = null;
            bool success;

            for (var i = 0; i < items.Length; i++)
            {
                switch (columns[i])
                {
                    case "Date":
                        DateTime tmpDate;
                        success = DateTime.TryParseExact(
                            items[i], DateFormatText, CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out tmpDate);
                        if (!success)
                            throw new Exception("Incorrect date format.");
                        bar.DateTimeClose = new DateTime(tmpDate.Ticks);
                        break;

                    case "DateTime":
                        DateTime tmpDT;
                        success = DateTime.TryParseExact(
                            items[i], DateTimeFormatText, CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out tmpDT);
                        if (!success)
                            throw new Exception("Incorrect datetime format.");
                        bar.DateTimeClose = new DateTime(tmpDT.Ticks);
                        break;

                    case "DateTimeOpen":
                        DateTime tmpDTOpen;
                        success = DateTime.TryParseExact(
                            items[i], DateTimeFormatText, CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out tmpDTOpen);
                        if (!success)
                            throw new Exception("Incorrect datetime format.");
                        bar.DateTimeOpen = new DateTime(tmpDTOpen.Ticks);
                        break;

                    case "Time":
                        DateTime tmpTS;
                        success = DateTime.TryParseExact(
                            items[i], TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out tmpTS);
                        if (!success)
                            throw new Exception("Incorrect time format.");
                        closingTime = TimeSpan.FromSeconds(tmpTS.TimeOfDay.TotalSeconds);
                        break;

                    case "Open":
                        bar.Open = priceMultiplier * decimal.Parse(items[i]);
                        break;

                    case "High":
                        bar.High = priceMultiplier * decimal.Parse(items[i]);
                        break;

                    case "Low":
                        bar.Low = priceMultiplier * decimal.Parse(items[i]);
                        break;

                    case "Close":
                        bar.Close = priceMultiplier * decimal.Parse(items[i]);
                        break;
                        
                    case "Volume":
                        bar.Volume = volumeMultiplier * long.Parse(items[i]);
                        break;

                    
                }
            }

            //do the time addition
            if (closingTime != null)
                bar.DateTimeClose += closingTime.Value;

            return bar;
        }

        private void ConvertTimeZone(List<OHLCBar> bars, TimeZoneInfo tzInfo)
        {
            //time zone conversion
            if (SelectedTimezone == Timezone.Utc &&
                (Data.Columns.Contains("Time") || Data.Columns.Contains("DateTime")))
            {
                foreach (var t in bars)
                {
                    t.DateTimeClose = TimeZoneInfo.ConvertTimeFromUtc(t.DateTimeClose, tzInfo);
                    if (t.DateTimeOpen.HasValue)
                        t.DateTimeOpen = TimeZoneInfo.ConvertTimeFromUtc(t.DateTimeOpen.Value, tzInfo);
                }
            }
            else if (SelectedTimezone == Timezone.Local &&
                     (Data.Columns.Contains("Time") || Data.Columns.Contains("DateTime")))
            {
                for (var i = 0; i < bars.Count; i++)
                {
                    bars[i].DateTimeClose = TimeZoneInfo.ConvertTime(bars[i].DateTimeClose, TimeZoneInfo.Local, tzInfo);
                    if (bars[i].DateTimeOpen.HasValue)
                    {
                        bars[i].DateTimeOpen = TimeZoneInfo.ConvertTime(bars[i].DateTimeOpen.Value,
                                                                  TimeZoneInfo.Local, tzInfo);
                    }
                }
            }
        }
    }

    internal class InvalidDateTimeFormatException : Exception
    {
        public InvalidDateTimeFormatException(string canTParseDateColumnIncoredFormat)
        {
        }
    }
}