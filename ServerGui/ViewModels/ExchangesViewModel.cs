using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using Common;
using Common.EntityModels;
using ReactiveUI;

namespace ServerGui.ViewModels
{
    public class ExchangesViewModel : BaseViewModel
    {
        private List<Exchange> exchanges;
        private ReactiveList<Exchange> filteredExchanges;
        private string searchBoxText;
        private Exchange selectedExchange;


        public ExchangesViewModel()
        {
            
            Exchanges = DbContext.Exchanges.ToList();


            FilteredExchanges = new ReactiveList<Exchange>(Exchanges.OrderBy(x => x.Name));
            SelectedExchange = Exchanges.First();
            var canExecute = this.WhenAny(x => x.SelectedExchange, x => x.Value != null);

            AddCommand = ReactiveCommand.Create(Add);
            DeleteCommand = ReactiveCommand.Create(Delete, canExecute);
            EditCommand = ReactiveCommand.Create(Modify, canExecute);
            var canSearch = this.WhenAny(x => x.SearchBoxText,
                x => !string.IsNullOrWhiteSpace(x.Value));

            this.WhenAnyObservable(x => x.FilteredExchanges.ItemsAdded)
                .Subscribe(async item => { await ItemsAddedTask(item); });

            this.WhenAnyObservable(x => x.FilteredExchanges.ItemsRemoved)
                .Subscribe(async item => { await ItemsRemovedTask(item); });


            DeleteCommand = ReactiveCommand.Create(() => { FilteredExchanges.Remove(SelectedExchange); });

            SearchCommand = ReactiveCommand.Create(Search, canSearch);

            this.WhenAny(x => x.SearchBoxText, x => x.Value)
                .Subscribe(text => { SearchCommand.Execute(); });
        }

        public Exchange SelectedExchange
        {
            get { return selectedExchange; }
            set { this.RaiseAndSetIfChanged(ref selectedExchange, value); }
        }

        public ReactiveList<Exchange> FilteredExchanges
        {
            get { return filteredExchanges; }
            set { this.RaiseAndSetIfChanged(ref filteredExchanges, value); }
        }

        public string SearchBoxText
        {
            get { return searchBoxText; }
            set { this.RaiseAndSetIfChanged(ref searchBoxText, value); }
        }

        public List<Exchange> Exchanges
        {
            get { return exchanges; }
            set { this.RaiseAndSetIfChanged(ref exchanges, value); }
        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; set; }
        public ReactiveCommand<Unit, Unit> EditCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; set; }

        private async Task ItemsAddedTask(Exchange item)
        {
            IsBusy = true;
            DbContext.Exchanges.Add(item);
            await DbContext.SaveChangesAsync();
            IsBusy = false;
        }

        private async Task ItemsRemovedTask(Exchange item)
        {
            IsBusy = true;
            DbContext.Exchanges.Attach(item);
            DbContext.Exchanges.Remove(item);
            await DbContext.SaveChangesAsync();
            IsBusy = false;
        }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
                FilteredExchanges =
                    new ReactiveList<Exchange>(Exchanges.OrderBy(x => x.Name));
            }
            else
            {
                var searchText = SearchBoxText.ToLower();
                var filteredElements = Exchanges
                    .Where(e => e.Name.ToLower().Contains(searchText) ||
                                e.LongName != null &&
                                e.LongName.ToLower().Contains(searchText))
                    .OrderBy(e => e.Name);
                FilteredExchanges = new ReactiveList<Exchange>(filteredElements);
            }
        }


        private void Add()
        {
            var window = new Windows.EditExchangeWindow(this, null, true);
            window.ShowDialog();
        }

        private void Modify()
        {
            if (SelectedExchange == null) return;

            var window = new Windows.EditExchangeWindow(this, selectedExchange);
            window.ShowDialog();
        }

        private void Delete()
        {
            var curentlySelectedExchange = SelectedExchange;
            if (curentlySelectedExchange == null) return;

           
                var instrumentCount =
                    DbContext.Instruments.Count(x => x.ExchangeID == curentlySelectedExchange.ID);
                if (instrumentCount > 0)
                {
                    MessageBox.Show(
                        $"Can't delete this exchange it has {instrumentCount} instruments assigned to it.");
                    return;
                }
            

            var result = MessageBox.Show(
                $"Are you sure you want to delete {curentlySelectedExchange.Name}?", "Delete",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) return;

            
                DbContext.Exchanges.Attach(curentlySelectedExchange);
                DbContext.Exchanges.Remove(curentlySelectedExchange);
                DbContext.SaveChanges();
           

            Exchanges.Remove(curentlySelectedExchange);
        }
    }
}