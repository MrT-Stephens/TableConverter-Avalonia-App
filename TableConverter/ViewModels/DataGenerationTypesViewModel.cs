using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TableConverter.ViewModels;

public partial class DataGenerationTypesViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    private ObservableCollection<KeyValuePair<string, string>> _GenerationTypes = new();

    [ObservableProperty]
    private ObservableCollection<string> _CurrentTypes = new();

    [ObservableProperty]
    private ObservableCollection<string> _Categories = new();

    [ObservableProperty]
    private string _SelectedType = string.Empty;

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
                    OnOkClicked?.Invoke(SelectedType);
                    break;
                case "Cancel":
                    break;
                default:
                    throw new NotImplementedException($"Button {buttonName} is not implemented");
            }
        }
    }

    #endregion

    #region Misc Functions

    partial void OnGenerationTypesChanged(ObservableCollection<KeyValuePair<string, string>> value)
    {
        CurrentTypes = new(value.Select(val => val.Key));

        Categories = new(value.Select(val => val.Value).Distinct());
    }

    #endregion
}
