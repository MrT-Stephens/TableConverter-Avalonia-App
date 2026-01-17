using System;
using System.ComponentModel;
using Avalonia.Collections;
using FastMember;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels
{
    public sealed class EnumViewModel : PropertyViewModelBase<Enum?>
    {
        public IAvaloniaReadOnlyList<object> Values { get; }

        public EnumViewModel(
            INotifyPropertyChanged viewmodel, 
            string displayName, 
            Member propertyInfo,
            ObjectAccessor viewModelAccessor)
            : base(viewmodel, displayName, propertyInfo, viewModelAccessor)
        {
            var temp = new AvaloniaList<object>();
            
            foreach (var item in Enum.GetValues(propertyInfo.Type))
            {
                temp.Add(item);
            }

            Values = temp;
        }
    }
}