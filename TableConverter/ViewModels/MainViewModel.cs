﻿using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TableConverter.DataModels;
using TableConverter.Services;
using TableConverter.Views;

namespace TableConverter.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Properties

    // File converter view properties
    [ObservableProperty]
    private ObservableCollection<ConverterType> _InputConverters = [];

    [ObservableProperty]
    private ObservableCollection<ConverterType> _OutputConverters = [];

    [ObservableProperty]
    private ObservableCollection<ConvertDocumentViewModel>? _ConvertDocuments = null;

    [ObservableProperty]
    private ConvertDocumentViewModel? _SelectedConvertDocument = null;

    #endregion

    #region Constructor

    public MainViewModel()
    {
        LoadConverterTypes();

        if (ConvertDocuments is null)
        {
            ConvertDocuments = new()
            {
                ExampleConverterDocument()
            };
        }
    }

    #endregion

    #region Commands 

    [RelayCommand]
    private void ConvertFileNewFileButtonClicked()
    {
        SukiHost.ShowDialog(new TypesSelectorView(
            "Please select a file type to input",
            App.Current?.Resources["FileSearchIcon"] as StreamGeometry ?? throw new NullReferenceException(),
            InputConverters.Select(converter => converter.name).ToArray(),
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

                    var data = await currentDoc.InputConverter.inputConverter!.ReadTextAsync(currentDoc.InputFileText.Text);

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

                currentDoc.InputConverter.inputConverter!.InitializeControls();

                if (currentDoc.InputConverter.inputConverter!.Controls is not null)
                {
                    SukiHost.ShowDialog(new ConvertFilesOptionsView(
                        $"How would you like your {currentDoc.InputConverter.name} file inputted?",
                        App.Current?.Resources["OptionsIcon"] as StreamGeometry ?? throw new NullReferenceException(),
                        currentDoc.InputConverter.inputConverter!.Controls,
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
                SukiHost.ShowDialog(new TypesSelectorView(
                    "Please select a file type to output",
                    App.Current?.Resources["FileSearchIcon"] as StreamGeometry ?? throw new NullReferenceException(),
                    OutputConverters.Select(converter => converter.name).ToArray(),
                    (converterName) =>
                    {
                        currentDoc.OutputConverter = OutputConverters.First(converter => converter.name == converterName);

                        if (currentDoc.OutputConverter is not null) 
                        {
                            Action processDoc = async () =>
                            {
                                currentDoc.IsBusy = true;

                                var data = await currentDoc.OutputConverter.outputConverter!.ConvertAsync(currentDoc.EditHeaders.ToArray(), currentDoc.EditRows.ToArray());

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
                                        $"The file '{currentDoc.Name}' has been converted to a '{currentDoc.OutputConverter.name}' file.",
                                        SukiUI.Enums.NotificationType.Success)
                                    );
                                }
                                else
                                {
                                    currentDoc.IsBusy = false;
                                }
                            };

                            currentDoc.OutputConverter.outputConverter!.InitializeControls();

                            if (currentDoc.OutputConverter.outputConverter!.Controls is not null)
                            {
                                SukiHost.ShowDialog(new ConvertFilesOptionsView(
                                    $"How would you like your {currentDoc.OutputConverter.name} file outputted?",
                                    App.Current?.Resources["OptionsIcon"] as StreamGeometry ?? throw new NullReferenceException(),
                                    currentDoc.OutputConverter.outputConverter!.Controls,
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

    #endregion

    #region Misc Functions

    private async void LoadConverterTypes()
    {
        var converters = await ConverterTypesService.GetConverterTypesAsync();

        InputConverters = new ObservableCollection<ConverterType>(converters.Where(c => c.inputConverter is not null).ToArray());
        OutputConverters = new ObservableCollection<ConverterType>(converters.Where(c => c.outputConverter is not null).ToArray());
    }

    private ConvertDocumentViewModel ExampleConverterDocument()
    {
        return new ConvertDocumentViewModel()
        {
            Name = "Example.csv",
            InputConverter = InputConverters.First(converter => converter.name == "CSV"),
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
                InputConverter = InputConverters.First(converter => converter.name == converterName),
            };

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Open {doc.InputConverter.name} File",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new(doc.InputConverter.name)
                    {
                        Patterns = doc.InputConverter.extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = doc.InputConverter.mimeTypes
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

                    loadingDoc.InputFileText = new AvaloniaEdit.Document.TextDocument()
                    {
                        FileName = files[0].Name,
                        Text = await loadingDoc.InputConverter.inputConverter!.ReadFileAsync(files[0])
                    };

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
