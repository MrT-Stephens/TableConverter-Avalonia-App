using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using System;

namespace TableConverter.Components.Xaml;

public partial class FileTypesSelectorViewModel(ISukiDialog dialog) : BaseDialogViewModel(dialog)
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
        if (name is string buttonName)
        {
            switch (buttonName)
            {
                case "Ok":
                    Close();
                    OnOkClicked?.Invoke(SelectedValue);
                    break;
                case "Cancel":
                    Close();
                    break;
                default:
                    throw new NotImplementedException($"Button {buttonName} is not implemented");
            }
        }
    }

    #endregion
}
