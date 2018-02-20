using Common;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Interfaces;

using Common.Requests;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;

namespace DataStorage.SqlServer
{
    public class SqlServerStorage : IDataStorage
    {
        private bool connected;

        /// <summary>
        ///     Periodically updates the Connected property.
        /// </summary>
        private Timer connectionStatusUpdateTimer;

        private readonly string connectionString;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SqlServerStorage(string connectionString)
        {
            Name = "Local Storage";
            this.connectionString = connectionString;

            connectionStatusUpdateTimer = new Timer(1000);
            connectionStatusUpdateTimer.Elapsed += _connectionStatusUpdateTimer_Elapsed;
            connectionStatusUpdateTimer.Start();
        }

        #region IDataStorage Members

        /// <summary>
        ///     Connect to the data source.
        /// </summary>
        public void Connect()
        {
        }

        /// <summary>
        ///     Disconnect from the data source.
        /// </summary>
        public void Disconnect()
        {
        }

        /// <summary>
        ///     Whether the connection to the data source is up or not.
        /// </summary>
        public bool Connected
        {
            get { return connected; }

            set
            {
                connected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     The name of the data source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Returns data from local storage.
        /// </summary>
        /// <param name="instrument">The instrument whose data you want.</param>
        /// <param name="startDate">Starting datetime.</param>
        /// <param name="endDate">Ending datetime.</param>
        /// <param name="frequency">Frequency.</param>
        /// <returns></returns>
        public List<OHLCBar> GetData(Instrument instrument, DateTime startDate, DateTime endDate,
            BarSize frequency = BarSize.OneDay)
        {
            if (!TryConnect(out SqlConnection connection))
                throw new Exception("Could not connect to database");
            using (connection)
            {
                using (var cmd = new SqlCommand("", connection))
                {
                    cmd.CommandText = "SELECT * FROM data WHERE " +
                                      "InstrumentID = @ID AND Frequency = @Freq AND DateTimeOpen >= @Start AND DateTimeClose <= @End ORDER BY DateTimeClose ASC";
                    cmd.Parameters.AddWithValue("ID", instrument.ID);
                    cmd.Parameters.AddWithValue("Freq", (int)frequency);
                    cmd.Parameters.AddWithValue("Start", frequency >= BarSize.OneDay ? startDate.Date : startDate);
                    cmd.Parameters.AddWithValue("End", frequency >= BarSize.OneDay ? endDate.Date : endDate);

                    var data = new List<OHLCBar>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var bar = new OHLCBar
                            {
                                DateTimeClose = reader.GetDateTime(2),
                                Open = reader.GetDecimal(3),
                                High = reader.GetDecimal(4),
                                Low = reader.GetDecimal(5),
                                Close = reader.GetDecimal(6),
                                DateTimeOpen = reader.GetDateTime(7),
                                Volume = reader.GetInt64(8)
                            };

                            data.Add(bar);
                        }
                    }
                    return data;
                }
            }
        }

        public void RequestHistoricalData(HistoricalDataRequest request)
        {
            var data = GetData(request.Instrument, request.StartingDate, request.EndingDate, request.Frequency);

            RaiseEvent(HistoricalDataArrived, this, new HistoricalDataEventArgs(request, data));
        }

        public void AddData(List<OHLCBar> data, Instrument instrument, BarSize frequency, bool overwrite = false,
            bool adjust = true)
        {
            if (ParameterChecks(data)) return;

            if (!TryConnect(out SqlConnection connection))
                throw new Exception("Could not connect to database");

            using (connection)
            {
                using (var cmd = new SqlCommand("", connection))
                {
                    cmd.CommandTimeout = 0;

                    var sb = new StringBuilder();
                    sb.Append("BEGIN TRAN T1;");

                    //We create a temporary table which will then be used to merge the data into the data table
                    var r = new Random();
                    var tableName = "tmpdata" + r.Next();
                    sb.AppendFormat("SELECT * INTO {0} from data where 1=2;", tableName);

                    //start the insert
                    for (var i = 0; i < data.Count; i++)
                    {
                        var bar = data[i];
                        if (frequency >= BarSize.OneDay)
                        {
                            //we don't save the time when saving this stuff to allow flexibility with changing sessions
                            bar.DateTimeClose = bar.DateTimeClose.Date;
                            bar.DateTimeOpen = null;
                        }

                        if (i == 0 || (i - 1) % 500 == 0)
                        {
                            sb.AppendFormat("INSERT INTO {0} " +
                                            "(DateTimeClose, InstrumentID, Frequency, [Open], High, Low, [Close], " +
                                            "Volume, DateTimeOpen) VALUES ", tableName);
                        }

                        AppendBar(instrument, frequency, sb, bar);

                        sb.Append(i % 500 != 0 && i < data.Count - 1 ? ", " : ";");
                    }

                    //Merge the temporary table with the data table
                    sb.AppendFormat(@"MERGE INTO
                                    dbo.data T
                                USING
(SELECT * FROM {0}) AS S (InstrumentID, Frequency,DateTimeClose, [Open], High, Low, [Close],
                                        DateTimeOpen,Volume )
                                ON
                                    T.InstrumentID = S.InstrumentID AND T.Frequency = S.Frequency AND T.DateTimeClose = S.DateTimeClose
                                WHEN NOT MATCHED THEN
                                    INSERT (InstrumentID, Frequency,DateTimeClose, [Open], High, Low, [Close],
                                        DateTimeOpen,Volume)
                                    VALUES (InstrumentID, Frequency,DateTimeClose, [Open], High, Low, [Close],
                                        DateTimeOpen,Volume)",
                        tableName);

                    if (overwrite)
                    {
                        sb.Append(@" WHEN MATCHED THEN
                                    UPDATE SET

                                        T.InstrumentID = S.InstrumentID,
                                        T.Frequency = S.Frequency,
                                        T.DateTimeClose = S.DateTimeClose,
                                        T.[Open] = S.[Open],
                                        T.High = S.High,
                                        T.Low = S.Low,
                                        T.[Close] = S.[Close],
                                        T.Volume = S.Volume,
                                        T.DateTimeOpen = S.DateTimeOpen;");
                    }
                    else
                        sb.Append(";");

                    sb.AppendFormat("DROP TABLE {0};", tableName);
                    sb.Append("COMMIT TRAN T1;");

                    cmd.CommandText = sb.ToString();

                    ExecuteQuery(cmd);

                    
                        cmd.CommandText = $@"
                                                MERGE INTO
                                                    instrumentinfo T
                                                USING
                                                    (SELECT * FROM (VALUES ({instrument.ID}, {(int)frequency}, '{data
                            [0].DateTimeClose:yyyy-MM-dd HH:mm:ss.fff}', '{data[data.Count - 1].DateTimeClose:yyyy-MM-dd HH:mm:ss.fff}'))
														Dummy(InstrumentID, Frequency, EarliestDate, LatestDate)) S
                                                ON
                                                    T.InstrumentID = S.InstrumentID AND T.Frequency = S.Frequency
                                                WHEN NOT MATCHED THEN
                                                    INSERT (InstrumentID, Frequency, EarliestDate, LatestDate)
                                                    VALUES (InstrumentID, Frequency, EarliestDate, LatestDate)
                                                WHEN MATCHED THEN
                                                    UPDATE SET
                                                        T.EarliestDate = (SELECT MIN(mydate) FROM (VALUES (T.EarliestDate), (S.EarliestDate)) AS AllDates(mydate)),
                                                        T.LatestDate = (SELECT MAX(mydate) FROM (VALUES (T.LatestDate), (S.LatestDate)) AS AllDates(mydate));";

                    ExecuteQuery(cmd);

                    Log(LogLevel.Info,
                        $"Saved {data.Count} data points of {instrument.Symbol} @ {Enum.GetName(typeof(BarSize), frequency)} " +
                        $"to local storage. {(overwrite ? "Overwrite" : "NoOverwrite")} {(adjust ? "Adjust" : "NoAdjust")}");
                }
            }
        }

        private static void AppendBar(Instrument instrument, BarSize frequency, StringBuilder sb, OHLCBar bar)
        {
            sb.AppendFormat(
                "('{0:yyyy-MM-ddTHH:mm:ss.fff}', {1}, {2}, {3}, {4}, {5}, {6}, {7},{8})",
                bar.DateTimeClose,
                instrument.ID,
                (int)frequency,
                bar.Open.ToString(CultureInfo.InvariantCulture),
                bar.High.ToString(CultureInfo.InvariantCulture),
                bar.Low.ToString(CultureInfo.InvariantCulture),
                bar.Close.ToString(CultureInfo.InvariantCulture),
                bar.Volume?.ToString(CultureInfo.InvariantCulture) ?? "NULL",
                bar.DateTimeOpen.HasValue ? $"'{bar.DateTimeOpen.Value:yyyy-MM-ddTHH:mm:ss.fff}'" : "NULL"
            );
        }

        private void ExecuteQuery(SqlCommand cmd)
        {
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "SQL Server query error: " + ex.Message);
            }
        }

        private bool ParameterChecks(List<OHLCBar> data)
        {
           

            if (data.Count == 0)
            {
                Log(LogLevel.Error, "Local storage: asked to add data of 0 length");
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This method allows adding data, but allowing the actual saving of the data to be delayed.
        ///     Useful when you want to allow the data source the ability to make batch inserts/save to file/whatever on its own
        ///     discretion.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="instrument"></param>
        /// <param name="frequency"></param>
        /// <param name="overwrite"></param>
        public void AddDataAsync(List<OHLCBar> data, Instrument instrument, BarSize frequency, bool overwrite = false)
        {
            AddData(data, instrument, frequency, overwrite);
        }

        public void AddData(OHLCBar data, Instrument instrument, BarSize frequency, bool overwrite = false)
        {
            AddData(new List<OHLCBar> { data }, instrument, frequency, overwrite);
        }

        public void UpdateData(List<OHLCBar> data, Instrument instrument, BarSize frequency, bool adjust = false)
        {
            AddData(data, instrument, frequency, true, adjust);
        }

        public void DeleteAllInstrumentData(Instrument instrument)
        {
            if (!TryConnect(out SqlConnection connection))
                throw new Exception("Could not connect to database");
            using (connection)
            {
                using (var cmd = new SqlCommand("", connection))
                {
                    cmd.CommandText = $"DELETE FROM data WHERE InstrumentID = {instrument.ID}";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = $"DELETE FROM instrumentinfo WHERE InstrumentID = {instrument.ID}";
                    cmd.ExecuteNonQuery();
                }
            }

            Log(LogLevel.Info, $"Deleted all data for instrument {instrument}");
        }

        public void DeleteData(Instrument instrument, BarSize frequency)
        {
            if (!TryConnect(out SqlConnection connection))
                throw new Exception("Could not connect to database");
            using (connection)
            {
                using (var cmd = new SqlCommand("", connection))
                {
                    cmd.CommandText =
                        $"DELETE FROM data WHERE InstrumentID = {instrument.ID} AND Frequency = {(int)frequency}";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                        $"DELETE FROM instrumentinfo WHERE InstrumentID = {instrument.ID} AND Frequency = {(int)frequency}";
                    cmd.ExecuteNonQuery();
                }
            }
            Log(LogLevel.Info, $"Deleted all {frequency} data for instrument {instrument}");
        }

        public void DeleteData(Instrument instrument, BarSize frequency, List<OHLCBar> bars)
        {
            if (!TryConnect(out SqlConnection connection))
                throw new Exception("Could not connect to database");

            using (connection)
            {
                using (var cmd = new SqlCommand("", connection))
                {
                    var sb = new StringBuilder();
                    sb.Append("BEGIN TRAN T1;");
                    foreach (OHLCBar t in bars)
                    {
                        sb.AppendFormat("DELETE FROM data WHERE InstrumentID = {0} AND Frequency = {1} AND DateTimeClose = '{2}';",
                            instrument.ID,
                            (int)frequency,
                            frequency < BarSize.OneDay
                                ? t.DateTimeClose.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                : t.DateTimeClose.ToString("yyyy-MM-dd"));
                        //for frequencies greater than a day, we don't care about time
                    }
                    sb.Append("COMMIT TRAN T1;");

                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();

                    //check if there's any data left
                    cmd.CommandText =
                        $"SELECT COUNT(*) FROM data WHERE InstrumentID = {instrument.ID} AND Frequency = {(int)frequency}";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        var count = reader.GetInt32(0);
                        reader.Close();

                        if (count == 0)
                        {
                            //remove from the instrumentinfo table
                            cmd.CommandText =
                                $"DELETE FROM instrumentinfo WHERE InstrumentID = {instrument.ID} AND Frequency = {(int)frequency}";
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            //update the instrumentinfo table
                            cmd.CommandText = string.Format(
                                @"UPDATE instrumentinfo
	                            SET
		                            EarliestDate = (SELECT MIN(DateTimeClose) FROM data WHERE InstrumentID = {0} AND Frequency = {1}),
		                            LatestDate = (SELECT MAX(DateTimeClose) FROM data WHERE InstrumentID = {0} AND Frequency = {1})
	                            WHERE
		                            InstrumentID = {0} AND Frequency = {1}", instrument.ID, (int)frequency);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Log(LogLevel.Info,
                $"Deleted {bars.Count} {frequency} bars for instrument {instrument}");
        }

        public List<StoredDataInfo> GetStoredDataInfo(int instrumentID)
        {
            if (!TryConnect(out SqlConnection connection))
                throw new Exception("Could not connect to database");

            var instrumentInfos = new List<StoredDataInfo>();
            using (connection)
            {
                using (var cmd = new SqlCommand("", connection))
                {
                    cmd.CommandText = "SELECT * FROM instrumentinfo WHERE InstrumentID = " + instrumentID;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var info = new StoredDataInfo
                            {
                                InstrumentID = instrumentID,
                                Frequency = (BarSize)reader.GetInt32(1),
                                EarliestDate = reader.GetDateTime(2),
                                LatestDate = reader.GetDateTime(3)
                            };
                            instrumentInfos.Add(info);
                        }
                    }
                }
            }

            return instrumentInfos;
        }

        public StoredDataInfo GetStoredDataInfo(int instrumentID, BarSize frequency)
        {
            if (!TryConnect(out SqlConnection connection))
                throw new Exception("Could not connect to database");

            var instrumentInfo = new StoredDataInfo();
            using (connection)
            {
                using (var cmd = new SqlCommand("", connection))
                {
                    cmd.CommandText =
                        $"SELECT * FROM instrumentinfo WHERE InstrumentID = {instrumentID} AND Frequency = {(int)frequency}";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            instrumentInfo.InstrumentID = instrumentID;
                            instrumentInfo.Frequency = (BarSize)reader.GetInt32(1);
                            instrumentInfo.EarliestDate = reader.GetDateTime(2);
                            instrumentInfo.LatestDate = reader.GetDateTime(3);
                        }
                        else
                            return null; //return null if nothing is found that matches these criteria
                    }
                }
            }

            return instrumentInfo;
        }

        public event EventHandler<HistoricalDataEventArgs> HistoricalDataArrived;

        public event EventHandler<ErrorArgs> Error;

        public event EventHandler<DataSourceDisconnectEventArgs> Disconnected;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (connectionStatusUpdateTimer != null)
            {
                connectionStatusUpdateTimer.Dispose();
                connectionStatusUpdateTimer = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion IDataStorage Members

        private void _connectionStatusUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Connected = connection.State == ConnectionState.Open;
                }
                catch
                {
                    Connected = false;
                }
            }
        }

        private bool TryConnect(out SqlConnection connection)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Local storage: DB connection failed with error: " + ex.Message);
                return false;
            }

            return connection.State == ConnectionState.Open;
        }

        /// <summary>
        ///     Raise the event in a threadsafe manner
        /// </summary>
        /// <param name="event"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <typeparam name="T"></typeparam>
        private static void RaiseEvent<T>(EventHandler<T> @event, object sender, T e)
            where T : EventArgs
        {
            var handler = @event;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        ///     Add a message to the log.
        /// </summary>
        private void Log(LogLevel level, string message)
        {
            logger.Log(level, message);
        }

        //todo
        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

#pragma warning restore 67