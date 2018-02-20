using Common.EntityModels;
using ReactiveUI;
using Server.Validation;

namespace ServerGui.ViewModels
{
    public class TagViewModel : ValidatingViewModelBase<Tag>
    {
        private bool confirmDelete;

        /// <summary>
        /// Used to require two clicks on the delete button before deleting a tag
        /// </summary>
        public bool ConfirmDelete
        {
            get { return confirmDelete; }
            set { this.RaiseAndSetIfChanged(ref confirmDelete, value); }
        }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (value == Model.Name) return;
                Model.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public TagViewModel(Tag tag) : base(tag, new TagValidator())
        {
        }
    }
}