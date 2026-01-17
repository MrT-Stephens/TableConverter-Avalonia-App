using System;
using System.ComponentModel;
using FastMember;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels
{
    public sealed class DateTimeViewModel : PropertyViewModelBase<DateTime?>
    {
        public DateTimeViewModel(
            INotifyPropertyChanged viewmodel, 
            string displayName, 
            Member propertyInfo, 
            ObjectAccessor viewModelAccessor) 
            : base(viewmodel, displayName, propertyInfo, viewModelAccessor)
        {
        }
    }
}