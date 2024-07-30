using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using System;

namespace TableConverter.ViewModels;

public partial class TypesSelectorViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    private string _Title = string.Empty;

    [ObservableProperty]
    private StreamGeometry? _Icon = null;

    [ObservableProperty]
    private string[] _Values = [];

    public Action<string>? OnItemClicked { get; set; } = null;

    #endregion

    #region Commands

    [RelayCommand]
    private void ItemClicked(object? item)
    {
        SukiHost.CloseDialog();

        if (OnItemClicked is not null && item is string strItem)
        {
            OnItemClicked.Invoke(strItem);
        }
    }

    #endregion
}
