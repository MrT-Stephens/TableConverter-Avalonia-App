using System.ComponentModel;
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
                }
            }
        }

        object? IPropertyViewModel.Value
        {
            get => Value;
            set => Value = (T?)value;
        }

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
    }
}