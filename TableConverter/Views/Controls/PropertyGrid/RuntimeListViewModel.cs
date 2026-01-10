using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using SukiUI.Controls;

namespace TableConverter.Views.Controls.PropertyGrid;

public sealed class RuntimeValuesViewModel : PropertyViewModelBase<string?>
{
    public RuntimeValuesViewModel(
        INotifyPropertyChanged viewmodel, 
        string displayName, 
        PropertyInfo propertyInfo,
        string valuePath) : base(viewmodel, displayName, propertyInfo)
    {
        var sourceProp = viewmodel.GetType().GetProperty(valuePath,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (sourceProp == null)
            throw new InvalidOperationException($"Property '{valuePath}' not found");

        var value = sourceProp.GetValue(viewmodel);

        if (value is not IEnumerable enumerable)
            throw new InvalidOperationException($"Property '{valuePath}' must be IEnumerable");

        Values = (IEnumerable<object>)enumerable;
    }

    public IEnumerable<object> Values { get; set; }
}