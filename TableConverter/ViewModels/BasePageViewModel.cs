using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TableConverter.ViewModels;

public abstract partial class BasePageViewModel : ObservableValidator
{
    [ObservableProperty]
    private string _DisplayName;

    [ObservableProperty]
    private object _Icon;

    [ObservableProperty]
    private int _Index;

    public BasePageViewModel(string displayName, object? icon, int index = 0)
    {
        ArgumentNullException.ThrowIfNull(icon, nameof(icon));

        DisplayName = displayName;
        Icon = icon;
        Index = index;
    }
}
