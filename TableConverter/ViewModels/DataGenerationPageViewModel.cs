using Avalonia;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;
using TableConverter.DataGeneration.DataModels;
using TableConverter.Services;
using TableConverter.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Linq;
using Avalonia.Controls.Notifications;
using TableConverter.Interfaces;
using Avalonia.Controls;
using TableConverter.Components.Xaml;

namespace TableConverter.ViewModels;

public partial class DataGenerationPageViewModel : BasePageViewModel
{
    #region Services

    public readonly DataGenerationTypesService DataGenerationTypes;
    private readonly PageNavigationService PageNavigation;
    private readonly ConvertFilesManager FilesManager;

    #endregion

    #region Properties

    [ObservableProperty]
    private ObservableCollection<DataGenerationFieldViewModel> _DataGenerationFields = new();

    [ObservableProperty]
    private int _NumberOfRows = 1000;

    [ObservableProperty]
    private string _GeneratedDocumentName = string.Empty;

    #endregion

    #region Constructors

    public DataGenerationPageViewModel(DataGenerationTypesService dataGenerationTypes, ConvertFilesManager filesManager, PageNavigationService pageNavigation, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        : base(dialogManager, toastManager, "Data Generation", Application.Current?.Resources["DataIcon"], 2)
    {
        DataGenerationTypes = dataGenerationTypes;

        PageNavigation = pageNavigation;

        FilesManager = filesManager;

        DataGenerationFields.Add(new DataGenerationFieldViewModel());
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ChooseTypeButtonClicked(DataGenerationFieldViewModel field)
    {
        
    }

    [RelayCommand]
    private void AddFieldButtonClicked(DataGenerationFieldViewModel field)
    {
        if (DataGenerationFields.Last() == field)
        {
            DataGenerationFields.Add(new DataGenerationFieldViewModel());
        }
        else
        {
            DataGenerationFields.Insert(DataGenerationFields.IndexOf(field) + 1, new DataGenerationFieldViewModel());
        }
    }

    [RelayCommand]
    private void RemoveFieldButtonClicked(DataGenerationFieldViewModel field)
    {
        if (DataGenerationFields.Count > 1)
        {
            DataGenerationFields.Remove(field);
        }
    }

    [RelayCommand]
    private async Task GenerateDataButtonClicked()
    {

        
    }

    #endregion
}
