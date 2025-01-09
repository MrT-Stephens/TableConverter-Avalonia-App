using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using TableConverter.DataModels;

namespace TableConverter.Components.Xaml;

public partial class FileTypesSelectorViewModel(ISukiDialog dialog) : BaseDialogViewModel(dialog)
{
    #region Commands

    [RelayCommand]
    private async Task ButtonClicked(object? name)
    {
        if (name is string buttonName)
        {
            Close();

            switch (buttonName)
            {
                case "Ok":
                    if (OnOkClicked is not null) await OnOkClicked(SelectedValue);
                    break;
                case "Cancel":
                    break;
                default:
                    throw new NotImplementedException($"Button {buttonName} is not implemented");
            }
        }
    }

    #endregion

    #region Properties

    [ObservableProperty] private string _Title = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _Values = [];

    [ObservableProperty] private string _SelectedValue = string.Empty;

    public AsyncAction<string>? OnOkClicked { get; set; } = null;

    #endregion
}