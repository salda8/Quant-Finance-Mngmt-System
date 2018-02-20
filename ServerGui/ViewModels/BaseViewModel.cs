using Common.Interfaces;
using NLog;
using ReactiveUI;
using Splat;
using System;
using System.ComponentModel;
using System.Reactive;
using CommonStandard.Interfaces;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace ServerGui.ViewModels
{
    public abstract class BaseViewModel : ReactiveObject, IDisposable
    {
        public IMyDbContext DbContext { get; }

        protected BaseViewModel()
        {
            CloseCommand = ReactiveCommand.Create<CancelEventArgs>((cancelEventArg) => { Dispose(); });
            DbContext = Locator.Current.GetService<IMyDbContext>();
        }

        protected Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public ReactiveCommand<CancelEventArgs, Unit> CloseCommand { get; set; }

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        public void Dispose()
        {
            //DbContext?.cDispose();
            GC.SuppressFinalize(this);
        }
    }
}