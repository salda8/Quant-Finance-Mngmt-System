using System;
using System.Linq;
using System.Reactive;
using Common;
using Common.EntityModels;
using Common.Interfaces;
using DataAccess;
using ReactiveUI;

namespace ServerGui.ViewModels
{
    public class EditExchangesViewModel : BaseViewModel
    {
        private const string TitleForAdd = "Add new exchange";
        private const string TitleForModify = "Modify exchange";
        private readonly IMyDbContext context;

        private readonly Exchange originalExchange;
        
        public EditExchangesViewModel(ExchangesViewModel exchangesViewModel, Exchange exchangeToEdit)
        {
            context = DbContext;
            TimeZones = new ReactiveList<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones().ToList());
           
            ExchangesViewModel = exchangesViewModel;
            if (exchangeToEdit == null)
            {
                Title = TitleForAdd;
                Exchange = new Exchange {ID = -1};
            }
            else
            {
                Title = TitleForModify;
                Exchange = exchangeToEdit;
                originalExchange =
                    context.Exchanges
                        .Single(e => e.ID == Exchange.ID);
            }

            SaveCommand = ReactiveCommand.Create(Save);

            SaveCommand.ThrownExceptions.Subscribe(ex => { MessageBus.Current.SendMessage(ex); });


          
        }

        

        public ExchangesViewModel ExchangesViewModel { get; set; }
        public Exchange Exchange { get; set; }
        public ReactiveList<TimeZoneInfo> TimeZones { get; set; }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }


        public string Title { get; set; }

        private void Save()
        {
            //MyUtils.ValidateSessions(Exchange.Sessions.ToList<ISession>(), out List<string> errors);
            var nameExists = context.Exchanges.Any(x => x.Name == Exchange.Name);
            if (nameExists && originalExchange?.Name != Exchange.Name)
            {
                MessageBus.Current.SendMessage("Name already exists, please change it.");
                return;
            }
            if (Title == TitleForAdd)
            {
                ExchangesViewModel.Exchanges.Add(Exchange);
            }
            else
            {
                //_context.Exchanges.Attach(_originalExchange);
                context.Entry(originalExchange).CurrentValues.SetValues(Exchange);
                context.SaveChanges();

               
            }
        }
    }
}