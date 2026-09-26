using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string DeleteRows = "TableData.DeleteRows";
}

/// <summary>
/// Deletes the rows the user selected.
/// </summary>
/// <remarks>
/// The rows are picked out in the grid, and the selection is shared with the table utilities tool, so the
/// command works the same whether it is run from the grid's workspace menu or from the tool. What is read
/// off the grid is where the selected rows are rather than what they hold, because the grid only holds the
/// rows it has scrolled to.
/// </remarks>
public class DeleteRowsCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.DeleteRows,
        "Delete Rows",
        "Delete the selected row(s) from the table.",
        "DeleteIcon",
        "Edit",
        0,
        ["Ctrl+Shift+Delete"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetTableDocument(out var document)
               && document.GetSelectedRowPositions().Count > 0;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetTableDocument(out var document))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        var positions = document.GetSelectedRowPositions();

        if (positions.Count == 0)
        {
            context.Cancel("No rows are selected.");
            return;
        }

        var result = await dialogManager.CreateDialog()
            .WithTitle("Are you sure?")
            .WithContent($"You are about to delete {positions.Count} row(s). Are you sure you want to proceed?")
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Yes", "No")
            .TryShowAsync();

        if (!result)
        {
            return;
        }

        await using var db = await databaseContextFactory.CreateDbContextAsync(document.Path);

        var rowIds = await db.GetRowIdsAtPositionsAsync(positions);

        var deleted = await db.DeleteRowsAsync(rowIds);

        if (deleted < rowIds.Count)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Error Deleting Rows")
                .WithContent($"Failed to delete {rowIds.Count - deleted} out of {rowIds.Count} row(s).")
                .Queue();
        }
        else
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Success)
                .WithTitle("Rows Deleted")
                .WithContent($"Successfully deleted {deleted} row(s).")
                .Queue();
        }

        document.DataSource.Invalidate();
    }
}
