using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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

    [MaybeNull] [ObservableProperty] private ConvertDocumentViewModel _SelectedConvertDocument;

    #endregion

    #region Constructors

    public ConvertFilesPageViewModel(ConverterTypesService converterTypes, ConvertFilesManagerService filesManager,
        ISukiDialogManager dialogManager, ISukiToastManager toastManager, FilesDialogManagerService filesDialogManager)
        : base(dialogManager, toastManager, "Convert Files", Application.Current?.Resources["ConvertIcon"], 1)
    {
        _ConverterTypes = converterTypes;

        FilesManager = filesManager;

        _FilesDialogManager = filesDialogManager;

        // If there are no files, add an example file.
        if (FilesManager.Files.Count <= 0) FilesManager.Files = [ExampleConverterDocument()];

        SelectedConvertDocument = FilesManager.Files.First();
    }

    #endregion

    #region Services

    private readonly ConverterTypesService _ConverterTypes;

    private readonly FilesDialogManagerService _FilesDialogManager;

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
                    _ConverterTypes.InputTypes.Select(converter => converter.Name)
                ),
                OnOkClicked = OnInputFileTypeClicked
            })
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
                                _ConverterTypes.OutputTypes.Select(converter => converter.Name)
                            ),
                            OnOkClicked = fileType => OnOutputFileTypeClicked(fileType, currentDoc, pageIndex)
                        })
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

        if (currentDoc is { OutputConverter: not null } &&
            !string.IsNullOrEmpty(currentDoc.OutputFileText.Text))
        {
            // Show a dialog to save the file.
            var file = await _FilesDialogManager.SaveFileAsync(new FilePickerSaveOptions
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
                ShowOverwritePrompt = true,
                SuggestedFileName =
                    $"TableConverter-{DateTime.Now.ToFileTime()}"
            });

            if (file is null)
                return;

            if (file.IsSuccess)
            {
                currentDoc.IsBusy = true;

                await using (var stream = file.Value.Stream)
                {
                    await currentDoc.OutputConverter.OutputConverterHandler!.SaveFileAsync(stream,
                        Encoding.UTF8.GetBytes(currentDoc.OutputFileText.Text));
                }

                currentDoc.IsBusy = false;

                ToastManager.CreateToast()
                    .WithTitle("File Saved")
                    .WithContent(
                        $"The file '{currentDoc.Name}' has been saved to '{file.Value.Path.AbsolutePath}'.")
                    .OfType(NotificationType.Success)
                    .Dismiss().ByClicking()
                    .Dismiss().After(new TimeSpan(0, 0, 3))
                    .Queue();
            }
            else
            {
                DialogManager.CreateDialog()
                    .WithTitle("Error saving file")
                    .WithContent(file.Error!)
                    .OfType(NotificationType.Error)
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
    }

    [RelayCommand]
    private void RemoveFileButtonClicked(string id)
    {
        if (FilesManager.Files.Any(val => val.Id == id))
        {
            var file = FilesManager.Files.First(val => val.Id == id);

            DialogManager.CreateDialog()
                .WithTitle("Are you sure?")
                .WithContent($"Are you sure you want to remove the file '{id}'?")
                .OfType(NotificationType.Warning)
                .WithActionButton("No", _ => { }, true)
                .WithActionButton("Yes", _ =>
                {
                    if (FilesManager.Files.Count == 1) FilesManager.Files.Add(ExampleConverterDocument());

                    if (SelectedConvertDocument.Id == id)
                    {
                        FilesManager.Files.Remove(file);
                        SelectedConvertDocument = FilesManager.Files.First();
                    }
                    else
                    {
                        FilesManager.Files.Remove(file);
                    }

                    ToastManager.CreateToast()
                        .WithTitle("File Removed")
                        .WithContent($"The file '{id}' has been removed from your documents.")
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
                .WithContent($"The file '{id}' could not be found.")
                .OfType(NotificationType.Error)
                .Dismiss().ByClicking()
                .Dismiss().After(new TimeSpan(0, 0, 3))
                .Queue();
        }
    }

    [RelayCommand]
    private void DuplicateFileButtonClicked(string id)
    {
        if (FilesManager.Files.Any(val => val.Id == id))
        {
            var file = FilesManager.Files.First(val => val.Id == id);

            var newDoc = new ConvertDocumentViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Copy-{file.Name}",
                InputConverter = file.InputConverter,
                OutputConverter = file.OutputConverter,
                InputFileText = new TextDocument(new StringTextSource(file.InputFileText.Text))
                {
                    FileName = $"Copy-{file.InputFileText.FileName}",
                },
                EditHeaders = new ObservableCollection<string>(file.EditHeaders),
                EditRows = new ObservableCollection<string[]>(file.EditRows),
                OutputFileText = new TextDocument(new StringTextSource(file.OutputFileText.Text))
                {
                    FileName = $"Copy-{file.OutputFileText.FileName}",
                }
            };

            FilesManager.Files.Add(newDoc);

            SelectedConvertDocument = FilesManager.Files.Last();
        }
        else
        {
            DialogManager.CreateDialog()
                .WithTitle("File Not Found")
                .WithContent($"The file '{id}' could not be found.")
                .OfType(NotificationType.Error)
                .Dismiss().ByClickingBackground()
                .TryShow();
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
            doc.OutputFileText = new TextDocument(new StringTextSource(data.Value))
            {
                FileName = $"{doc.Name}{doc.OutputConverter.Extensions[0]}",
            };

            doc.IsBusy = false;

            doc.ProgressStepIndex = currentPageIndex;
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
        if (SelectedConvertDocument is not null)
        {
            var doc = new ConvertDocumentViewModel
            {
                InputConverter = _ConverterTypes.GetInputConverter(converterName)
            };

            var file = await _FilesDialogManager.OpenFileAsync(new FilePickerOpenOptions
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

            if (file is null)
                return;

            if (file.IsSuccess)
            {
                FilesManager.Files.Add(doc);

                var loadingDoc = FilesManager.Files.Last();
                SelectedConvertDocument = loadingDoc;

                if (loadingDoc.InputConverter is not null)
                {
                    loadingDoc.IsBusy = true;

                    loadingDoc.Name = file.Value.Name.Split('.')[0];
                    loadingDoc.Path = file.Value.Path.AbsolutePath;
                    loadingDoc.Id = Guid.NewGuid().ToString();

                    await using var stream = file.Value.Stream;

                    var data = await loadingDoc.InputConverter.InputConverterHandler!.ReadFileAsync(stream);

                    if (data.IsSuccess)
                    {
                        loadingDoc.InputFileText = new TextDocument(new StringTextSource(data.Value))
                        {
                            FileName = $"{loadingDoc.Name}{loadingDoc.InputConverter.Extensions[0]}"
                        };

                        loadingDoc.IsBusy = false;
                    }
                    else
                    {
                        FilesManager.Files.Remove(loadingDoc);

                        SelectedConvertDocument = FilesManager.Files.First();

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
            else
            {
                DialogManager.CreateDialog()
                    .WithTitle("Error opening file")
                    .WithContent(file.Error!)
                    .OfType(NotificationType.Error)
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
    }

    private async Task OnOutputFileTypeClicked(string converterName, ConvertDocumentViewModel doc, int currentPageIndex)
    {
        doc.OutputConverter = _ConverterTypes.GetOutputConverter(converterName);

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
        var converter = _ConverterTypes.InputTypes.First(converter => converter.Name == "CSV");

        return new ConvertDocumentViewModel
        {
            Name = name,
            InputConverter = converter,
            Id = Guid.NewGuid().ToString(),
            InputFileText = new TextDocument(new StringTextSource(
                "FIRST_NAME,LAST_NAME,GENDER,COUNTRY_CODE" +
                Environment.NewLine +
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
                "Olivia,Parry,F,GB" + Environment.NewLine))
            {
                FileName = $"{name}{converter.Extensions[0]}"
            }
        };
    }

    #endregion
}