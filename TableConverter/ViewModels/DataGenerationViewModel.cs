using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TableConverter.DataModels;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class DataGenerationViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    private ObservableCollection<DataGenerationField> _DataGenerationFields = new();

    [ObservableProperty]
    private int _NumberOfRows = 100;

    public Dictionary<DataGenerationType, Type> DataGenerationTypes { get; set; } = new();

    #endregion

    public DataGenerationViewModel()
    {
        DataGenerationFields.Add(new DataGenerationField());

        LoadDataGenerationTypes();
    }

    #region Commands

    [RelayCommand]
    private void GoBackButtonClicked()
    {
        PageRouterService.NavigateBack();
    }

    [RelayCommand]
    private void AddFieldButtonClicked(DataGenerationField field)
    {
        if (DataGenerationFields.Last() == field)
        {
            DataGenerationFields.Add(new DataGenerationField());
        }
        else
        {
            DataGenerationFields.Insert(DataGenerationFields.IndexOf(field) + 1, new DataGenerationField());
        }
    }

    [RelayCommand]
    private void RemoveFieldButtonClicked(DataGenerationField field)
    {
        if (DataGenerationFields.Count > 1)
        {
            DataGenerationFields.Remove(field);
        }
    }

    #endregion

    private async void LoadDataGenerationTypes()
    {
        DataGenerationTypes = await DataGenerationTypesService.GetDataGenerationTypesAsync();
    }
}
