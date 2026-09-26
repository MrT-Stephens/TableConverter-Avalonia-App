using System;
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

/// <summary>
/// Turns the whole table a quarter turn, in whichever of the two directions the command is for.
/// </summary>
/// <remarks>
/// <para>
/// Both directions of the turn do the same thing to the same table and differ only in which way round
/// they do it, so the command is written once and the direction is what the two commands that extend
/// this each answer.
/// </para>
/// <para>
/// The heading row turns with the table rather than being held aside and named afresh: it becomes a
/// column of the result, and the column the turn brings to the top becomes the new heading row. Turning
/// a table one way and then the other therefore leaves it exactly as it was.
/// </para>
/// </remarks>
public abstract class TableRotationCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory)
    : ICommandHandlerAsync
{
    /// <summary>
    /// The direction this command turns the table in.
    /// </summary>
    protected abstract TableRotation Rotation { get; }

    /// <summary>
    /// How the direction reads, which is how the command names itself and how the dialogs describe the
    /// turn.
    /// </summary>
    protected abstract string DirectionName { get; }

    public abstract ICommandMetadata CommandMetadata { get; }

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

        var direction = DirectionName.ToLowerInvariant();

        var confirmed = await dialogManager.CreateDialog()
            .WithTitle($"Transpose {DirectionName}")
            .WithContent($"The whole table will be turned a quarter turn {direction}, its headings with it: " +
                         "the heading row becomes a column of the result and the column that comes up to " +
                         "the top becomes the new heading row. Turning it back the other way restores the " +
                         "table.")
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Transpose", "Cancel")
            .TryShowAsync();

        if (!confirmed)
        {
            return;
        }

        await using var db = await databaseContextFactory.CreateDbContextAsync(document.Path);

        var shape = await TableStoreMaintenance.Create(db).RotateAsync(Rotation);

        // A table with no columns holds nothing to turn, and turning it could not be described, so it is
        // reported rather than replaced with a table that says nothing.
        if (shape is not { } turned)
        {
            context.Cancel("The table has no columns to turn.");
            return;
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle($"Table Transposed {DirectionName}")
            .WithContent($"The table now has {turned.ColumnCount} column(s) and {turned.RowCount} row(s).")
            .Queue();

        // The columns of the table changed, so the grid has to be rebuilt rather than just re-read.
        await document.InvalidateDataAsync();
    }
}

