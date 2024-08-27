using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using System;
using System.Collections.ObjectModel;

namespace TableConverter.Components.Xaml;

public partial class ConvertFilesOptionsViewModel(ISukiDialog dialog) : BaseDialogViewModel(dialog)
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
        if (name is string buttonName)
        {
            switch (buttonName)
            {
                case "Ok":
                    Close();
                    OnOkClicked?.Invoke();
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
