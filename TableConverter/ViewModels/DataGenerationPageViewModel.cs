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

    public DataGenerationPageViewModel(DataGenerationTypesService dataGenerationTypes,
        ConvertFilesManagerService filesManager,
        PageNavigationService pageNavigation, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        : base(dialogManager, toastManager, "Data\nGeneration", Application.Current?.Resources["DataIcon"], 2)
    {
        _DataGenerationTypesService = dataGenerationTypes;

        _PageNavigation = pageNavigation;

        _FilesManager = filesManager;

        DataGenerationFields.Add(new DataGenerationFieldViewModel());

        IsLoading = false;

        AvailableLocales = new ObservableCollection<string>(_DataGenerationTypesService.AvailableLocales);

        SelectedLocale = "en";

        Seed = Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode() ^ Environment.TickCount.GetHashCode();
    }

    #endregion

    #region Services

    private readonly DataGenerationTypesService _DataGenerationTypesService;
    private readonly PageNavigationService _PageNavigation;
    private readonly ConvertFilesManagerService _FilesManager;

    #endregion

    #region Properties

    [ObservableProperty] private ObservableCollection<DataGenerationFieldViewModel> _DataGenerationFields = [];

    [ObservableProperty] private int _NumberOfRows = 1000;

    [ObservableProperty] private string _GeneratedDocumentName = string.Empty;

    [ObservableProperty] private bool _IsLoading;

    [ObservableProperty] private ObservableCollection<string> _AvailableLocales;

    [ObservableProperty] private string _SelectedLocale;

    [ObservableProperty] private int _Seed;

    public static int Int64MinValue => int.MinValue;

    public static int Int64MaxValue => int.MaxValue;

    #endregion

    #region Commands

    [RelayCommand]
    private void ChooseTypeButtonClicked(DataGenerationFieldViewModel field)
    {
        DialogManager.CreateDialog()
            .WithViewModel(dialog => new DataGenerationTypesViewModel(dialog, _DataGenerationTypesService)
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
        if (DataGenerationFields.Select((field, index) => (field, index))
                .Where(field => string.IsNullOrEmpty(field.field.Key)).ToArray() is { Length: > 0 } fields)
        {
            DialogManager.CreateDialog()
                .WithTitle("Something went wrong")
                .WithContent(
                    $"Some of the data generation fields could not be generated. Please try again.\n{string.Join("\n", fields.Select(field => $"At row {field.index} the field is null or empty."))}")
                .OfType(NotificationType.Error)
                .WithActionButton("Ok", _ => { }, true)
                .TryShow();
            
            return;
        }
        
        IsLoading = true;

        _DataGenerationTypesService.SetLocale(SelectedLocale);

        _DataGenerationTypesService.SetSeed(Seed);

        var name = string.IsNullOrWhiteSpace(GeneratedDocumentName)
            ? $"Generated-Data-{DateTime.Now.ToFileTime()}"
            : GeneratedDocumentName;

        var data = await Task.Run(() =>
            _DataGenerationTypesService.GenerateData(DataGenerationFields.ToArray(), NumberOfRows));

        _FilesManager.Files.Add(new ConvertDocumentViewModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            IsGenerated = true,
            ProgressStepIndex = 1,
            EditHeaders = new ObservableCollection<string>(data.Headers),
            EditRows = new ObservableCollection<string[]>(data.Rows)
        });

        IsLoading = false;

        _PageNavigation.RequestNavigation<ConvertFilesPageViewModel>(viewModel =>
        {
            if (viewModel is ConvertFilesPageViewModel view) view.SelectedConvertDocument = _FilesManager.Files.Last();
        });
    }

    #endregion
}