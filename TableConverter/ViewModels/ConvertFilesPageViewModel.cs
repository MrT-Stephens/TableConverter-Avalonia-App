using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Components.Xaml;
using TableConverter.DataModels;
using TableConverter.Interfaces;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class ConvertFilesPageViewModel : BasePageViewModel
{
    #region Properties

    [ObservableProperty] private ConvertDocumentViewModel _SelectedConvertDocument;

    #endregion

    #region Constructors

    public ConvertFilesPageViewModel(ConverterTypesService converterTypes, ConvertFilesManagerService filesManager,
        ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        : base(dialogManager, toastManager, "Convert Files", Application.Current?.Resources["ConvertIcon"], 1)
    {
        ConverterTypes = converterTypes;

        FilesManager = filesManager;

        // If there are no files, add an example file.
        if (FilesManager.Files.Count <= 0) FilesManager.Files = [ExampleConverterDocument()];

        SelectedConvertDocument = FilesManager.Files.First();
    }

    #endregion

    #region Services

    private readonly ConverterTypesService ConverterTypes;
    public ConvertFilesManagerService FilesManager { get; }

    #endregion

    #region Commands

    [RelayCommand]
    private void ConvertFileNewFileButtonClicked()
    {
        // Show a dialog to select the input file type.
        DialogManager.CreateDialog()
            .WithViewModel(dialog => new FileTypesSelectorViewModel(dialog)
            {
                Title = "Please select a file type to input",
                Values = new ObservableCollection<string>(
                    ConverterTypes.InputTypes.Select(converter => converter.Name)
                ),
                OnOkClicked = OnInputFileTypeClicked
            })
            .OfType(NotificationType.Information)
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    [RelayCommand]
    private async Task ConvertFileNextBackButtonClicked(object? parameter)
    {
        var currentDoc = SelectedConvertDocument;

        if (currentDoc is not null && int.TryParse(parameter?.ToString(), out var pageIndex))
        {
            var count = currentDoc.ProgressStepValues.Count();

            if (pageIndex < 0 || pageIndex > count)
                throw new ArgumentOutOfRangeException($"Page index must be between 0 and {count}.");

            switch (pageIndex)
            {
                // If the page index is 1 and the current document index is less than 1.
                case 1 when currentDoc is { ProgressStepIndex: < 1, InputConverter: not null }:
                {
                    // Process the inputted text file to the tabular data.

                    // If the input converter has options, show a dialog to get the options.
                    if (currentDoc.InputConverter.InputConverterHandler!.Options is not null &&
                        currentDoc.InputConverter.InputConverterHandler is IInitializeControls controls)
                    {
                        controls.InitializeControls();

                        DialogManager.CreateDialog()
                            .WithViewModel(dialog => new ConvertFilesOptionsViewModel(dialog)
                            {
                                Title = $"How would you like your {currentDoc.InputConverter.Name} file inputted?",
                                Options = new ObservableCollection<Control>(controls.Controls),
                                OnOkClicked = async () => await ProcessInputtedFileToTableData(currentDoc, pageIndex)
                            })
                            .OfType(NotificationType.Information)
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                    }
                    // Otherwise, process the inputted file to tabular data.
                    else
                    {
                        await ProcessInputtedFileToTableData(currentDoc, pageIndex);
                    }

                    break;
                }
                // If the page index is 2 and the current document index is less than 2.
                case 2 when currentDoc.ProgressStepIndex < 2:
                {
                    // Process the tabular data to the outputted file type.
                    DialogManager.CreateDialog()
                        .WithViewModel(dialog => new FileTypesSelectorViewModel(dialog)
                        {
                            Title = "Please select a file type to output",
                            Values = new ObservableCollection<string>(
                                ConverterTypes.OutputTypes.Select(converter => converter.Name)
                            ),
                            OnOkClicked = fileType => OnOutputFileTypeClicked(fileType, currentDoc, pageIndex)
                        })
                        .OfType(NotificationType.Information)
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                    break;
                }
                default:
                {
                    currentDoc.ProgressStepIndex = pageIndex;
                    break;
                }
            }
        }
    }

    [RelayCommand]
    private async Task CopyFileButtonClicked()
    {
        var currentDoc = SelectedConvertDocument;
        var topLevel =
            TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
                .MainWindow);

        if (topLevel is not null && currentDoc is not null && !string.IsNullOrEmpty(currentDoc.OutputFileText.Text))
        {
            currentDoc.IsBusy = true;

            // Copy the file data to the clipboard.
            await topLevel.Clipboard!.SetTextAsync(currentDoc.OutputFileText.Text);

            currentDoc.IsBusy = false;

            // Show a success toast.
            ToastManager.CreateToast()
                .WithTitle("File Copied")
                .WithContent($"The file '{currentDoc.Name}' has been copied to clipboard.")
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(new TimeSpan(0, 0, 3))
                .Queue();
        }
    }

    [RelayCommand]
    private async Task SaveFileButtonClicked()
    {
        var currentDoc = SelectedConvertDocument;
        var topLevel =
            TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
                .MainWindow);

        if (topLevel is not null &&
            currentDoc is { InputConverter: not null, OutputConverter: not null } &&
            !string.IsNullOrEmpty(currentDoc.OutputFileText.Text))
        {
            // Show a dialog to save the file.
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Save {currentDoc.OutputConverter.Name} File",
                FileTypeChoices =
                [
                    new FilePickerFileType(currentDoc.OutputConverter.Name)
                    {
                        Patterns = currentDoc.OutputConverter.Extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = currentDoc.OutputConverter.MimeTypes,
                        AppleUniformTypeIdentifiers = currentDoc.OutputConverter.AppleUTIs
                    },
                    FilePickerFileTypes.All
                ],
                DefaultExtension = currentDoc.OutputConverter.Extensions[0],
                ShowOverwritePrompt = false,
                SuggestedFileName =
                    $"TableConverter-{currentDoc.InputConverter.Name}-{currentDoc.OutputConverter.Name}-{DateTime.Now.ToFileTime()}"
            });

            if (file is not null)
            {
                AsyncAction action = async () =>
                {
                    currentDoc.IsBusy = true;

                    await using (var stream = await file.OpenWriteAsync())
                    {
                        await currentDoc.OutputConverter.OutputConverterHandler!.SaveFileAsync(stream,
                            Encoding.UTF8.GetBytes(currentDoc.OutputFileText.Text));
                    }

                    currentDoc.IsBusy = false;

                    ToastManager.CreateToast()
                        .WithTitle("File Saved")
                        .WithContent($"The file '{currentDoc.Name}' has been saved to '{file.Path.AbsolutePath}'.")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(new TimeSpan(0, 0, 3))
                        .Queue();
                };

                if (File.Exists(file.Path.AbsolutePath))
                    DialogManager.CreateDialog()
                        .WithTitle("File already exists")
                        .WithContent(
                            $"The file '{file.Name}' already exists at that location. Would you like to replace it?")
                        .OfType(NotificationType.Warning)
                        .WithActionButton("No", _ => { }, true)
                        .WithActionButton("Yes", _ => action.Invoke(), true)
                        .TryShow();
                else
                    await action.Invoke();
            }
        }
    }

    [RelayCommand]
    private void RemoveFileButtonClicked(string name)
    {
        if (FilesManager.Files.Any(val => val.Name == name))
        {
            var file = FilesManager.Files.First(val => val.Name == name);

            DialogManager.CreateDialog()
                .WithTitle("Are you sure?")
                .WithContent($"Are you sure you want to remove the file '{name}'?")
                .OfType(NotificationType.Warning)
                .WithActionButton("No", _ => { }, true)
                .WithActionButton("Yes", _ =>
                {
                    if (FilesManager.Files.Count == 1) FilesManager.Files.Add(ExampleConverterDocument());

                    FilesManager.Files.Remove(file);

                    ToastManager.CreateToast()
                        .WithTitle("File Removed")
                        .WithContent($"The file '{name}' has been removed from your documents.")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(new TimeSpan(0, 0, 3))
                        .Queue();
                }, true)
                .TryShow();
        }
        else
        {
            ToastManager.CreateToast()
                .WithTitle("File Not Found")
                .WithContent($"The file '{name}' could not be found.")
                .OfType(NotificationType.Error)
                .Dismiss().ByClicking()
                .Dismiss().After(new TimeSpan(0, 0, 3))
                .Queue();
        }
    }

    [RelayCommand]
    private void DuplicateFileButtonClicked(string name)
    {
        if (FilesManager.Files.Any(val => val.Name == name))
        {
            var file = FilesManager.Files.First(val => val.Name == name);

            var newDoc = new ConvertDocumentViewModel
            {
                Name = $"Copy-{file.Name}",
                InputConverter = file.InputConverter,
                OutputConverter = file.OutputConverter,
                InputFileText = new TextDocument
                {
                    FileName = $"Copy-{file.InputFileText.FileName}",
                    Text = file.InputFileText.Text
                },
                EditHeaders = new ObservableCollection<string>(file.EditHeaders),
                EditRows = new ObservableCollection<string[]>(file.EditRows),
                OutputFileText = new TextDocument
                {
                    FileName = $"Copy-{file.OutputFileText.FileName}",
                    Text = file.OutputFileText.Text
                }
            };

            FilesManager.Files.Add(newDoc);
        }
        else
        {
            ToastManager.CreateToast()
                .WithTitle("File Not Found")
                .WithContent($"The file '{name}' could not be found.")
                .OfType(NotificationType.Error)
                .Dismiss().ByClicking()
                .Dismiss().After(new TimeSpan(0, 0, 3))
                .Queue();
        }
    }

    #endregion

    #region Misc Items

    private async Task ProcessInputtedFileToTableData(ConvertDocumentViewModel doc, int currentPageIndex)
    {
        doc.IsBusy = true;

        var data = await doc.InputConverter!.InputConverterHandler!.ReadTextAsync(
            doc.InputFileText.Text
        );

        if (data.IsSuccess)
        {
            // If the data is successful, set the headers and rows.
            doc.EditHeaders = new ObservableCollection<string>(data.Value.Headers);
            doc.EditRows = new ObservableCollection<string[]>(data.Value.Rows);

            doc.IsBusy = false;

            doc.ProgressStepIndex = currentPageIndex;

            // Show a success toast.
            ToastManager.CreateToast()
                .WithTitle("File Converted")
                .WithContent($"The file '{doc.Name}' has been converted to tabular data.")
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(new TimeSpan(0, 0, 3))
                .Queue();
        }
        else
        {
            // If the data is not successful, show an error dialog.
            DialogManager.CreateDialog()
                .WithTitle("Error converting file")
                .WithContent(data.Error!)
                .OfType(NotificationType.Error)
                .Dismiss().ByClickingBackground()
                .TryShow();

            doc.IsBusy = false;
        }
    }

    private async Task ProcessTableDataToOutputFile(ConvertDocumentViewModel doc, int currentPageIndex)
    {
        doc.IsBusy = true;

        var data = await doc.OutputConverter!.OutputConverterHandler!.ConvertAsync(
            doc.EditHeaders.ToArray(),
            doc.EditRows.ToArray()
        );

        if (data.IsSuccess)
        {
            doc.OutputFileText = new TextDocument
            {
                FileName = $"{doc.Name}{doc.OutputConverter.Extensions[0]}",
                Text = data.Value
            };

            doc.IsBusy = false;

            doc.ProgressStepIndex = currentPageIndex;

            ToastManager.CreateToast()
                .WithTitle("File Converted")
                .WithContent(
                    $"The file '{doc.Name}' has been converted to a '{doc.OutputConverter.Name}' file.")
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(new TimeSpan(0, 0, 3))
                .Queue();
        }
        else
        {
            doc.IsBusy = false;

            DialogManager.CreateDialog()
                .WithTitle("Error converting file")
                .WithContent(data.Error!)
                .OfType(NotificationType.Error)
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    private async Task OnInputFileTypeClicked(string converterName)
    {
        var topLevel =
            TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
                .MainWindow);

        if (topLevel is not null && SelectedConvertDocument is not null)
        {
            var doc = new ConvertDocumentViewModel
            {
                InputConverter = ConverterTypes.InputTypes.First(converter => converter.Name == converterName)
            };

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Open {doc.InputConverter.Name} File",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(doc.InputConverter.Name)
                    {
                        Patterns = doc.InputConverter.Extensions.Select(ext => $"*{ext}").ToArray(),
                        MimeTypes = doc.InputConverter.MimeTypes,
                        AppleUniformTypeIdentifiers = doc.InputConverter.AppleUTIs
                    },
                    FilePickerFileTypes.All
                ]
            });

            if (files.Count >= 1)
            {
                FilesManager.Files.Add(doc);

                var loadingDoc = FilesManager.Files.Last();
                SelectedConvertDocument = loadingDoc;

                if (loadingDoc.InputConverter is not null)
                {
                    loadingDoc.IsBusy = true;

                    loadingDoc.Name = files[0].Name.Split('.')[0];
                    loadingDoc.Path = files[0].Path.AbsolutePath;

                    await using var stream = await files[0].OpenReadAsync();

                    var data = await loadingDoc.InputConverter.InputConverterHandler!.ReadFileAsync(stream);

                    if (data.IsSuccess)
                    {
                        loadingDoc.InputFileText = new TextDocument
                        {
                            FileName = $"{loadingDoc.Name}{loadingDoc.InputConverter.Extensions[0]}",
                            Text = data.Value
                        };

                        loadingDoc.IsBusy = false;

                        ToastManager.CreateToast()
                            .WithTitle("File Added")
                            .WithContent($"The file '{files[0].Name}' has been added to your documents.")
                            .OfType(NotificationType.Success)
                            .Dismiss().ByClicking()
                            .Dismiss().After(new TimeSpan(0, 0, 3))
                            .Queue();
                    }
                    else
                    {
                        FilesManager.Files.Remove(loadingDoc);
                        
                        loadingDoc.IsBusy = false;

                        DialogManager.CreateDialog()
                            .WithTitle("Error reading file")
                            .WithContent(data.Error!)
                            .OfType(NotificationType.Error)
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                    }
                }
            }
        }
    }

    private async Task OnOutputFileTypeClicked(string converterName, ConvertDocumentViewModel doc, int currentPageIndex)
    {
        doc.OutputConverter =
            ConverterTypes.OutputTypes.First(converter => converter.Name == converterName);

        if (doc.OutputConverter is not null)
        {
            // If the output converter has options, show a dialog to get the options.
            if (doc.OutputConverter.OutputConverterHandler!.Options is not null &&
                doc.OutputConverter.OutputConverterHandler is IInitializeControls
                    controls)
            {
                controls.InitializeControls();

                DialogManager.CreateDialog()
                    .WithViewModel(modal => new ConvertFilesOptionsViewModel(modal)
                    {
                        Title = $"How would you like your {doc.OutputConverter.Name} file outputted?",
                        Options = new ObservableCollection<Control>(controls.Controls),
                        OnOkClicked = async () =>
                            await ProcessTableDataToOutputFile(doc, currentPageIndex)
                    })
                    .OfType(NotificationType.Information)
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            // Otherwise, process the tabular data to the outputted file type.
            else
            {
                await ProcessTableDataToOutputFile(doc, currentPageIndex);
            }
        }
    }

    private ConvertDocumentViewModel ExampleConverterDocument()
    {
        var name = $"Example-{DateTime.Now.ToFileTime()}";
        var converter = ConverterTypes.InputTypes.First(converter => converter.Name == "CSV");

        return new ConvertDocumentViewModel
        {
            Name = name,
            InputConverter = converter,
            InputFileText = new TextDocument
            {
                FileName = $"{name}{converter.Extensions[0]}",
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

    #endregion
}