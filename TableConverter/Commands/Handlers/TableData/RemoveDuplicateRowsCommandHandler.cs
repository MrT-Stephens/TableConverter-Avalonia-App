using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string RemoveDuplicateRows = "TableData.RemoveDuplicateRows";
}

/// <summary>
/// Deletes every row whose values are all the same as those of a row kept above it.
/// </summary>
public class RemoveDuplicateRowsCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.RemoveDuplicateRows,
        "Remove Duplicates",
        "Delete rows that repeat the values of a row above them.",
        "TrashIcon",
        "Tools",
        1,
        ["Ctrl+Shift+U"],
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

        await using var db = await databaseContextFactory.CreateDbContextAsync(document.Path);

        var maintenance = TableStoreMaintenance.Create(db);

        // What the user is agreeing to is the number the delete itself works out, so the dialog cannot
        // promise one number and the delete take another.
        var duplicates = await maintenance.CountDuplicateRowsAsync();

        if (duplicates == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("No Duplicate Rows")
                .WithContent("Every row of the table holds a different set of values.")
                .Queue();

            return;
        }

        var result = await dialogManager.CreateDialog()
            .WithTitle("Are you sure?")
            .WithContent($"You are about to delete {duplicates} duplicate row(s). " +
                         "The first row of each repeated set of values is kept. Are you sure you want to proceed?")
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Yes", "No")
            .TryShowAsync();

        if (!result)
        {
            return;
        }

        var deleted = await maintenance.RemoveDuplicateRowsAsync();

        if (deleted < duplicates)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Error Removing Duplicates")
                .WithContent($"Failed to delete {duplicates - deleted} out of {duplicates} row(s).")
                .Queue();
        }
        else
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Success)
                .WithTitle("Duplicates Removed")
                .WithContent($"Successfully deleted {duplicates} duplicate row(s).")
                .Queue();
        }

        document.DataSource.Invalidate();
    }
}
