using System;
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
    public const string ExportFile = "TableData.ExportFile";
}

public class ExportFileCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    IFilesDialogManager filesDialogManager,
    IConverterService converterService)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.ExportFile,
        "Export File",
        "Export the selected table data to any of the supported file types.",
        "ExportFile",
        "File",
        2,
        ["Ctrl+E"],
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.Parent is TableWorkspaceEditorViewModel;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (context.Parent is not TableWorkspaceEditorViewModel)
        {
            context.Cancel("Selected workspace is not correct.");
            return;
        }

        if (!context.TryGetSelectedItem<TableDataViewModel>(out var document))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        string? outputConverterName = null;

        var outputConverterSelectResult = await dialogManager.CreateDialog()
            .WithTitle("Select a File Type to Export")
            .WithSelection(
                converterService.OutputNames,
                conv => outputConverterName = conv)
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Ok", "Cancel")
            .TryShowAsync();

        dialogManager.DismissDialog();

        if (!outputConverterSelectResult || string.IsNullOrEmpty(outputConverterName))
        {
            context.Cancel("File type was not selected.");
            return;
        }

        // The name is assigned from inside the dialog callback, so the flow analysis cannot tell that
        // it is known to be set here.
        var outputName = outputConverterName!;

        var options = converterService.GetOutputOptionsByName<ConverterHandlerBaseOptions>(outputName);

        if (options is not null)
        {
            var optionsResult = await dialogManager.CreateDialog()
                .WithTitle("Export Options")
                .WithForm(options)
                .WithYesNoResult("Ok", "Cancel")
                .TryShowAsync();

            if (!optionsResult)
            {
                context.Cancel();
                return;
            }
        }

        var metadata = converterService.GetOutputMetadataByName(outputName);

        var file = await filesDialogManager.SaveFileAsync(new FilePickerSaveOptions
        {
            Title = $"Export {metadata.Name} File",
            SuggestedFileName = document.Title,
            DefaultExtension = metadata.Extensions.FirstOrDefault()?.TrimStart('.'),
            ShowOverwritePrompt = true,
            FileTypeChoices =
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

        // The converters open the file by path, and the storage provider hands back a Uri that is not
        // a path File.Open can use.
        var path = file.TryGetLocalPath();

        if (string.IsNullOrEmpty(path))
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Export Failed")
                .WithContent($"'{file.Name}' is not a file on this device, so it cannot be written.")
                .Queue();

            return;
        }

        try
        {
            // Reading the store and converting the table is not rendering work, so it all stays off
            // the UI thread. The converter reads straight from the store, so a large table is never held
            // in memory as a whole before it can be written out.
            await Task.Run(async () =>
            {
                await document.ExportDataAsync(converterService, outputName, path);
            });
        }
        catch (Exception exception)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Export Failed")
                .WithContent($"'{document.Title}' could not be exported. {exception.Message}")
                .Queue();

            return;
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Success")
            .WithContent($"'{document.Title}' has been successfully exported.")
            .Queue();
    }
}
