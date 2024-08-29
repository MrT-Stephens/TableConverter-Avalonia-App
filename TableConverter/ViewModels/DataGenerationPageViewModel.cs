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

namespace TableConverter.ViewModels;

public partial class DataGenerationPageViewModel : BasePageViewModel
{
    #region Services

    public readonly DataGenerationTypesService DataGenerationTypes;

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

    public DataGenerationPageViewModel(DataGenerationTypesService dataGenerationTypes, ISukiDialogManager dialogManager, ISukiToastManager toastManager) 
        : base(dialogManager, toastManager, "Data Generation", Application.Current?.Resources["DataIcon"], 2)
    {
        DataGenerationTypes = dataGenerationTypes;

        DataGenerationFields.Add(new DataGenerationFieldViewModel());
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ChooseTypeButtonClicked(DataGenerationFieldViewModel field)
    {
        //DialogManager.CreateDialog()
        //    .WithContent(
        //    new DataGenerationTypesView(DataGenerationTypes.Types,
        //        (type) =>
        //        {
        //            field.Type = type.Name;

        //            field.DataGenerationTypeHandler = DataGenerationTypes.GetHandlerByName(type.Name);

        //            if (field.DataGenerationTypeHandler.Options is not null && field.DataGenerationTypeHandler is IInitializeControls controls)
        //            {
        //                controls.InitializeControls();

        //                field.OptionsControls = new(controls.Controls);
        //            }
        //            else
        //            {
        //                field.OptionsControls = new()
        //                {
        //                    new TextBlock()
        //                    {
        //                        Text = "No Options Available"
        //                    }
        //                };
        //            }
        //        }
        //    ))
        //    .TryShow();
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
        TableData tableData = await DataGenerationTypes.GenerateDataAsync(
            DataGenerationFields.Select(val => new DataGenerationField(
                val.Name,
                val.Type,
                val.BlankPercentage,
                val.DataGenerationTypeHandler!
            )).ToArray(), NumberOfRows
        );

        if (tableData.headers.Count > 0 && tableData.rows.Count > 0)
        {
            var newDoc = new ConvertDocumentViewModel()
            {
                Name = string.IsNullOrEmpty(GeneratedDocumentName) ?
                           $"TableConverter-Generated-{DateTime.Now.ToFileTime()}" :
                           GeneratedDocumentName,
                EditHeaders = new(tableData.headers),
                EditRows = new(tableData.rows),
                IsGenerated = true,
                ProgressStepIndex = 1,
            };

            //ConvertDocuments?.Add(newDoc);

            ToastManager.CreateToast()
                .WithTitle("Data Generated")
                .WithContent($"The file '{newDoc.Name}' has been added to your documents.")
                .OfType(NotificationType.Success)
                .Queue();
        }
    }

    #endregion
}
