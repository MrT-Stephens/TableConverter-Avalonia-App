using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        0,
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

        var path = file.Path.ToString();

        var tableData = await converterService.InputFileAsync(inputConverterName, path);

        var document = (editorViewModel.CreateNewDocumentInstance() as TableDataViewModel)!;

        document.Title = Path.GetFileNameWithoutExtension(path);
        document.Headers = tableData.Headers.ToObservableCollection();
        document.Rows = tableData.Rows.ToObservableCollection();
        
        editorViewModel.AddDocument(document);
        editorViewModel.SelectedDocument = document;

        toastManager.CreateSimpleInfoToast()
            .WithTitle("Success")
            .WithContent($"The file '{document.Title}' has been successfully imported.")
            .Queue();
    }
}