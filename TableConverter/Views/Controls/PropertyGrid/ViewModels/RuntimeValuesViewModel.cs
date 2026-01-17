using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FastMember;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels;

public sealed class RuntimeValuesViewModel : PropertyViewModelBase<string?>
{
    public RuntimeValuesViewModel(
        INotifyPropertyChanged viewmodel, 
        string displayName, 
        string valuePath,
        Member propertyInfo,
        ObjectAccessor viewModelAccessor) 
        : base(viewmodel, displayName, propertyInfo, viewModelAccessor)
    {
        var value = viewModelAccessor[valuePath];

        if (value is not IEnumerable enumerable)
            throw new InvalidOperationException($"Property '{valuePath}' must be IEnumerable");

        Values = (IEnumerable<object>)enumerable;
    }

    public IEnumerable<object> Values { get; set; }
}