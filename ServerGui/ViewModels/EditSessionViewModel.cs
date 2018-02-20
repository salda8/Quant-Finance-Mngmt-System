using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reactive;
using System.Windows;
using Common;
using Common.EntityModels;
using Common.Enums;
using Common.ExtensionMethods;
using Common.Utils;
using ReactiveUI;

namespace ServerGui.ViewModels
{
    public class EditSessionViewModel : BaseViewModel
    {
        private readonly SessionTemplate originalTemplate;
        private TemplateSession selectedSession;
        private SessionTemplate theSessionTemplate;
        private bool sessionAddedRemovedOrChanged = false;

        public EditSessionViewModel(SessionTemplate template)
        {
            if (template == null)
            {
                TheSessionTemplate = new SessionTemplate() { ID = -1, Sessions = new List<TemplateSession>() { new TemplateSession { IsSessionEnd = true, OpeningDay = DayOfTheWeek.Monday, ClosingDay = DayOfTheWeek.Monday } } };
                ModifyBtnContent = "Add";
                WindowTitle = "Add New Session";
                AddSessionCommand = ReactiveCommand.Create(AddSession);
            }
            else
            {
                template.Sessions = template.Sessions.OrderBy(x => x.OpeningDay).ThenBy(x => x.OpeningTime).ToList();
                originalTemplate = template;
                TheSessionTemplate = (SessionTemplate) template;
                ModifyBtnContent = "Modify";
                WindowTitle = $"Edit Session {template.Name}";
                IObservable<bool> canAddEditSession = this.WhenAny(x => x.TheSessionTemplate.Sessions, x => x.Value != null);
                AddSessionCommand = ReactiveCommand.Create(AddSession, canAddEditSession);

            }

            ModifyCommand = ReactiveCommand.Create(Modify);
           
            var canDeleteSession = this.WhenAny(x => x.SelectedSession, x => x.Value != null);
            RemoveSessionCommand = ReactiveCommand.Create(() => TheSessionTemplate.Sessions.Remove(SelectedSession),
                canDeleteSession);
        }

        public ReactiveCommand<Unit, bool> RemoveSessionCommand { get; set; }

        public ReactiveCommand<Unit, Unit> AddSessionCommand { get; set; }

      public ReactiveCommand<Unit, Unit> ModifyCommand { get; set; }

        public string WindowTitle { get; set; } 

        public TemplateSession SelectedSession
        {
            get { return selectedSession; }
            set { this.RaiseAndSetIfChanged(ref selectedSession, value); }
        }

        public string ModifyBtnContent { get; set; }

        public List<TemplateSession> Sessions { get; set; }

        public SessionTemplate TheSessionTemplate
        {
            get { return theSessionTemplate; }
            set { this.RaiseAndSetIfChanged(ref theSessionTemplate, value); }
        }

        public bool SessionAddedRemovedOrChanged
        {
            get { return sessionAddedRemovedOrChanged; }
            set { this.RaiseAndSetIfChanged(ref sessionAddedRemovedOrChanged, value); }
        }
        //todo move it to session repostitory?
        private void Modify()
        {
            //ensure sessions don't overlap
            if (!MyUtils.ValidateSessions(TheSessionTemplate.Sessions.ToList(), out List<string> error))
            {
                MessageBox.Show(error.First());
                return;
            }

            //save to db

            var nameExists = DbContext.SessionTemplates.Any(x => x.Name == TheSessionTemplate.Name);
                var addingNew = TheSessionTemplate.ID == -1;

                if (nameExists && (addingNew || originalTemplate.Name != TheSessionTemplate.Name))
                {
                    MessageBox.Show("Name already exists, please change it.");
                    return;
                }

                if (addingNew)
                {
                    DbContext.SessionTemplates.Add(TheSessionTemplate);
                }
                else
                {
                    DbContext.SessionTemplates.Attach(originalTemplate);
                    DbContext.Entry(originalTemplate).CurrentValues.SetValues(TheSessionTemplate);
                }

                DbContext.SaveChanges();

                //find removed sessions and mark them as deleted
                if (originalTemplate != null)
                {
                    var removedSessions =
                        originalTemplate.Sessions.Where(x => TheSessionTemplate.Sessions.All(y => y.ID != x.ID))
                            .ToList();
                    foreach (var t in removedSessions)
                        DbContext.Entry(t).State = EntityState.Deleted;


                    //find the ones that overlap and modify them, if not add them
                    foreach (var s in TheSessionTemplate.Sessions)
                        if (s.ID != 0) //this means it's not newly added
                        {
                            var session = originalTemplate.Sessions.First(x => x.ID == s.ID);
                            DbContext.TemplateSessions.Attach(session);
                            DbContext.Entry(session).CurrentValues.SetValues(s);
                        }
                }

                DbContext.SaveChanges();

                //find instruments using this exchange as session source, and update their sessions
                if (TheSessionTemplate.ID != -1)
                {
                    var instruments =
                        DbContext.Instruments.Where(
                                x => x.SessionsSource == SessionsSource.Template && x.ExchangeID == TheSessionTemplate.ID)
                            .ToList();
                    foreach (var i in instruments)
                    {
                        DbContext.InstrumentSessions.RemoveRange(i.Sessions);
                        i.Sessions.Clear();

                        foreach (var s in TheSessionTemplate.Sessions)
                            i.Sessions.Add(s.ToInstrumentSession());
                    }
                }

                DbContext.SaveChanges();
                SessionAddedRemovedOrChanged = true;


        }

        private void AddSession()
        {
            var sessionToAdd = new TemplateSession {IsSessionEnd = true};

            if (TheSessionTemplate.Sessions?.Count == 0)
            {
                sessionToAdd.OpeningDay = DayOfTheWeek.Monday;
                sessionToAdd.ClosingDay = DayOfTheWeek.Monday;
               
            }
            else
            {
                var maxDay = (DayOfTheWeek) Math.Min(6, TheSessionTemplate.Sessions.Max(x => (int) x.OpeningDay) + 1);
                sessionToAdd.OpeningDay = maxDay;
                sessionToAdd.ClosingDay = maxDay;
            }

            TheSessionTemplate.Sessions.Add(sessionToAdd);
        }
    }
}