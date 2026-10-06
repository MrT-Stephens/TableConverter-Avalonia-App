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
    public const string Undo = "TableData.Undo";
}

/// <summary>
/// Takes the last change made to the table back.
/// </summary>
/// <remarks>
/// The step is replayed by the history itself, which holds a copy of the values the change put in place,
/// so undoing needs nothing of the table the grid is showing: the store is walked back and the grid is
/// read again once it has been.
/// </remarks>
public class UndoCommandHandler(
    ISukiToastManager toastManager,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.Undo,
        "Undo",
        "Take back the last change made to the table.",
        "UndoIcon",
        ["Ctrl+Z"],
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

        var undone = await history.UndoAsync(document.Path);

        if (undone is null)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Undo")
                .WithContent("There is no change left to take back.")
                .Queue();

            return;
        }

        // A step can have changed anything about the table - its columns, its rows, their order, or the
        // table as a whole - so the grid is rebuilt rather than merely re-read.
        await document.InvalidateDataAsync();

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Undone")
            .WithContent($"Took back: {undone.Description}.")
            .Queue();
    }
}

