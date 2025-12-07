using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using SukiUI.Dialogs;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string ImportFile = "TableData.ImportFile";
}

public class ImportFileCommandHandler(
    ISukiDialogManager dialogManager,
    IConverterTypes converterTypes,
    IFilesDialogManager filesDialogManager) : ICommandHandlerAsync
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
        }
        
        string? inputConverterName = null;
        
        var inputConverterSelectResult = await dialogManager.CreateDialog()
            .WithTitle("Select a File Type to Import")
            .WithSelection(
                converterTypes.InputTypes.Select(x => x.Name),
                conv => inputConverterName = conv)
            .Dismiss().ByClickingBackground()
            .WithOkResult("Ok")
            .TryShowAsync();
        
        if (!inputConverterSelectResult || string.IsNullOrEmpty(inputConverterName))
            return;

        var inputConverterData = converterTypes.GetInputConverter(inputConverterName);

        var file = await filesDialogManager.OpenFileAsync(new FilePickerOpenOptions
        {
            Title = $"Import {inputConverterData.Name} File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(inputConverterData.Name)
                {
                    Patterns = inputConverterData.Extensions.Select(ext => $"*{ext}").ToArray(),
                    MimeTypes = inputConverterData.MimeTypes,
                    AppleUniformTypeIdentifiers = inputConverterData.AppleUTIs,
                },
                FilePickerFileTypes.All,
            ],
        });
        
        if (file is null) return;

        var inputConverterHandler = inputConverterData.InputConverterHandler;

        if (inputConverterHandler!.Options is not null 
            && inputConverterHandler is IInitializeControls controls)
        {
            controls.InitializeControls();

            var result = await dialogManager.CreateDialog()
                .WithTitle($"How would you like your {inputConverterName} file imported?")
                .WithContentList(controls.Controls)
                .Dismiss().ByClickingBackground()
                .WithOkResult("Ok")
                .TryShowAsync();
            
            if (!result) return;
        }
    }
}