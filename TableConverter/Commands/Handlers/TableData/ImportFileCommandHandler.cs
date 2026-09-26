using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string ImportFile = "TableData.ImportFile";
}

public class ImportFileCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    IFilesDialogManager filesDialogManager,
    IConverterService converterService) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.ImportFile,
        "Import File",
        "Import table data from any of the supported file types.",
        "ImportFile",
        "File",
        1,
        ["Ctrl+I"]);
    
    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.Parent is TableWorkspaceEditorViewModel;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (context.Parent is not TableWorkspaceEditorViewModel editorViewModel)
        {
            context.Cancel("Selected workspace is not correct.");
            return;
        }
        
        string? inputConverterName = null;
        
        var inputConverterSelectResult = await dialogManager.CreateDialog()
            .WithTitle("Select a File Type to Import")
            .WithSelection(
                converterService.InputNames,
                conv => inputConverterName = conv)
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Ok", "Cancel")
            .TryShowAsync();
        
        dialogManager.DismissDialog();

        if (!inputConverterSelectResult || string.IsNullOrEmpty(inputConverterName))
        {
            context.Cancel("File type was not selected.");
            return;
        }

        var options = converterService.GetInputOptionsByName<ConverterHandlerBaseOptions>(inputConverterName);

        if (options is not null)
        {
            var optionsResult = await dialogManager.CreateDialog()
                .WithTitle("Import Options")
                .WithForm(options)
                .WithYesNoResult("Ok", "Cancel")
                .TryShowAsync();

            if (!optionsResult)
            {
                context.Cancel();
                return;
            }
        }
        
        var metadata = converterService.GetInputMetadataByName(inputConverterName);
        
        var file = await filesDialogManager.OpenFileAsync(new FilePickerOpenOptions
        {
            Title = $"Import {metadata.Name} File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(metadata.Name)
                {
                    Patterns = metadata.Extensions.Select(ext => $"*{ext}").ToArray(),
                    MimeTypes = metadata.MimeTypes,
                    AppleUniformTypeIdentifiers = metadata.AppleUTIs,
                },
                FilePickerFileTypes.All,
            ],
        });

        if (file is null)
        {
            context.Cancel("File was not selected.");
            return;
        }

        var selectedFile = file.First();

        // The converters open the file by path, and the storage provider hands back a Uri that is not
        // a path SQLite or File.Open can use.
        var path = selectedFile.TryGetLocalPath();

        if (string.IsNullOrEmpty(path))
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Import Failed")
                .WithContent($"'{selectedFile.Name}' is not a file on this device, so it cannot be read.")
                .Queue();

            return;
        }

        var document = (TableDataViewModel)editorViewModel.CreateNewDocumentInstance();

        try
        {
            // The imported table lives in a store of its own, so the rest of the editor (search,
            // columns, editing) works on it exactly as it does on a document created with New File.
            await document.CreateNewStoreAsync(Path.GetFileNameWithoutExtension(selectedFile.Name));

            // The converter writes straight into the store, so a large file is never held in memory as
            // a whole table before it can be stored.
            await document.ImportDataAsync(converterService, inputConverterName, path);
        }
        catch (Exception exception)
        {
            // The document was never added to the workspace, so it has to be released explicitly.
            document.Dispose();

            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Import Failed")
                .WithContent($"'{selectedFile.Name}' could not be imported. {exception.Message}")
                .Queue();

            return;
        }

        editorViewModel.AddDocument(document);
        editorViewModel.SelectedDocument = document;

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Success")
            .WithContent($"The file '{document.Title}' has been successfully imported.")
            .Queue();
    }
}