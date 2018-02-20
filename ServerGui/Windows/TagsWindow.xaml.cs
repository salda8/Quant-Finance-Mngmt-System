using ReactiveUI;
using ServerGui.ViewModels;
using System.Reactive.Linq;
using System.Windows;

namespace ServerGui.Windows
{
    /// <summary>
    ///     Interaction logic for TagsWindow.xaml
    /// </summary>
    public partial class TagsWindow : IViewFor<TagsViewModel>
    {
        public TagsWindow()
        {
            ViewModel = new TagsViewModel();
            DataContext = ViewModel;
            InitializeComponent();
            this.Events().Closing.InvokeCommand(ViewModel.CloseCommand);
        }

        public TagsViewModel ViewModel { get; set; }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadTags.Execute();
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TagsViewModel)value; }
        }
    }
}