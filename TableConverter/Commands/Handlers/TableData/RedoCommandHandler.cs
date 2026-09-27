using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database.History;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string Redo = "TableData.Redo";
}

/// <summary>
/// Puts in place again the change that was last taken back.
/// </summary>
/// <remarks>
/// Redo only ever moves towards the newest step that was undone, so a change made after an undo is what
/// stops a step being put back - the history drops the steps that were undone as soon as a new one is
/// recorded, rather than leaving them to be applied to a table that has gone a different way.
/// </remarks>
public class RedoCommandHandler(
    ISukiToastManager toastManager,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.Redo,
        "Redo",
        "Put back the change that was last taken back.",
        "RedoIcon",
        "Edit",
        0,
        ["Ctrl+Y"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetTableDocument(out _);
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetTableDocument(out var document))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        var redone = await history.RedoAsync(document.Path);

        if (redone is null)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Redo")
                .WithContent("There is no change left to put back.")
                .Queue();

            return;
        }

        // A step can have changed anything about the table - its columns, its rows, their order, or the
        // table as a whole - so the grid is rebuilt rather than merely re-read.
        await document.InvalidateDataAsync();

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Redone")
            .WithContent($"Put back: {redone.Description}.")
            .Queue();
    }
}

