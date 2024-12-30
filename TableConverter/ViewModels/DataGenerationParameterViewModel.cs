using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NPOI.SS.Formula.Functions;

namespace TableConverter.ViewModels;

public partial class DataGenerationParameterViewModel : ObservableObject
{
    #region Constructor

    public DataGenerationParameterViewModel(string name, object? defaultValue, Type type)
    {
        Name = name;
        Type = type;

        if (defaultValue is not null)
            Value = defaultValue;
        else
            Value = type.IsValueType 
                ? Activator.CreateInstance(type) 
                : Convert.ChangeType(null, type);
    }

    partial void OnValueChanged(object? oldValue, object? newValue)
    {
        var str = newValue;
    }

    #endregion

    #region Properties

    [ObservableProperty] private string _Name;

    [ObservableProperty] private object? _Value;

    [ObservableProperty] private Type _Type;

    #endregion
}