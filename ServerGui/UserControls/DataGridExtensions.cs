using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace ServerGui.UserControls
{
    public static class DataGridExtensions
    {
        public static void SerializeLayout(this DataGrid grid, StreamWriter sw)
        {
            var allSettings = grid.Columns.Select(c => new ColumnOptions
            {
                DisplayIndex = c.DisplayIndex,
                Width = c.ActualWidth,
                SortDirection = c.SortDirection
            }).ToList();

            var serializer = new XmlSerializer(typeof(List<ColumnOptions>));

            serializer.Serialize(sw, allSettings);
        }

        public static void DeserializeLayout(this DataGrid grid, string settings)
        {
            List<ColumnOptions> allSettings;
            var serializer = new XmlSerializer(typeof(List<ColumnOptions>));
            using (var sw = new StringReader(settings))
            {
                allSettings = (List<ColumnOptions>)serializer.Deserialize(sw);
            }

            for (int i = 0; i < allSettings.Count; i++)
            {
                ColumnOptions co = allSettings[i];
                grid.Columns[i].Width = co.Width;
                grid.Columns[i].SortDirection = co.SortDirection;
                grid.Columns[i].DisplayIndex = co.DisplayIndex;
            }
        }
    }
}