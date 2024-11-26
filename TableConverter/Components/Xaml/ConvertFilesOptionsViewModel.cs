using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Components.Xaml;

public partial class ConvertFilesOptionsViewModel(ISukiDialog dialog) : BaseDialogViewModel(dialog)
{
    #region Properties

    [ObservableProperty]
    private string _Title = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Control> _Options = new();

    public AsyncAction? OnOkClicked { get; set; }

    #endregion

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
                    if (OnOkClicked is not null)
                    {
                        await OnOkClicked();
                    }
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
