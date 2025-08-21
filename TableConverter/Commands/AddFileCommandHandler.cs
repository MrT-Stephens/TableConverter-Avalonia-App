using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using SukiUI.Dialogs;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Commands;

public class AddFileCommandHandler(IFilesDialogManager filesDialogManager, 
    ISukiDialogManager dialogManager, IConverterTypes converterTypes) : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata { get; } = new CommandMetadata("AddFile");
    
    public async Task Execute(object? parameter, ICommandContext context)
    {
        string? selectedItem = null;

        var dialog = dialogManager.CreateDialog()
            .WithTitle("Please select a file type to input")
            .WithSelection(converterTypes.InputTypes.Select(x => x.Name),
                item => selectedItem = item)
            .WithYesNoResult("Ok", "Cancel")
            .Dismiss().ByClickingBackground();

        if (await dialog.TryShowAsync() && selectedItem is not null)
        {
            var doc = new ConvertDocumentViewModel
            {
                InputConverter = converterTypes.GetInputConverter(selectedItem)
            };
            
            var file = await filesDialogManager.OpenFileAsync(new FilePickerOpenOptions
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
            
            
        }
    }

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return true;
    }
}