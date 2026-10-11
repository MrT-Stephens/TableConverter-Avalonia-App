using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.History;
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
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.RemoveDuplicateRows,
        "Remove Duplicates",
        "Delete rows that repeat the values of a row above them.",
        "TrashIcon",
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

        // The rows the delete is about to take are named by the query the delete itself runs on, so what is
        // about to be lost is described before it goes and the step can be taken back with its values.
        await using var edit = history.BeginEdit(
            document.Path, TableEditKind.RowsDeleted, $"Removed {duplicates} duplicate row(s)");

        await edit.CaptureBeforeAsync(TableRegion.Rows(await maintenance.GetDuplicateRowIdsAsync()));

        var deleted = await maintenance.RemoveDuplicateRowsAsync();

        await edit.CommitAsync();

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