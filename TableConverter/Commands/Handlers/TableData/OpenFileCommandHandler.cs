using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
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
                new FilePickerFileType(".tcstore")
            ]
        });

        if (result is null)
        {
            return;
        }

        var numberOfAddedDocuments = 0;
            
        foreach (var file in result)
        {
            if (editorViewModel.Documents.Any(d => d is TableDataViewModel doc && doc.Path == file.Path.ToString()))
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

            document.Title = Path.GetFileNameWithoutExtension(file.Name);
            document.Path = file.Path.ToString();
            document.DataSource.Path = file.Path.ToString();
                
            editorViewModel.Documents.Add(document);
            editorViewModel.SelectedDocument = document;
            document.InvalidateData();   
                
            numberOfAddedDocuments++;
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Added New File(s)")
            .WithContent("Successfully added " + numberOfAddedDocuments + " file(s).")
            .Queue();
    }
}