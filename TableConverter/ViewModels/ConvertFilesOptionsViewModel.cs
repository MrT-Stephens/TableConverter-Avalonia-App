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
    private ObservableCollection<Control> _Options = new();

    public Action? OnOkClicked { get; set; } = null;

    #endregion

    #region Commands

    [RelayCommand]
    private void ButtonClicked(object? name)
    {
        SukiHost.CloseDialog();

        if (name is string buttonName)
        {
            switch (buttonName)
            {
                case "Ok":
                    OnOkClicked?.Invoke();
                    break;
                case "Cancel":
                    break;
                default:
                    throw new NotImplementedException($"Button {buttonName} is not implemented");
            }
        }
    }

    #endregion
}
