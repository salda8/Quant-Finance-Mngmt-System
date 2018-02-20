using System.ComponentModel;

namespace ServerGui
{
    public class CheckBoxItem<T>
    {
        private bool isChecked;
        private T item;

        public CheckBoxItem(T item, bool isChecked = false)
        {
            this.item = item;
            this.isChecked = isChecked;
        }

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyPropertyChanged("IsChecked");
            }
        }

        public T Item
        {
            get { return item; }
            set
            {
                item = value;
                NotifyPropertyChanged("Item");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}