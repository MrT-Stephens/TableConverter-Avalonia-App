using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string MoveColumnLeft = "TableData.MoveColumnLeft";
    public const string MoveColumnRight = "TableData.MoveColumnRight";
}

/// <summary>
/// Moves the selected column one place along the table.
/// </summary>
/// <remarks>
/// Both directions do the same thing and differ only in which way they move the column, so the command is
/// written once and the direction is what the two commands that extend this each answer.
/// </remarks>
public abstract class MoveColumnCommandHandler(ISukiToastManager toastManager)
    : ICommandHandlerAsync
{
    /// <summary>
    /// Which way this command moves the column: below zero towards the front of the table, above zero
    /// towards the end.
    /// </summary>
    protected abstract int Offset { get; }

    /// <summary>
    /// How the direction reads, which is what the command names itself and what the notices say.
    /// </summary>
    protected abstract string DirectionName { get; }

    public abstract ICommandMetadata CommandMetadata { get; }

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var viewModel)
            || !context.TryGetSelectedItem<DataItem<ColumnEntity>>(out var column))
        {
            return false;
        }

        // A column can only move towards a place the table has. The places columns hold run from one
        // without gaps, so the column at the front can only move towards the end and the one at the end
        // only towards the front.
        var position = column.Item.OrdinalPosition;
        var count = viewModel.DataSource.Collection.Count;

        return Offset < 0 ? position > 1 : position < count;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var viewModel))
        {
            context.Cancel("No table columns editor found.");
            return;
        }

        if (!context.TryGetSelectedItem<DataItem<ColumnEntity>>(out var column))
        {
            context.Cancel("No column selected.");
            return;
        }

        var name = column.Item.Name;

        if (!await viewModel.DataSource.MoveAsync(column.Item, Offset))
        {
            // The button is only offered while there is a place to move to, so this is the store disagreeing
            // with what the grid was showing rather than the column being at the edge as far as the user is
            // concerned.
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Column Not Moved")
                .WithContent($"'{name}' is already at the edge of the table it can move towards.")
                .Queue();

            return;
        }

        // The columns editor shows the columns in the order the store keeps them, so it is read again and
        // the column just moved is put back under the cursor. The table's own grid is rebuilt as well,
        // because a move swaps the places of two columns and every value is read by the place its column
        // holds.
        viewModel.DataSource.Invalidate();
        viewModel.SelectColumnAsync(column.Item.Id).FireAndForget();

        if (context.TryGetTableDocument(out var document))
        {
            await document.InvalidateDataAsync();
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Column Moved")
            .WithContent($"'{name}' was moved {DirectionName.ToLowerInvariant()}.")
            .Queue();
    }
}

/// <summary>
/// Moves the selected column one place towards the front of the table.
/// </summary>
public class MoveColumnLeftCommandHandler(ISukiToastManager toastManager)
    : MoveColumnCommandHandler(toastManager)
{
    protected override int Offset => -1;

    protected override string DirectionName => "Left";

    public override ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.MoveColumnLeft,
        "Move Left",
        "Move the selected column one place towards the front of the table.",
        "MoveLeftIcon",
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);
}

/// <summary>
/// Moves the selected column one place towards the end of the table.
/// </summary>
public class MoveColumnRightCommandHandler(ISukiToastManager toastManager)
    : MoveColumnCommandHandler(toastManager)
{
    protected override int Offset => 1;

    protected override string DirectionName => "Right";

    public override ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.MoveColumnRight,
        "Move Right",
        "Move the selected column one place towards the end of the table.",
        "MoveRightIcon",
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);
}

