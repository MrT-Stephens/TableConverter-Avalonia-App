using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.ViewModels;

public partial class DataGenerationTypesViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty]
    private ObservableCollection<DataGenerationType> _GenerationTypes = new();

    [ObservableProperty]
    private ObservableCollection<DataGenerationType> _CurrentTypes = new();

    [ObservableProperty]
    private ObservableCollection<string> _Categories = new();

    [ObservableProperty]
    private DataGenerationType? _SelectedType = null;

    [ObservableProperty]
    private string _SelectedCategory = "All";

    public Action<DataGenerationType>? OnOkClicked { get; set; } = null;

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
                    OnOkClicked?.Invoke(SelectedType!);
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

    partial void OnGenerationTypesChanged(ObservableCollection<DataGenerationType> value)
    {
        CurrentTypes = value;

        Categories = new(value.Select(val => val.Category).Distinct().Order());

        Categories.Insert(0, "All");

        SelectedCategory = Categories[0];
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        CurrentTypes = value == "All" ? GenerationTypes : new(GenerationTypes.Where(type => type.Category == value));
    }

    #endregion
}
