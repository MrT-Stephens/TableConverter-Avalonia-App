using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Components.Xaml;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class DataGenerationPageViewModel : BasePageViewModel
{
    #region Constructors

    public DataGenerationPageViewModel(DataGenerationTypesService dataGenerationTypes, ConvertFilesManager filesManager,
        PageNavigationService pageNavigation, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        : base(dialogManager, toastManager, "Data Generation", Application.Current?.Resources["DataIcon"], 2)
    {
        _dataGenerationTypesService = dataGenerationTypes;

        _pageNavigation = pageNavigation;

        _filesManager = filesManager;

        DataGenerationFields.Add(new DataGenerationFieldViewModel());
        
        IsBusy = false;
    }

    #endregion

    #region Services

    private readonly DataGenerationTypesService _dataGenerationTypesService;
    private readonly PageNavigationService _pageNavigation;
    private readonly ConvertFilesManager _filesManager;

    #endregion

    #region Properties

    [ObservableProperty] private ObservableCollection<DataGenerationFieldViewModel> _DataGenerationFields = new();

    [ObservableProperty] private int _NumberOfRows = 1000;

    [ObservableProperty] private string _GeneratedDocumentName = string.Empty;
    
    [ObservableProperty] private bool _IsBusy;

    #endregion

    #region Commands

    [RelayCommand]
    private void ChooseTypeButtonClicked(DataGenerationFieldViewModel field)
    {
        DialogManager.CreateDialog()
            .WithViewModel(dialog => new DataGenerationTypesViewModel(dialog, _dataGenerationTypesService)
            {
                OnOkClicked = type =>
                {
                    field.SetDataGenerationMethod(type);

                    return Task.FromResult(Task.CompletedTask);
                }
            })
            .OfType(NotificationType.Information)
            .Dismiss()
            .ByClickingBackground()
            .TryShow();
    }

    [RelayCommand]
    private void AddFieldButtonClicked(DataGenerationFieldViewModel field)
    {
        if (DataGenerationFields.Last() == field)
            DataGenerationFields.Add(new DataGenerationFieldViewModel());
        else
            DataGenerationFields.Insert(DataGenerationFields.IndexOf(field) + 1, new DataGenerationFieldViewModel());
    }

    [RelayCommand]
    private void RemoveFieldButtonClicked(DataGenerationFieldViewModel field)
    {
        if (DataGenerationFields.Count > 1) DataGenerationFields.Remove(field);
    }

    [RelayCommand]
    private async Task GenerateDataButtonClicked()
    {
        IsBusy = true;
        
        var name = string.IsNullOrWhiteSpace(GeneratedDocumentName)
            ? $"Data-{DateTime.Now.ToFileTime()}"
            : GeneratedDocumentName;

        var data = await _dataGenerationTypesService.GenerateData(DataGenerationFields.ToArray(), NumberOfRows);
        
        _filesManager.Files.Add(new ConvertDocumentViewModel
        {
            Name = name,
            IsGenerated = true,
            ProgressStepIndex = 1,
            EditHeaders = new ObservableCollection<string>(data.Headers),
            EditRows = new ObservableCollection<string[]>(data.Rows)
        });
        
        IsBusy = false;
        
        _pageNavigation.RequestNavigation<ConvertFilesPageViewModel>(viewModel =>
        {
            if (viewModel is ConvertFilesPageViewModel view)
            {
                view.SelectedConvertDocument = _filesManager.Files.Last();
            }
        });
    }

    #endregion
}