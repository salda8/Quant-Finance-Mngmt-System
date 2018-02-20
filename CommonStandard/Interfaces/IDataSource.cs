using Common.EventArguments;
using System;
using System.ComponentModel;

namespace Common.Interfaces
{
    public interface IDataSource : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Connect to the data source.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from the data source.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Whether the connection to the data source is up or not.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// The name of the data source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Fires on any error.
        /// </summary>
        event EventHandler<ErrorArgs> Error;

        /// <summary>
        /// Fires on disconnection from the data source.
        /// </summary>
        event EventHandler<DataSourceDisconnectEventArgs> Disconnected;
    }
}