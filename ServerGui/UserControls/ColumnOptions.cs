using System;
using System.ComponentModel;

namespace ServerGui.UserControls
{
    /// <summary>
    /// Holds options for a DataGrid Column.
    /// </summary>
    [Serializable]
    public class ColumnOptions
    {
        public int DisplayIndex { get; set; }
        public double Width { get; set; }
        public ListSortDirection? SortDirection { get; set; }
    }
}