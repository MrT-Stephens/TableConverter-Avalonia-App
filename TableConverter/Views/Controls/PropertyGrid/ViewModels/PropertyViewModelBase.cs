using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FastMember;
using SukiUI.Helpers;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels
{
    public abstract class PropertyViewModelBase<T> : SukiObservableObject, IPropertyViewModel<T?>
    {
        private readonly string _propertyName;

        private T? _Value;

        public T? Value
        {
            get => _Value;
            set
            {
                if (SetAndRaise(ref _Value, value))
                {
                    ViewModelSetter(value);
                    GetErrors(nameof(Value));
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
                }
            }
        }

        object? IPropertyViewModel.Value
        {
            get => Value;
            set
            {
                if (SetAndRaise(ref _Value, (T?)value))
                {
                    ViewModelSetter((T?)value);
                    GetErrors(nameof(Value));
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
                }
            }
        }
        
        private bool _HasErrors;
        public bool HasErrors
        {
            get => _HasErrors;
            protected set => SetAndRaise(ref _HasErrors, value);
        }
        
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public string DisplayName { get; }
        public bool IsReadOnly { get; init; }
        protected Member PropertyInfo { get; }
        protected INotifyPropertyChanged Viewmodel { get; }
        protected ObjectAccessor ViewModelAccessor { get; }

        public PropertyViewModelBase(
            INotifyPropertyChanged viewmodel, 
            string displayName, 
            Member propertyInfo, 
            ObjectAccessor viewModelAccessor)
        {
            Viewmodel = viewmodel;
            DisplayName = displayName;
            PropertyInfo = propertyInfo;
            IsReadOnly = !propertyInfo.CanWrite;
            ViewModelAccessor = viewModelAccessor;
            _propertyName = propertyInfo.Name;
            _Value = ViewModelGetter();
            Viewmodel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_propertyName == e.PropertyName)
            {
                Value = ViewModelGetter();
            }
        }

        protected T? ViewModelGetter()
        {
            return (T?)ViewModelAccessor[PropertyInfo.Name];
        }

        protected void ViewModelSetter(T? newValue)
        {
            if (PropertyInfo.CanWrite)
            {
                ViewModelAccessor[PropertyInfo.Name] = newValue;
            }
        }

        public void Dispose()
        {
            Viewmodel.PropertyChanged -= OnPropertyChanged;
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName != _propertyName && propertyName != nameof(Value))
            {
                return new List<string>();
            }

            var validationContext = new ValidationContext(Viewmodel, null, null) 
            {
                MemberName = _propertyName
            };
            
            var validationResults = new List<ValidationResult>();
    
            if (!Validator.TryValidateProperty(Value, validationContext, validationResults))
            {
                HasErrors = true;
                return validationResults
                    .Select(r => r.ErrorMessage ?? "Validation failed")
                    .ToList();
            }

            HasErrors = false;
            return new List<string>();
        }
    }
}