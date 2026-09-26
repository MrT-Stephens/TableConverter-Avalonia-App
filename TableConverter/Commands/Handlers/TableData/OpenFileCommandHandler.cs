using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string OpenFile = "TableData.OpenFile";
}

public class OpenFileCommandHandler(
    IFilesDialogManager filesDialogManager,
    ISukiToastManager toastManager) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.OpenFile,
        "Open File",
        "Open an existing table data file.",
        "AddFile",
        "File",
        0,
        ["Ctrl+O"]);
    
    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.Parent is TableWorkspaceEditorViewModel;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (context.Parent is not TableWorkspaceEditorViewModel editorViewModel)
        {
            return;
        }

        var result = await filesDialogManager.OpenFileAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Table Store")
                {
                    Patterns = [$"*{TableStoreFile.Extension}"],
                },
                FilePickerFileTypes.All,
            ]
        });

        if (result is null)
        {
            return;
        }

        var numberOfAddedDocuments = 0;

        foreach (var file in result)
        {
            // The storage provider hands back a Uri, which SQLite would treat as a relative file name.
            // A local path is required to open the store.
            var path = file.TryGetLocalPath();

            if (string.IsNullOrEmpty(path))
            {
                toastManager.CreateSimpleInfoToast()
                    .OfType(NotificationType.Error)
                    .WithTitle("File Not On This Device")
                    .WithContent($"'{file.Name}' is not a file on this device, so it cannot be opened.")
                    .Queue();

                continue;
            }

            path = Path.GetFullPath(path);

            if (editorViewModel.Documents.Any(document =>
                    document is TableDataViewModel existing
                    && string.Equals(existing.Path, path, StringComparison.OrdinalIgnoreCase)))
            {
                toastManager.CreateSimpleInfoToast()
                    .OfType(NotificationType.Error)
                    .WithTitle("File Already Open")
                    .WithContent($"The file '{file.Name}' is already open.")
                    .Queue();
                    
                continue;
            }

            if (editorViewModel.CreateNewDocumentInstance() is not TableDataViewModel document)
            {
                throw new InvalidOperationException("Document should be of type TableDataViewModel");
            }

            try
            {
                // Opened stores stay owned by the user, so the document only ever reads from them.
                await document.OpenStoreAsync(path, Path.GetFileNameWithoutExtension(file.Name));
            }
            catch (Exception exception)
            {
                // One unreadable file should not stop the rest of the selection from opening.
                document.Dispose();

                toastManager.CreateSimpleInfoToast()
                    .OfType(NotificationType.Error)
                    .WithTitle("Could Not Open File")
                    .WithContent($"'{file.Name}' could not be opened. {exception.Message}")
                    .Queue();

                continue;
            }

            editorViewModel.AddDocument(document);
            editorViewModel.SelectedDocument = document;

            numberOfAddedDocuments++;
        }

        if (numberOfAddedDocuments == 0)
        {
            return;
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Added New File(s)")
            .WithContent($"Successfully added {numberOfAddedDocuments} file(s).")
            .Queue();
    }
}