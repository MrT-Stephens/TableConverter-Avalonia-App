using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
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
    ITableStoreDbContextFactory databaseContextFactory)
    : ICommandHandlerAsync
{
    private static readonly string[] SortDirections = ["Ascending", "Descending"];

    /// <summary>
    /// The order the store keeps its rows in is their ids, so putting the table in order means renumbering
    /// its rows. The values the chosen column holds are read into a temporary table first, so the ordering
    /// is worked out once rather than for every row that is renumbered.
    /// </summary>
    /// <remarks>
    /// Which direction the sort runs in and whether the values are read as numbers or as text travel as
    /// parameters rather than being written into the script, and <c>NULL</c> is what the case expressions
    /// produce for the direction that is not in use, which leaves the other one to decide the order.
    /// </remarks>
    private const string SortSql = """
        DROP TABLE IF EXISTS T_SORT_KEYS;
        DROP TABLE IF EXISTS T_ROW_ORDER;
        DROP TABLE IF EXISTS T_SORTED_CELLS;

        -- A numeric column is read as a number before it is compared, because read as text 10 would come
        -- before 2. A missing value sorts alongside the empty text it is shown as.
        CREATE TEMP TABLE T_SORT_KEYS AS
        SELECT R.ID AS ROW_ID,
               CASE WHEN @READ_AS_NUMBER = 1
                        THEN CAST(C.VALUE AS REAL)
                    ELSE COALESCE(C.VALUE, '')
               END AS SORT_KEY
        FROM ROWS R
        LEFT JOIN CELLS C ON C.ROW_ID = R.ID AND C.COLUMN_ID = @COLUMN_ID;

        CREATE TEMP TABLE T_ROW_ORDER AS
        SELECT ROW_ID AS OLD_ID,
               ROW_NUMBER() OVER (
                   ORDER BY
                       CASE WHEN @DESCENDING = 0 THEN SORT_KEY END ASC,
                       CASE WHEN @DESCENDING = 1 THEN SORT_KEY END DESC,
                       ROW_ID
               ) AS NEW_ID
        FROM T_SORT_KEYS;

        CREATE TEMP TABLE T_SORTED_CELLS AS
        SELECT O.NEW_ID AS ROW_ID, C.COLUMN_ID, C.VALUE
        FROM CELLS C
        JOIN T_ROW_ORDER O ON O.OLD_ID = C.ROW_ID;

        DELETE FROM CELLS;
        DELETE FROM ROWS;

        INSERT INTO ROWS (ID) SELECT NEW_ID FROM T_ROW_ORDER ORDER BY NEW_ID;

        -- The cells are written in (row, column) order so that a row's cells come back in the order of the
        -- columns they belong to, which is the order the grid reads them in.
        INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE)
        SELECT ROW_ID, COLUMN_ID, VALUE FROM T_SORTED_CELLS ORDER BY ROW_ID, COLUMN_ID;

        DROP TABLE T_SORTED_CELLS;
        DROP TABLE T_ROW_ORDER;
        DROP TABLE T_SORT_KEYS;
        """;

    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.SortByColumn,
        "Sort by Column",
        "Put the rows of the table in the order of one of its columns.",
        "UpDownIcon",
        "Tools",
        2,
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
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            await db.Database.ExecuteSqlRawAsync(SortSql,
                new SqliteParameter("@COLUMN_ID", column.Id),
                new SqliteParameter("@READ_AS_NUMBER", column.DataType.IsNumeric() ? 1 : 0),
                new SqliteParameter("@DESCENDING", isDescending ? 1 : 0));

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

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
