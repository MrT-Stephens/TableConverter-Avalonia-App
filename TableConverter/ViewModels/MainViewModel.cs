﻿using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataModels;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.Views;

namespace TableConverter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Services

    private readonly DataGenerationTypesService DataGenerationTypesService;

    private readonly ConverterTypesService ConverterTypesService;

    #endregion

    #region Properties

    ///////////////////////////////////////////////////////////////////////////////////////
    ////                            Convert File Properties
    ///////////////////////////////////////////////////////////////////////////////////////
    
    [ObservableProperty]
    private ObservableCollection<ConverterType> _InputConverters = [];

    [ObservableProperty]
    private ObservableCollection<ConverterType> _OutputConverters = [];

    [ObservableProperty]
    private ObservableCollection<ConvertDocumentViewModel>? _ConvertDocuments = null;

    [ObservableProperty]
    private ConvertDocumentViewModel? _SelectedConvertDocument = null;

    ///////////////////////////////////////////////////////////////////////////////////////
    ////                            Data Generation Properties
    ///////////////////////////////////////////////////////////////////////////////////////
    
    [ObservableProperty]
    private ObservableCollection<DataGenerationFieldViewModel> _DataGenerationFields = new();

    [ObservableProperty]
    private int _NumberOfRows = 1000;

    [ObservableProperty]
    private string _GeneratedDocumentName = string.Empty;

    #endregion

    #region Constructors

    public MainViewModel()
    {
        DataGenerationTypesService = new();
        ConverterTypesService = new();

        LoadConverterTypes();

        if (ConvertDocuments is null)
        {
            ConvertDocuments = new()
            {
                ExampleConverterDocument()
            };
        }

        DataGenerationFields.Add(new());
    }

    public MainViewModel(DataGenerationTypesService dataGenerationTypesService, ConverterTypesService converterTypesService)
    {
        DataGenerationTypesService = dataGenerationTypesService;
        ConverterTypesService = converterTypesService;

        LoadConverterTypes();

        if (ConvertDocuments is null)
        {
            ConvertDocuments = new()
            {
                ExampleConverterDocument()
            };
        }

        DataGenerationFields.Add(new());
    }

    #endregion

    #region Commands 

    ///////////////////////////////////////////////////////////////////////////////////////
    ////                            Convert File Commands
    ///////////////////////////////////////////////////////////////////////////////////////

    [RelayCommand]
    private void ConvertFileNewFileButtonClicked()
    {
        SukiHost.ShowDialog(new FileTypesSelectorView(
            "Please select a file type to input",
            InputConverters.Select(converter => converter.Name).ToArray(),
            OnInputFileTypeClicked
        ), false, true);
    }

    [RelayCommand]
    private void ConvertFileNextBackButtonClicked(object? parameter)
    {
        var currentDoc = SelectedConvertDocument;

        if (currentDoc is not null && int.TryParse(parameter?.ToString(), out int pageIndex))
        {
            var count = currentDoc.ProgressStepValues.Count();

            if (pageIndex < 0 || pageIndex > count)
            {
                throw new ArgumentOutOfRangeException($"Page index must be between 0 and {count}.");
            }
            else if (pageIndex == 1 && 
                     currentDoc.ProgressStepIndex < 1 && 
                     currentDoc.InputConverter is not null)
            {
                // Process the inputted text file to the tabular data.
                Action processDoc = async () =>
                {
                    currentDoc.IsBusy = true;

                    var data = await currentDoc.InputConverter.InputConverterHandler!.ReadTextAsync(currentDoc.InputFileText.Text);

                    if (data != null)
                    {
                        currentDoc.EditHeaders = new(data.headers);
                        currentDoc.EditRows = new(data.rows);

                        currentDoc.IsBusy = false;

                        currentDoc.ProgressStepIndex = pageIndex;

                        await SukiHost.ShowToast(new ToastModel(
                            "File Converted",
                            $"The file '{currentDoc.Name}' has been converted to tabular data.",
                            SukiUI.Enums.NotificationType.Success)
                        );
                    }
                    else
                    {
                        currentDoc.IsBusy = false;
                    }
                };

                if (currentDoc.InputConverter.InputConverterHandler!.Options is not null && currentDoc.InputConverter.InputConverterHandler is IInitializeControls controls)
                {
                    controls.InitializeControls();

                    SukiHost.ShowDialog(new ConvertFilesOptionsView(
                        $"How would you like your {currentDoc.InputConverter.Name} file inputted?",
                        controls.Controls,
                        processDoc
                    ), false, true);
                }
                else
                {
                    processDoc.Invoke();
                }
            }
            else if (pageIndex == 2 && currentDoc.ProgressStepIndex < 2)
            {
                // Process the tabular data to the outputted file type.
                SukiHost.ShowDialog(new FileTypesSelectorView(
                    "Please select a file type to output",
                    OutputConverters.Select(converter => converter.Name).ToArray(),
                    (converterName) =>
                    {
                        currentDoc.OutputConverter = OutputConverters.First(converter => converter.Name == converterName);

                        if (currentDoc.OutputConverter is not null) 
                        {
                            Action processDoc = async () =>
                            {
                                currentDoc.IsBusy = true;

                                var data = await currentDoc.OutputConverter.OutputConverterHandler!.ConvertAsync(currentDoc.EditHeaders.ToArray(), currentDoc.EditRows.ToArray());

                                if (data != null)
                                {
                                    currentDoc.OutputFileText = new AvaloniaEdit.Document.TextDocument()
                                    { 
                                        FileName = currentDoc.Name,
                                        Text = data 
                                    };

                                    currentDoc.IsBusy = false;

                                    currentDoc.ProgressStepIndex = pageIndex;

                                    await SukiHost.ShowToast(new ToastModel(
                                        "File Converted",
                                        $"The file '{currentDoc.Name}' has been converted to a '{currentDoc.OutputConverter.Name}' file.",
                                        SukiUI.Enums.NotificationType.Success)
                                    );
                                }
                                else
                                {
                                    currentDoc.IsBusy = false;
                                }
                            };

                            if (currentDoc.OutputConverter.OutputConverterHandler!.Options is not null && currentDoc.OutputConverter.OutputConverterHandler is IInitializeControls controls)
                            {
                                controls.InitializeControls();

                                SukiHost.ShowDialog(new ConvertFilesOptionsView(
                                    $"How would you like your {currentDoc.OutputConverter.Name} file outputted?",
                                    controls.Controls,
                                    processDoc
                                ), false, true);
                            }
                            else
                            {
                                processDoc.Invoke();
                            }
                        }
                    }
                ), false, true);
            }
            else
            {
                currentDoc.ProgressStepIndex = pageIndex;
            }
        }
    }

    [RelayCommand]
    private async Task CopyFileButtonClicked()
    {
        var currentDoc = SelectedConvertDocument;
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)App.Current?.ApplicationLifetime!).MainWindow);

        if (topLevel is not null && currentDoc is not null && !string.IsNullOrEmpty(currentDoc.OutputFileText.Text))
        {
            currentDoc.IsBusy = true;

            await topLevel.Clipboard!.SetTextAsync(currentDoc.OutputFileText.Text);

            currentDoc.IsBusy = false;

            await SukiHost.ShowToast(new ToastModel(
                "File Copied",
                $"The file '{currentDoc.Name}' has been copied to clipboard.",
                SukiUI.Enums.NotificationType.Success)
            );
        }
    }

    [RelayCommand]
    private async Task SaveFileButtonClicked()
    {
        var currentDoc = SelectedConvertDocument;
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)App.Current?.ApplicationLifetime!).MainWindow);

        if (topLevel is not null &&
            currentDoc is not null &&
            currentDoc.InputConverter is not null &&
            currentDoc.OutputConverter is not null && 
            !string.IsNullOrEmpty(currentDoc.OutputFileText.Text))
        {
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Save {currentDoc.OutputConverter.Name} File",
                FileTypeChoices = [
                    new(currentDoc.OutputConverter.Name)
                    {
                        Patterns = currentDoc.OutputConverter.Extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = currentDoc.OutputConverter.MimeTypes
                    },
                    FilePickerFileTypes.All
                ],
                DefaultExtension = currentDoc.OutputConverter.Extensions[0],
                ShowOverwritePrompt = false,
                SuggestedFileName = $"TableConverter-{currentDoc.InputConverter.Name}-{currentDoc.OutputConverter.Name}-{DateTime.Now.ToFileTime()}"
            });

            if (file is not null)
            {
                Action action = async () =>
                {
                    currentDoc.IsBusy = true;

                    using (var stream = await file.OpenWriteAsync())
                    {
                        await currentDoc.OutputConverter.OutputConverterHandler!.SaveFileAsync(stream, Encoding.UTF8.GetBytes(currentDoc.OutputFileText.Text));
                    }

                    currentDoc.IsBusy = false;

                    await SukiHost.ShowToast(new ToastModel(
                        "File Saved",
                        $"The file '{currentDoc.Name}' has been saved to '{file.Path.AbsolutePath}'.",
                        SukiUI.Enums.NotificationType.Success)
                    );
                };

                if (File.Exists(file.Path.AbsolutePath))
                {
                    SukiHost.ShowMessageBox(new MessageBoxModel(
                        "File already exists",
                        $"The file '{file.Name}' already exists at that location. Would you like to replace it?",
                        SukiUI.Enums.NotificationType.Info,
                        "Yes", () =>
                        {
                            SukiHost.CloseDialog();

                            action.Invoke();
                        }), true
                    );
                }
                else
                {
                    action.Invoke();
                }
            }
        }
    }

    [RelayCommand]
    private void RemoveFileButtonClicked(string name)
    {
        if (ConvertDocuments!.Any(val => val.Name == name))
        {
            ConvertDocuments!.Remove(ConvertDocuments.First(val => val.Name == name));
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////
    ////                            Data Generation Commands
    ///////////////////////////////////////////////////////////////////////////////////////
    
    [RelayCommand]
    private void ChooseTypeButtonClicked(DataGenerationFieldViewModel field)
    {
        SukiHost.ShowDialog(new DataGenerationTypesView(DataGenerationTypesService.Types, 
            (type) => 
            {
                field.Type = type.Name;

                field.DataGenerationTypeHandler = DataGenerationTypesService.GetHandlerByName(type.Name);

                if (field.DataGenerationTypeHandler.Options is not null && field.DataGenerationTypeHandler is IInitializeControls controls)
                {
                    controls.InitializeControls();

                    field.OptionsControls = new(controls.Controls);
                }
                else
                {
                    field.OptionsControls = new()
                    {
                        new TextBlock()
                        {
                            Text = "No Options Available"
                        }
                    };
                }
            }
        ), false, true);
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
        DataGeneration.DataModels.TableData tableData = await DataGenerationTypesService.GenerateDataAsync(
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

            ConvertDocuments?.Add(newDoc);

            await SukiHost.ShowToast(new ToastModel(
                "Data Generated",
                $"The file '{newDoc.Name}' has been added to your documents.",
                SukiUI.Enums.NotificationType.Success)
            );
        }
    }

    #endregion

    #region Misc Functions

    ///////////////////////////////////////////////////////////////////////////////////////
    ////                            Convert File Misc Functions
    ///////////////////////////////////////////////////////////////////////////////////////
    
    private void LoadConverterTypes()
    {
        InputConverters = new ObservableCollection<ConverterType>(ConverterTypesService.Types.Where(c => c.InputConverterHandler is not null));
        OutputConverters = new ObservableCollection<ConverterType>(ConverterTypesService.Types.Where(c => c.OutputConverterHandler is not null));
    }

    private ConvertDocumentViewModel ExampleConverterDocument()
    {
        return new ConvertDocumentViewModel()
        {
            Name = "Example.csv",
            InputConverter = InputConverters.First(converter => converter.Name == "CSV"),
            InputFileText = new AvaloniaEdit.Document.TextDocument()
            {
                FileName = "Example.csv",
                Text =
                    "FIRST_NAME,LAST_NAME,GENDER,COUNTRY_CODE" + Environment.NewLine +
                    "Luxeena,Binoy,F,GB" + Environment.NewLine +
                    "Lisa,Allen,F,GB" + Environment.NewLine +
                    "Richard,Wood,M,GB" + Environment.NewLine +
                    "Luke,Murphy,M,GB" + Environment.NewLine +
                    "Adrian,Heacock,M,GB" + Environment.NewLine +
                    "Elvinas,Palubinskas,M,GB" + Environment.NewLine +
                    "Sian,Turner,F,GB" + Environment.NewLine +
                    "Potar,Potts,M,GB" + Environment.NewLine +
                    "Janis,Chrisp,F,GB" + Environment.NewLine +
                    "Sarah,Proffitt,F,GB" + Environment.NewLine +
                    "Calissa,Noonan,F,GB" + Environment.NewLine +
                    "Andrew,Connors,M,GB" + Environment.NewLine +
                    "Siann,Tynan,F,GB" + Environment.NewLine +
                    "Olivia,Parry,F,GB" + Environment.NewLine
            }
        };
    }

    private async void OnInputFileTypeClicked(string converterName)
    {
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)App.Current?.ApplicationLifetime!).MainWindow);

        if (topLevel is not null && SelectedConvertDocument is not null)
        {
            var doc = new ConvertDocumentViewModel()
            {
                InputConverter = InputConverters.First(converter => converter.Name == converterName),
            };

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Open {doc.InputConverter.Name} File",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new(doc.InputConverter.Name)
                    {
                        Patterns = doc.InputConverter.Extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = doc.InputConverter.MimeTypes
                    },
                    FilePickerFileTypes.All
                ]
            });

            if (files.Count >= 1 && ConvertDocuments is not null)
            {
                ConvertDocuments.Add(doc);

                var loadingDoc = ConvertDocuments.Last();
                SelectedConvertDocument = loadingDoc;

                if (loadingDoc.InputConverter is not null)
                {
                    loadingDoc.IsBusy = true;

                    loadingDoc.Name = files[0].Name;
                    loadingDoc.Path = files[0].Path.AbsolutePath;

                    using (var stream = await files[0].OpenReadAsync())
                    {
                        loadingDoc.InputFileText = new AvaloniaEdit.Document.TextDocument()
                        {
                            FileName = files[0].Name,
                            Text = await loadingDoc.InputConverter.InputConverterHandler!.ReadFileAsync(stream)
                        };
                    }

                    loadingDoc.IsBusy = false;

                    await SukiHost.ShowToast(new ToastModel(
                        "File Added",
                        $"The file '{files[0].Name}' has been added to your documents.",
                        SukiUI.Enums.NotificationType.Success)
                    );
                }
            }
        }
    }

    #endregion
}
