using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Microsoft.EntityFrameworkCore;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string SortByColumn = "TableData.SortByColumn";
}

/// <summary>
/// Puts the rows of the table in the order of one of its columns.
/// </summary>
public class SortByColumnCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    private static readonly string[] SortDirections = ["Ascending", "Descending"];

    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.SortByColumn,
        "Sort by Column",
        "Put the rows of the table in the order of one of its columns.",
        "UpDownIcon",
        ["Ctrl+Shift+S"],
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

        List<ColumnEntity> columns;

        await using (var reader = await databaseContextFactory.CreateDbContextAsync(document.Path))
        {
            columns = await reader.Columns
                .AsNoTracking()
                .OrderBy(column => column.OrdinalPosition)
                .ToListAsync();
        }

        if (columns.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Sort By")
                .WithContent("The table has no columns, so there is nothing to order its rows by.")
                .Queue();

            return;
        }

        // The two choices are asked for in one dialog: the column and the direction belong together, and
        // splitting them over two dialogs would let the second one be dismissed without an answer.
        var columnList = CreateChoices(columns.Select(column => column.Name), maxHeight: 250);
        var directionList = CreateChoices(SortDirections);

        directionList.SelectedIndex = 0;

        var confirmed = await dialogManager.CreateDialog()
            .WithTitle("Sort Table")
            .WithContentList(
            [
                CreateLabelled("Sort by", columnList),
                CreateLabelled("Order", directionList),
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Sort", "Cancel")
            .TryShowAsync();

        if (!confirmed)
        {
            return;
        }

        var columnName = columnList.SelectedItem as string;

        var column = columns.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, columnName, StringComparison.Ordinal));

        if (column is null)
        {
            context.Cancel("No column was selected to sort by.");
            return;
        }

        var isDescending = directionList.SelectedIndex == 1;

        await using var db = await databaseContextFactory.CreateDbContextAsync(document.Path);

        // Putting the rows in a different order renumbers them, and a renumbering that lands on the same
        // range of ids the rows already held leaves the set of ids unchanged - so what is remembered is the
        // whole table the sort is about to rearrange rather than the order its rows are named in. An
        // ordering of the ids cannot describe the move, because the ids are the ordering.
        await using var edit = history.BeginEdit(
            document.Path,
            TableEditKind.RowOrderChanged,
            $"Sorted by '{column.Name}' {(isDescending ? "descending" : "ascending")}");

        await edit.CaptureBeforeAsync(TableRegion.Table());

        // The order is worked out from the values read as the type the column was given, so the rows go
        // in the order of what the column holds rather than the order it happens to be written in.
        await TableStoreMaintenance.Create(db)
            .SortByColumnAsync(column.Id, column.DataType, isDescending);

        await edit.CommitAsync();

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Table Sorted")
            .WithContent($"The rows are now in {(isDescending ? "descending" : "ascending")} order of '{column.Name}'.")
            .Queue();

        document.DataSource.Invalidate();
    }

    /// <summary>
    /// Builds the picker a choice is made with.
    /// </summary>
    /// <param name="choices">The values to choose between.</param>
    /// <param name="maxHeight">How tall the picker may grow, when it holds an unknown number of choices.</param>
    /// <remarks>
    /// A list is used rather than a drop down so the two choices the dialog asks for look and read the same
    /// way.
    /// </remarks>
    private static ListBox CreateChoices(IEnumerable<string> choices, double maxHeight = double.PositiveInfinity)
    {
        return new ListBox
        {
            ItemsSource = choices.ToList(),
            SelectionMode = SelectionMode.AlwaysSelected | SelectionMode.Single,
            MaxHeight = maxHeight,
            ItemsPanel = new FuncTemplate<Panel?>(() => new UniformGrid
            {
                Columns = 2,
                RowSpacing = 10,
                ColumnSpacing = 10,
            }),
        };
    }

    /// <summary>
    /// Puts a caption above a picker so what it is asking for is not left to be guessed.
    /// </summary>
    private static StackPanel CreateLabelled(string caption, Control picker)
    {
        return new StackPanel
        {
            Spacing = 5,
            Children =
            {
                new TextBlock { Text = caption },
                picker,
            },
        };
    }
}