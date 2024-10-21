using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;
using TableConverter.Interfaces;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Services;
using System.Linq;
using Avalonia.Controls.Notifications;
using System.IO;
using System.Text;
using TableConverter.Components.Xaml;

namespace TableConverter.ViewModels;

public partial class ConvertFilesPageViewModel : BasePageViewModel
{
    #region Services

    public readonly ConverterTypesService ConverterTypes;
    public ConvertFilesManager FilesManager { get; }

    #endregion

    #region Properties

    [ObservableProperty]
    private ConvertDocumentViewModel? _SelectedConvertDocument = null;

    #endregion

    #region Constructors

    public ConvertFilesPageViewModel(ConverterTypesService converterTypes, ConvertFilesManager filesManager, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        : base(dialogManager, toastManager, "Convert Files", Application.Current?.Resources["ConvertIcon"], 1)
    {
        ConverterTypes = converterTypes;

        FilesManager = filesManager;

        if (FilesManager.Files.Count <= 0)
        {
            FilesManager.Files = new()
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
        var dialog = DialogManager.CreateDialog()
            .WithViewModel((dialog) => new FileTypesSelectorViewModel(dialog)
            {
                Title = "Please select a file type to input",
                Values = new(ConverterTypes.InputTypes.Select(converter => converter.Name)),
                OnOkClicked = OnInputFileTypeClicked
            })
            .Dismiss().ByClickingBackground()
            .TryShow();
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

                        ToastManager.CreateToast()
                            .WithTitle("File Converted")
                            .WithContent($"The file '{currentDoc.Name}' has been converted to tabular data.")
                            .OfType(NotificationType.Success)
                            .Dismiss().ByClicking()
                            .Dismiss().After(new(0, 0, 3))
                            .Queue();
                    }
                    else
                    {
                        currentDoc.IsBusy = false;
                    }
                };

                if (currentDoc.InputConverter.InputConverterHandler!.Options is not null && currentDoc.InputConverter.InputConverterHandler is IInitializeControls controls)
                {
                    controls.InitializeControls();

                    DialogManager.CreateDialog()
                        .WithViewModel((dialog) => new ConvertFilesOptionsViewModel(dialog)
                        {
                            Title = $"How would you like your {currentDoc.InputConverter.Name} file inputted?",
                            Options = new(controls.Controls),
                            OnOkClicked = processDoc
                        })
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
                else
                {
                    processDoc.Invoke();
                }
            }
            else if (pageIndex == 2 && currentDoc.ProgressStepIndex < 2)
            {
                // Process the tabular data to the outputted file type.
                DialogManager.CreateDialog()
                    .WithViewModel((dialog) => new FileTypesSelectorViewModel(dialog)
                    {
                        Title = "Please select a file type to output",
                        Values = new(ConverterTypes.OutputTypes.Select(converter => converter.Name)),
                        OnOkClicked = (converterName) =>
                        {
                            currentDoc.OutputConverter = ConverterTypes.OutputTypes.First(converter => converter.Name == converterName);

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

                                        ToastManager.CreateToast()
                                            .WithTitle("File Converted")
                                            .WithContent($"The file '{currentDoc.Name}' has been converted to a '{currentDoc.OutputConverter.Name}' file.")
                                            .OfType(NotificationType.Success)
                                            .Dismiss().ByClicking()
                                            .Dismiss().After(new(0, 0, 3))
                                            .Queue();
                                    }
                                    else
                                    {
                                        currentDoc.IsBusy = false;
                                    }
                                };

                                if (currentDoc.OutputConverter.OutputConverterHandler!.Options is not null && currentDoc.OutputConverter.OutputConverterHandler is IInitializeControls controls)
                                {
                                    controls.InitializeControls();

                                    DialogManager.CreateDialog()
                                        .WithViewModel((dialog) => new ConvertFilesOptionsViewModel(dialog)
                                        {
                                            Title = $"How would you like your {currentDoc.OutputConverter.Name} file outputted?",
                                            Options = new(controls.Controls),
                                            OnOkClicked = processDoc
                                        })
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
                                }
                                else
                                {
                                    processDoc.Invoke();
                                }
                            }
                        }
                    })
                    .Dismiss().ByClickingBackground()
                    .TryShow();
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

            ToastManager.CreateToast()
                .WithTitle("File Copied")
                .WithContent($"The file '{currentDoc.Name}' has been copied to clipboard.")
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(new(0, 0, 3))
                .Queue();
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
                        MimeTypes = currentDoc.OutputConverter.MimeTypes,
                        AppleUniformTypeIdentifiers = currentDoc.OutputConverter.AppleUTIs
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

                    ToastManager.CreateToast()
                        .WithTitle("File Saved")
                        .WithContent($"The file '{currentDoc.Name}' has been saved to '{file.Path.AbsolutePath}'.")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(new(0, 0, 3))
                        .Queue();
                };

                if (File.Exists(file.Path.AbsolutePath))
                {
                    DialogManager.CreateDialog()
                        .WithTitle("File already exists")
                        .WithContent($"The file '{file.Name}' already exists at that location. Would you like to replace it?")
                        .OfType(NotificationType.Warning)
                        .WithActionButton("No", (dialog) => { }, true)
                        .WithActionButton("Yes", (dialog) => action.Invoke(), true)
                        .TryShow();
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
        if (FilesManager.Files!.Any(val => val.Name == name))
        {
            var file = FilesManager.Files.First(val => val.Name == name);

            DialogManager.CreateDialog()
                .WithTitle("Are you sure?")
                .WithContent($"Are you sure you want to remove the file '{name}'?")
                .OfType(NotificationType.Warning)
                .WithActionButton("No", (dialog) => { }, true)
                .WithActionButton("Yes", (dialog) => {
                    if (FilesManager.Files!.Count == 1)
                    {
                        FilesManager.Files.Add(ExampleConverterDocument());
                    }

                    FilesManager.Files.Remove(file);

                    ToastManager.CreateToast()
                        .WithTitle("File Removed")
                        .WithContent($"The file '{name}' has been removed from your documents.")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(new(0, 0, 3))
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
                .Dismiss().After(new(0, 0, 3))
                .Queue();
        }
    }

    [RelayCommand]
    private void DuplicateFileButtonClicked(string name)
    {
        if (FilesManager.Files!.Any(val => val.Name == name))
        {
            var file = FilesManager.Files.First(val => val.Name == name);

            var newDoc = new ConvertDocumentViewModel()
            {
                Name = $"Copy-{file.Name}",
                InputConverter = file.InputConverter,
                OutputConverter = file.OutputConverter,
                InputFileText = new AvaloniaEdit.Document.TextDocument()
                {
                    FileName = file.InputFileText.FileName,
                    Text = file.InputFileText.Text
                },
                EditHeaders = new(file.EditHeaders),
                EditRows = new(file.EditRows),
                OutputFileText = new AvaloniaEdit.Document.TextDocument()
                {
                    FileName = file.OutputFileText.FileName,
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
                .Dismiss().After(new(0, 0, 3))
                .Queue();
        }
    }

    #endregion

    #region Misc Items

    private ConvertDocumentViewModel ExampleConverterDocument()
    {
        var name = $"Example-{DateTime.Now.ToFileTime()}.csv";

        return new ConvertDocumentViewModel()
        {
            Name = name,
            InputConverter = ConverterTypes.InputTypes.First(converter => converter.Name == "CSV"),
            InputFileText = new AvaloniaEdit.Document.TextDocument()
            {
                FileName = name,
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
                InputConverter = ConverterTypes.InputTypes.First(converter => converter.Name == converterName),
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
                        MimeTypes = doc.InputConverter.MimeTypes,
                        AppleUniformTypeIdentifiers = doc.InputConverter.AppleUTIs
                    },
                    FilePickerFileTypes.All,
                ],
            });

            if (files.Count >= 1)
            {
                FilesManager.Files.Add(doc);

                var loadingDoc = FilesManager.Files.Last();
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

                    ToastManager.CreateToast()
                        .WithTitle("File Added")
                        .WithContent($"The file '{files[0].Name}' has been added to your documents.")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(new(0, 0, 3))
                        .Queue();
                }
            }
        }
    }

    #endregion
}
