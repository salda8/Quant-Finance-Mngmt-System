namespace Common
{
    public class DataSourceStatus
    {
        public DataSourceStatus()
        {
        }

        public string Name { get; set; }

        /// <summary>
        /// Set to true if connected, false is not, null if N/A
        /// </summary>
        public bool? RealtimeConnected { get; set; }

        /// <summary>
        /// Set to true if connected, false is not, null if N/A
        /// </summary>
        public bool? HistoricalConnected { get; set; }

        /// <summary>
        /// Set to true if connected, false is not, null if N/A
        /// </summary>
        public bool? EconReleasesConnected { get; set; }
    }
}