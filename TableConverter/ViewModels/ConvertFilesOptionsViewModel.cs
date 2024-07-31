using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using System;
using System.Collections.ObjectModel;

namespace TableConverter.ViewModels;

public partial class ConvertFilesOptionsViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    private string _Title = string.Empty;

    [ObservableProperty]
    private StreamGeometry? _Icon = null;

    [ObservableProperty]
    private ObservableCollection<Control> _Options = new();

    public Action? OnContinueClicked { get; set; } = null;

    #endregion

    #region Commands

    [RelayCommand]
    private void ContinueClicked()
    {
        SukiHost.CloseDialog();

        if (OnContinueClicked is not null)
        {
            OnContinueClicked.Invoke();
        }
    }

    #endregion
}
