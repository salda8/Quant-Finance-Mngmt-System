using System;
using Common;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Common.EntityModels;
using Common.Enums;

namespace ServerGui.ViewModels
{
    public class SessionTemplatesViewModel : BaseViewModel
    {
        private SessionTemplate selectedSessionTemplate;
        private ReactiveList<SessionTemplate> sessionTemplates;
        private bool windowOpened;
        private bool sessionAddedRemovedOrChanged;

        public SessionTemplatesViewModel()
        {
            ReloadTemplates();

            AddCommand = ReactiveCommand.Create(() => OpenEditSessionWindow());
            DeleteCommand = ReactiveCommand.Create(Delete);
            ModifyCommand = ReactiveCommand.Create(() => OpenEditSessionWindow(selectedSessionTemplate));
        }

        public SessionTemplate SelectedSessionTemplate
        {
            get { return selectedSessionTemplate; }
            set { this.RaiseAndSetIfChanged(ref selectedSessionTemplate, value); }
        }

        public ReactiveCommand<Unit, Unit> ModifyCommand { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }

        public ReactiveCommand<Unit, Unit> AddCommand { get; set; }

        public ReactiveList<SessionTemplate> SessionTemplates
        {
            get { return sessionTemplates; }
            set { this.RaiseAndSetIfChanged(ref sessionTemplates, value); }
        }

        private void Delete()
        {
            var selectedTemplate = selectedSessionTemplate;
            if (selectedTemplate == null) return;

            var instrumentCount =
                DbContext.Instruments.Count(
                    x => x.SessionTemplateID == selectedTemplate.ID);
            if (instrumentCount > 0)
            {
                MessageBox.Show(
                    $"Can't delete this template it has {instrumentCount} instruments assigned to it.");
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selectedTemplate.Name}?", "Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) return;

            DbContext.SessionTemplates.Attach(selectedTemplate);
            DbContext.SessionTemplates.Remove(selectedTemplate);
            DbContext.SaveChanges();

            SessionTemplates.Remove(selectedTemplate);
        }

        private void OpenEditSessionWindow(SessionTemplate template = null)
        {
            SessionAddedRemovedOrChanged = false;
            var window = new Windows.EditSessionTemplateWindow(template);
            window.Show();
            windowOpened = true;
            window.Events().Closing.Subscribe(x => this.windowOpened = false);
            Task.Run(() =>
            {
                var addedRemovedOrChanged = false;

                while (windowOpened)
                {
                    if (window.ViewModel.SessionAddedRemovedOrChanged)
                    {
                        addedRemovedOrChanged = true;
                    }
                    
                    new ManualResetEvent(false).WaitOne(200);
                }

                SessionAddedRemovedOrChanged = addedRemovedOrChanged;//mainwindow is subscribed to this value and will rebuild the tag context menu
            });
        }

        public bool SessionAddedRemovedOrChanged
        {
            get { return sessionAddedRemovedOrChanged; }
            set
            {
                if (value)
                {
                    ReloadTemplates();
                }
                this.RaiseAndSetIfChanged(ref sessionAddedRemovedOrChanged, value);
            }
        }

        private void ReloadTemplates()
        {
            SessionTemplates = new ReactiveList<SessionTemplate>();

            IOrderedEnumerable<SessionTemplate> templates =
                DbContext.SessionTemplates.Include("Sessions").ToList().OrderBy(x => x.Name);
            foreach (SessionTemplate s in templates)
            {
                SessionTemplates.Add(s);
            }
        }
    }
}