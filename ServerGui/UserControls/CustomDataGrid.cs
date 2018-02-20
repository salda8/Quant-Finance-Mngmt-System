﻿using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ServerGui.UserControls
{
    public class CustomDataGrid : DataGrid
    {

        public CustomDataGrid()
        {
            this.SelectionChanged += CustomDataGrid_SelectionChanged;
        }

        void CustomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          SelectedItemsList.Clear();
            foreach (var item in this.SelectedItems) { this.SelectedItemsList.Add(item); }
        }
        #region SelectedItemsList

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
                DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(CustomDataGrid), new PropertyMetadata(null));

        #endregion
    }
}
