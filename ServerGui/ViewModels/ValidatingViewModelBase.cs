using FluentValidation;
using FluentValidation.Results;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ServerGui.ViewModels
{
    public abstract class ValidatingViewModelBase<T> : ReactiveObject, INotifyDataErrorInfo where T : class
    {
        public T Model { get; }
        private readonly AbstractValidator<T> validator;
        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        private bool hasErrors;

        protected ValidatingViewModelBase(T model, AbstractValidator<T> validator)
        {
            Model = model;
            this.validator = validator;
            
            this.PropertyChanged += (s, e) => Validate(e.PropertyName);
        }

        protected void Validate(string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                ValidateObject();
            }
            else
            {
                ValidateProperty(propertyName);
            }
        }

        /// <summary>
        /// Validate only a particular property
        /// </summary>
        /// <param name="propertyName"></param>
        private void ValidateProperty(string propertyName)
        {
            var result = validator.Validate(Model, propertyName);
            if (!errors.ContainsKey(propertyName)) errors.Add(propertyName, new List<string>());

            var newErrors = result.Errors.Select(x => x.ErrorMessage).ToList();
            var notify = newErrors.Intersect(errors[propertyName]).Count() != newErrors.Count;
            errors[propertyName] = newErrors;
            HasErrors = errors.Any(x => x.Value.Count > 0);

            if (notify)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Validate the entire object
        /// </summary>
        private void ValidateObject()
        {
            var result = validator.Validate(Model);
            HasErrors = result.IsValid;

            var newErrors = GetErrorDict(result.Errors);
            var oldErrors = errors;
            errors = newErrors;
            NotifyChangedErrors(oldErrors, newErrors);
        }

        private void NotifyChangedErrors(Dictionary<string, List<string>> oldErrors, Dictionary<string, List<string>> newErrors)
        {
            var removed = oldErrors.Keys.Except(newErrors.Keys);
            var added = newErrors.Keys.Except(oldErrors.Keys);
            var changed = oldErrors.Keys.Intersect(newErrors.Keys).Where(x => oldErrors[x].Any(y => !newErrors[x].Contains(y)));
            var allChangedErrors = removed.Concat(changed).Concat(added);
            foreach (var prop in allChangedErrors)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
            }
        }

        private Dictionary<string, List<string>> GetErrorDict(IList<ValidationFailure> validationFailures)
        {
            var dict = new Dictionary<string, List<string>>();

            foreach (var failure in validationFailures)
            {
                if (!dict.ContainsKey(failure.PropertyName))
                {
                    dict.Add(failure.PropertyName, new List<string>());
                }

                dict[failure.PropertyName].Add(failure.ErrorMessage);
            }

            return dict;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == null)
            {
                //it wants all the errors
                return errors.SelectMany(x => x.Value).ToList();
            }

            if (errors != null && errors.TryGetValue(propertyName, out List<string> failures))
            {
                return failures.ToList();
            }
            return null;
        }

        public bool HasErrors
        {
            get { return hasErrors; }
            set { this.RaiseAndSetIfChanged(ref hasErrors, value); }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}