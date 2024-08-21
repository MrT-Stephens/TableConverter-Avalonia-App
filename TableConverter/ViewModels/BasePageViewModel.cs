using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TableConverter.ViewModels;

public abstract partial class BasePageViewModel : ObservableValidator
{
    [ObservableProperty]
    private string _Name;

    [ObservableProperty]
    private object _Icon;

    [ObservableProperty]
    private int _Index;

    public BasePageViewModel(string name, object? icon, int index = 0)
    {
        ArgumentNullException.ThrowIfNull(icon, nameof(icon));

        Name = name;
        Icon = icon;
        Index = index;
    }
}
