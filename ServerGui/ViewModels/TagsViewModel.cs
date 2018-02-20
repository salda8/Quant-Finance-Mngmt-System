using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Common;
using Common.EntityModels;
using ReactiveUI;

namespace ServerGui.ViewModels
{
    public class TagsViewModel : BaseViewModel
    {
        private ReactiveObservableCollection<TagViewModel> tags;
        private TagViewModel selectedTag;

        public TagViewModel NewTag { get; set; } = new TagViewModel(new Tag());

        public ReactiveObservableCollection<TagViewModel> Tags
        {
            get { return tags; }
            set { this.RaiseAndSetIfChanged(ref tags, value); }
        }

        public TagViewModel SelectedTag
        {
            get { return selectedTag; }
            set { this.RaiseAndSetIfChanged(ref selectedTag, value); }
        }

        public TagsViewModel()
        {
            Tags = new ReactiveObservableCollection<TagViewModel>()
            {
                ChangeTrackingEnabled = true
            };

            var canDeleteEdit = this.WhenAny(x => x.SelectedTag, x => x.Value != null);

            LoadTags = ReactiveCommand.Create(LoadAllTags);

            Add = ReactiveCommand.Create(AddNewTag, this.WhenAny(x => x.NewTag.Name, x => !string.IsNullOrWhiteSpace(x.Value)));

            Delete = ReactiveCommand.Create(DeleteTag, canDeleteEdit);

            Save = ReactiveCommand.Create(SaveTag, canDeleteEdit);

            //When changing the selected tag, reset the delete confirmation
            this.WhenAnyValue(x => x.SelectedTag)
                .Buffer(1, 1)
                .Subscribe(x => { TagViewModel tagVm = x.FirstOrDefault(); if (tagVm != null) tagVm.ConfirmDelete = false; });
        }

        private void SaveTag()
        {
            var tagToEdit = DbContext.Tags.Find(SelectedTag?.Model?.ID);
            tagToEdit = SelectedTag?.Model;
            DbContext.SaveChanges();
            Tags.First(x => x.Name == SelectedTag.Name).Name = SelectedTag.Name;
            SelectedTag = null;
            TagAddedRemovedOrChanged = true;
        }

        private void DeleteTag()
        {
            DbContext.Tags.Remove(SelectedTag.Model);
            DbContext.SaveChanges();
            Tags.Remove(Tags.FirstOrDefault(x => x.Model.ID == SelectedTag.Model.ID));
            TagAddedRemovedOrChanged = true;
        }

        private void AddNewTag()
        {
            DbContext.Tags.Add(NewTag.Model);
            DbContext.SaveChanges();
            Tags.Add(new TagViewModel(NewTag.Model));
            TagAddedRemovedOrChanged = true;
           

        }

        public bool TagAddedRemovedOrChanged { get; set; } = false;

        private void LoadAllTags()
        {
            foreach (var tag in DbContext.Tags.ToList())
            {
                Tags.Add(new TagViewModel(tag));
            }
        }

        public ReactiveCommand<Unit, Unit> LoadTags { get; set; }

        public ReactiveCommand<Unit, Unit> Add { get; set; }
        public ReactiveCommand<Unit, Unit> Save { get; set; }
        public ReactiveCommand<Unit, Unit> Delete { get; set; }
    }

    public class ReactiveObservableCollection<T> : ReactiveList<T>
    {

        public ObservableCollection<T> ObservableCollection { private set; get; }

        public ReactiveObservableCollection()
        {
            this.ObservableCollection = new ObservableCollection<T>();
            ItemsAdded.Subscribe(ObservableCollection.Add);
            ItemsRemoved.Subscribe((x) => ObservableCollection.Remove(x));
        }
    }
}