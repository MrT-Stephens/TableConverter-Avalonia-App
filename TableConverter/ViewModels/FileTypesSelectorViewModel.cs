using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using System;

namespace TableConverter.ViewModels;

public partial class FileTypesSelectorViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    private string _Title = string.Empty;

    [ObservableProperty]
    private string[] _Values = [];

    [ObservableProperty]
    private string _SelectedValue = string.Empty;

    public Action<string>? OnOkClicked { get; set; } = null;

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
                    OnOkClicked?.Invoke(SelectedValue);
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
