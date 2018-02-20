using MvvmValidation;
using ReactiveUI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ServerGui.ViewModels
{
    public class ValidateableBaseViewModel : BaseViewModel, IValidatable, INotifyDataErrorInfo
    {
        protected ValidationHelper Validator { get; }

        private NotifyDataErrorInfoAdapter NotifyDataErrorInfoAdapter { get; }

        public ValidateableBaseViewModel()// IUnityContainer container)
        {
            Validator = new ValidationHelper();

            NotifyDataErrorInfoAdapter = new NotifyDataErrorInfoAdapter(Validator);
            NotifyDataErrorInfoAdapter.ErrorsChanged += OnErrorsChanged;
        }

        private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            // Notify the UI that the property has changed so that the validation error gets displayed (or removed).
            this.RaisePropertyChanged(e.PropertyName);
        }

        #region Implementation of INotifyDataErrorInfo

        public IEnumerable GetErrors(string propertyName)
        {
            return NotifyDataErrorInfoAdapter.GetErrors(propertyName);
        }

        public bool HasErrors => NotifyDataErrorInfoAdapter.HasErrors;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
        {
            add { NotifyDataErrorInfoAdapter.ErrorsChanged += value; }
            remove { NotifyDataErrorInfoAdapter.ErrorsChanged -= value; }
        }

        public Task<ValidationResult> Validate()
        {
            return Validator.ValidateAllAsync();
        }

        #endregion Implementation of INotifyDataErrorInfo
    }
}