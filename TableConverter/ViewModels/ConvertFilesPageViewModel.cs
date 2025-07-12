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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        ShowFileTypeSelectorDialog(
            "Please select a file type to input",
            _ConverterTypes.InputTypes.Select(converter => converter.Name),
            OnInputFileTypeClicked
        );
    }

    [RelayCommand]
    private async Task ConvertFileNextBackButtonClicked(object? parameter)
    {
        var currentDoc = SelectedConvertDocument;

        if (currentDoc is null || !int.TryParse(parameter?.ToString(), out var pageIndex))
            return;

        var count = currentDoc.ProgressStepValues.Count();

        if (pageIndex < 0 || pageIndex > count)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                ShowErrorDialog(
                    "Invalid Page", 
                    $"Page index must be between 0 and {count}."));

            return;
        }

        switch (pageIndex)
        {
            case 1 when currentDoc is { ProgressStepIndex: < 1, InputConverter: not null }:
                {
                    if (currentDoc.InputConverter.InputConverterHandler?.Options is not null &&
                        currentDoc.InputConverter.InputConverterHandler is IInitializeControls controls)
                    {
                        controls.InitializeControls();
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            ShowOptionsDialog(
                                $"How would you like your {currentDoc.InputConverter.Name} file inputted?",
                                controls.Controls,
                                async () => await ProcessInputtedFileToTableData(currentDoc, pageIndex)
                            ));
                    }
                    else
                    {
                        await ProcessInputtedFileToTableData(currentDoc, pageIndex);
                    }

                    break;
                }

            case 2 when currentDoc.ProgressStepIndex < 2:
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        ShowFileTypeSelectorDialog(
                            "Please select a file type to output",
                            _ConverterTypes.OutputTypes.Select(converter => converter.Name),
                            fileType => OnOutputFileTypeClicked(fileType, currentDoc, pageIndex)
                        ));

                    break;
                }

            default:
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        currentDoc.ProgressStepIndex = pageIndex);

                    break;
                }
        }
    }

    [RelayCommand]
    private async Task CopyFileButtonClicked()
    {
        var currentDoc = SelectedConvertDocument;

        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!).MainWindow);

        if (topLevel is null || currentDoc is null || string.IsNullOrEmpty(currentDoc.OutputFileText.Text))
            return;

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => currentDoc.IsBusy = true);

        try
        {
            await topLevel.Clipboard!.SetTextAsync(currentDoc.OutputFileText.Text);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                ShowToast(
                    "File Copied", 
                    $"The file '{currentDoc.Name}' has been copied to clipboard.", 
                    NotificationType.Success));
        }
        finally
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => currentDoc.IsBusy = false);
        }
    }

    [RelayCommand]
    private async Task SaveFileButtonClicked()
    {
        var currentDoc = SelectedConvertDocument;

        if (currentDoc is not { OutputConverter: not null } || string.IsNullOrEmpty(currentDoc.OutputFileText.Text))
            return;

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
            SuggestedFileName = $"TableConverter-{DateTime.Now.ToFileTime()}"
        });

        if (file is null)
            return;

        if (file.IsSuccess)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => currentDoc.IsBusy = true);

            try
            {
                await using (var stream = file.Value.Stream)
                {
                    await currentDoc.OutputConverter.OutputConverterHandler!.SaveFileAsync(
                        stream, Encoding.UTF8.GetBytes(currentDoc.OutputFileText.Text));
                }

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    ShowToast(
                        "File Saved", 
                        $"The file '{currentDoc.Name}' has been saved to '{file.Value.Path.AbsolutePath}.", 
                        NotificationType.Success));
            }
            finally
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => currentDoc.IsBusy = false);
            }
        }
        else
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ShowErrorDialog("Error saving file", file.Error));
        }
    }

    [RelayCommand]
    private void RemoveFileButtonClicked(string id)
    {
        var file = FilesManager.Files.FirstOrDefault(val => val.Id == id);

        if (file is null)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                ShowToast(
                    "File Not Found", 
                    $"The file '{id}' could not be found.", 
                    NotificationType.Error));

            return;
        }

        DialogManager.CreateDialog()
            .WithTitle("Are you sure?")
            .WithContent($"Are you sure you want to remove the file '{id}'?")
            .OfType(NotificationType.Warning)
            .WithActionButton("No", _ => { }, true)
            .WithActionButton("Yes", _ =>
            {
                if (FilesManager.Files.Count == 1)
                    FilesManager.Files.Add(ExampleConverterDocument());

                FilesManager.Files.Remove(file);

                if (SelectedConvertDocument?.Id == id)
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => SelectedConvertDocument = FilesManager.Files.FirstOrDefault()!);

                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    ShowToast(
                        "File Removed", 
                        $"The file '{id}' has been removed from your documents.", 
                        NotificationType.Success));
            }, true)
            .TryShow();
    }

    [RelayCommand]
    private void DuplicateFileButtonClicked(string id)
    {
        var file = FilesManager.Files.FirstOrDefault(val => val.Id == id);

        if (file is null)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                ShowErrorDialog("File Not Found", $"The file '{id}' could not be found. Please ensure the file exists in the list."));

            return;
        }

        var newDoc = new ConvertDocumentViewModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"Copy-{file.Name}",
            InputConverter = file.InputConverter,
            OutputConverter = file.OutputConverter,
            InputFileText = new TextDocument(file.InputFileText.Text)
            {
                FileName = $"Copy-{file.InputFileText.FileName}",
            },
            EditHeaders = new ObservableCollection<string>(file.EditHeaders),
            EditRows = new ObservableCollection<string[]>(file.EditRows),
            OutputFileText = new TextDocument(file.OutputFileText.Text)
            {
                FileName = $"Copy-{file.OutputFileText.FileName}",
            }
        };

        FilesManager.Files.Add(newDoc);

        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => SelectedConvertDocument = newDoc);
    }

    #endregion

    #region Misc Items

    private async Task ProcessInputtedFileToTableData(ConvertDocumentViewModel doc, int currentPageIndex)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => doc.IsBusy = true);

        try
        {
            var result = await doc.InputConverter?.InputConverterHandler?.ReadTextAsync(doc.InputFileText.Text)!;
            if (result.IsSuccess)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    doc.EditHeaders = new ObservableCollection<string>(result.Value.Headers);
                    doc.EditRows = new ObservableCollection<string[]>(result.Value.Rows);
                    doc.ProgressStepIndex = currentPageIndex;
                });
            }
            else
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ShowErrorDialog("Error converting file", result.Error));
            }
        }
        finally
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => doc.IsBusy = false);
        }
    }

    private async Task ProcessTableDataToOutputFile(ConvertDocumentViewModel doc, int currentPageIndex)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => doc.IsBusy = true);
        try
        {
            var result = await doc.OutputConverter?.OutputConverterHandler?.ConvertAsync(
                doc.EditHeaders.ToArray(),
                doc.EditRows.ToArray()
            )!;

            if (result.IsSuccess)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    doc.OutputFileText = new TextDocument(result.Value)
                    {
                        FileName = $"{doc.Name}{doc.OutputConverter.Extensions[0]}"
                    };
                    doc.ProgressStepIndex = currentPageIndex;
                });
            }
            else
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ShowErrorDialog("Error converting file", result.Error));
            }
        }
        finally
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => doc.IsBusy = false);
        }
    }

    private async Task OnInputFileTypeClicked(string converterName)
    {
        if (SelectedConvertDocument is null)
            return;

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
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => SelectedConvertDocument = loadingDoc);

            if (loadingDoc.InputConverter is not null)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => loadingDoc.IsBusy = true);
                try
                {
                    loadingDoc.Name = Path.GetFileNameWithoutExtension(file.Value.Name);
                    loadingDoc.Path = file.Value.Path.AbsolutePath;
                    loadingDoc.Id = Guid.NewGuid().ToString();

                    await using var stream = file.Value.Stream;
                    var result = await loadingDoc.InputConverter.InputConverterHandler!.ReadFileAsync(stream);

                    if (result.IsSuccess)
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            loadingDoc.InputFileText = new TextDocument(result.Value)
                            {
                                FileName = $"{loadingDoc.Name}{loadingDoc.InputConverter.Extensions[0]}"
                            };
                        });
                    }
                    else
                    {
                        FilesManager.Files.Remove(loadingDoc);
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => SelectedConvertDocument = FilesManager.Files.FirstOrDefault()!);
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ShowErrorDialog("Error reading file", result.Error));
                    }
                }
                finally
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => loadingDoc.IsBusy = false);
                }
            }
        }
        else
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ShowErrorDialog("Error opening file", file.Error));
        }
    }

    private async Task OnOutputFileTypeClicked(string converterName, ConvertDocumentViewModel doc, int currentPageIndex)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => doc.OutputConverter = _ConverterTypes.GetOutputConverter(converterName));

        if (doc.OutputConverter?.OutputConverterHandler?.Options is not null &&
            doc.OutputConverter.OutputConverterHandler is IInitializeControls controls)
        {
            controls.InitializeControls();

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                DialogManager.CreateDialog()
                    .WithViewModel(modal => new ConvertFilesOptionsViewModel(modal)
                    {
                        Title = $"How would you like your {doc.OutputConverter.Name} file outputted?",
                        Options = new ObservableCollection<Control>(controls.Controls),
                        OnOkClicked = async () => await ProcessTableDataToOutputFile(doc, currentPageIndex)
                    })
                    .Dismiss().ByClickingBackground()
                    .TryShow());
        }
        else
        {
            await ProcessTableDataToOutputFile(doc, currentPageIndex);
        }
    }

    #endregion

    #region Helper Methods

    private void ShowErrorDialog(string title, string? content)
    {
        DialogManager.CreateDialog()
            .WithTitle(title)
            .WithContent(content ?? "An unknown error occurred.")
            .OfType(NotificationType.Error)
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private void ShowFileTypeSelectorDialog(string title, IEnumerable<string> values, AsyncAction<string> onOk)
    {
        DialogManager.CreateDialog()
            .WithViewModel(dialog => new FileTypesSelectorViewModel(dialog)
            {
                Title = title,
                Values = new ObservableCollection<string>(values),
                OnOkClicked = onOk
            })
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private void ShowOptionsDialog(string title, IEnumerable<Control> options, AsyncAction onOk)
    {
        DialogManager.CreateDialog()
            .WithViewModel(dialog => new ConvertFilesOptionsViewModel(dialog)
            {
                Title = title,
                Options = new ObservableCollection<Control>(options),
                OnOkClicked = onOk
            })
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private void ShowToast(string title, string content, NotificationType type)
    {
        ToastManager.CreateToast()
            .WithTitle(title)
            .WithContent(content)
            .OfType(type)
            .Dismiss().ByClicking()
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Queue();
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
            InputFileText = new TextDocument(
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
                "Olivia,Parry,F,GB" + Environment.NewLine)
            {
                FileName = $"{name}{converter.Extensions[0]}"
            }
        };
    }

    #endregion
}