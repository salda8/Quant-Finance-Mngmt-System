﻿using System;

namespace Common.EventArguments
{
    public class DataSourceDisconnectEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the datasource.
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Event arguments for a data source disconnection event.
        /// </summary>
        public DataSourceDisconnectEventArgs(string sourceName, string message)
        {
            SourceName = sourceName;
            Message = message;
        }
    }
}