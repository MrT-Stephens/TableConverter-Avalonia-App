using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Database;

/// <summary>
///     The table wide operations that change a store in place: the passes a user runs to clean a table
///     up after an import, the quarter turn that swaps its rows for its columns, and the row insert the
///     row utilities use.
/// </summary>
/// <remarks>
///     <para>
///         The operations that touch many cells are written with raw SQL rather than through the change
///         tracker. A store is the size it is because its table is large, so an update which loaded the
///         rows it changes would cost as much memory as the table itself; a single statement costs none
///         of it. Columns are the exception, because there are few of them and because changing one
///         through the change tracker is what raises the entity changed events the open views redraw
///         from.
///     </para>
///     <para>
///         An operation that is more than one statement runs in a transaction, so a failure leaves the
///         store exactly as it was rather than half cleaned.
///     </para>
/// </remarks>
public sealed class TableStoreMaintenance
{
    /// <summary>
    ///     The characters a pass trims from the ends of a value. SQLite's <c>TRIM</c> removes only spaces
    ///     unless it is handed the set of characters to remove, and a padded export carries a tab or a
    ///     carriage return as readily as it does a space.
    /// </summary>
    private const string WhitespaceSql = "CHAR(9) || CHAR(10) || CHAR(11) || CHAR(12) || CHAR(13) || CHAR(32)";

    /// <summary>
    ///     How a decimal is written when two of them are compared. The digits are written with no
    ///     thousands grouped and no trailing zeroes, so 1, 1.0 and 1.00 are written the same, and there is
    ///     a place after the point for every place a decimal can hold, so no precision is rounded away by
    ///     being compared.
    /// </summary>
    private const string DecimalTokenFormat = "#0.############################";

    /// <summary>
    ///     How many rows a rotation writes per change set, so turning a table that is very wide does not
    ///     have to hold the whole of its other direction as one change.
    /// </summary>
    private const int RowsPerBatch = 250;

    private static readonly string TrimSql = $"""
        UPDATE CELLS
        SET VALUE = TRIM(VALUE, {WhitespaceSql})
        WHERE {TrimmableCellSql("VALUE")};
        """;

    /// <summary>
    ///     The rows that hold a cell a trim would change, so what is about to be rewritten can be remembered
    ///     before it is.
    /// </summary>
    /// <remarks>
    ///     The rows are found by the condition the trim itself runs on rather than by reading the table, so
    ///     naming them costs what the change costs rather than what the table costs. A row the trim would
    ///     leave alone is left out, which is what keeps an entry describing the trim from holding values the
    ///     trim never touched.
    /// </remarks>
    private static readonly string RowsToTrimSql = $"""
        SELECT DISTINCT ROW_ID AS Value
        FROM CELLS
        WHERE {TrimmableCellSql("VALUE")}
        ORDER BY ROW_ID;
        """;

    /// <summary>
    ///     The rows that hold no cells at all, which hold nothing and so repeat one another.
    /// </summary>
    /// <remarks>
    ///     A row holding no cells is not met while the cells are being read, so the rows holding none are
    ///     read on their own and given the place they hold among the rows that hold nothing.
    /// </remarks>
    private static readonly string RowsWithoutCellsSql = """
        SELECT ROWS.ID AS Value
        FROM ROWS
        WHERE NOT EXISTS (
            SELECT 1
            FROM CELLS
            WHERE CELLS.ROW_ID = ROWS.ID
        )
        ORDER BY ROWS.ID;
        """;

    private static readonly string FillEmptyCellsSql = $"""
        UPDATE CELLS
        SET VALUE = (
            SELECT C.DEFAULT_VALUE_FOR_CELL
            FROM COLUMNS C
            WHERE C.ID = CELLS.COLUMN_ID
        )
        WHERE {FillableCellSql};
        """;

    /// <summary>
    ///     The rows that hold a cell a fill would change, so what is about to be rewritten can be remembered
    ///     before it is.
    /// </summary>
    private static readonly string RowsToFillSql = $"""
        SELECT DISTINCT ROW_ID AS Value
        FROM CELLS
        WHERE {FillableCellSql}
        ORDER BY ROW_ID;
        """;

    private static readonly string RemoveEmptyRowsSql = $"""
        DELETE FROM ROWS
        WHERE {EmptyRowSql};
        """;

    /// <summary>
    ///     The rows <see cref="RemoveEmptyRowsSql" /> deletes, so what is about to be removed can be
    ///     remembered before it is.
    /// </summary>
    private static readonly string EmptyRowIdsSql = $"""
        SELECT ROWS.ID AS Value
        FROM ROWS
        WHERE {EmptyRowSql}
        ORDER BY ROWS.ID;
        """;

    private readonly TableStoreDbContext _dbContext;

    private TableStoreMaintenance(TableStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Creates the maintenance operations for the store behind <paramref name="dbContext" />.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to change. The caller owns it.</param>
    /// <remarks>
    ///     The operations never dispose the context or commit a transaction they did not open.
    /// </remarks>
    public static TableStoreMaintenance Create(TableStoreDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return new TableStoreMaintenance(dbContext);
    }

    /// <summary>
    ///     Trims leading and trailing whitespace from every cell value and every column name, which is
    ///     what an imported file with padded fields needs before anything can be matched against it.
    /// </summary>
    /// <returns>The number of cells that were trimmed.</returns>
    public async Task<int> TrimAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.RunAsync(async () =>
        {
            // A value that is nothing but whitespace is trimmed to nothing rather than left holding the
            // whitespace, which is what lets the passes that treat an empty cell as a missing one see it.
            var trimmedCells = await _dbContext.Database
                .ExecuteSqlRawAsync(TrimSql, cancellationToken)
                .ConfigureAwait(false);

            await TrimColumnNamesAsync(cancellationToken).ConfigureAwait(false);

            return trimmedCells;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Fills every cell that holds nothing with the default value of the column it belongs to. A
    ///     column without a default value is left alone, so the pass never invents data.
    /// </summary>
    /// <returns>The number of cells that were filled.</returns>
    public async Task<int> FillEmptyCellsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database
            .ExecuteSqlRawAsync(FillEmptyCellsSql, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Deletes every row whose cells all hold nothing.
    /// </summary>
    /// <returns>The number of rows that were deleted.</returns>
    public async Task<int> RemoveEmptyRowsAsync(CancellationToken cancellationToken = default)
    {
        // The cells of a deleted row go with it: the foreign key from CELLS to ROWS cascades, which is
        // what keeps a row from leaving orphaned cells behind.
        return await _dbContext.Database
            .ExecuteSqlRawAsync(RemoveEmptyRowsSql, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Counts the rows that repeat a row above them, which is the number
    ///     <see cref="RemoveDuplicateRowsAsync" /> would delete.
    /// </summary>
    /// <returns>The number of rows that repeat another row.</returns>
    public async Task<int> CountDuplicateRowsAsync(CancellationToken cancellationToken = default)
    {
        return (await FindDuplicateRowIdsAsync(cancellationToken).ConfigureAwait(false)).Count;
    }

    /// <summary>
    ///     Deletes every row that repeats a row above it, keeping the first of each set.
    /// </summary>
    /// <returns>The number of rows that were deleted.</returns>
    public async Task<int> RemoveDuplicateRowsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.RunAsync(async () =>
        {
            // The rows that go are named a moment before they go, by the same reading of the values the
            // count and the undo entry are made from, so the rows removed are exactly the rows a user was
            // told they would lose.
            var duplicates = await FindDuplicateRowIdsAsync(cancellationToken).ConfigureAwait(false);

            var deleted = 0;

            // The ids are deleted in batches, so a table whose rows are nearly all repeats is not deleted
            // through one statement holding every id the table holds.
            foreach (var batch in duplicates.Chunk(RowsPerBatch))
            {
                // The ids were read out of the store, so there is nothing in them that could be read as
                // anything but a number.
                var ids = string.Join(",", batch.Select(id => id.ToString(CultureInfo.InvariantCulture)));

#pragma warning disable EF1002
                deleted += await _dbContext.Database
                    .ExecuteSqlRawAsync($"DELETE FROM ROWS WHERE ID IN ({ids});", cancellationToken)
                    .ConfigureAwait(false);
#pragma warning restore EF1002
            }

            return deleted;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Puts the rows of the table in the order of one of its columns, so the table reads top to bottom
    ///     the way that column's values go.
    /// </summary>
    /// <param name="columnId">The column whose values the rows are put in order by.</param>
    /// <param name="dataType">The type of that column, which is what decides how a value reads.</param>
    /// <param name="descending">Whether the order runs from the greatest value to the least.</param>
    /// <returns>The number of rows the table holds.</returns>
    /// <remarks>
    ///     The order is worked out here rather than in the statement that writes it, because a value only
    ///     takes its place once it has been read the way its column's type says it should be read, and a
    ///     statement cannot read a value the way a type does: asked to compare the text "9" with the text
    ///     "10" a database puts 9 after 10, and asked to compare two dates it puts whichever was written
    ///     first first, whatever day either of them names. The rows are then put in that order by the same
    ///     write that replays a change of order from the history, so a sort and its undo are one change.
    /// </remarks>
    public async Task<int> SortByColumnAsync(
        int columnId,
        ColumnDataType dataType,
        bool descending,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.RunAsync(async () =>
        {
            // A row's id is the place it sits in, so the rows are read in the order the store already
            // holds them and put back in the order the column's values go.
            var rowIds = await _dbContext.Rows
                .AsNoTracking()
                .OrderBy(row => row.Id)
                .Select(row => row.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (rowIds.Count == 0)
            {
                return 0;
            }

            var values = await _dbContext.Cells
                .AsNoTracking()
                .Where(cell => cell.ColumnId == columnId)
                .Select(cell => new { cell.RowId, cell.Value })
                .ToDictionaryAsync(cell => cell.RowId, cell => cell.Value, cancellationToken)
                .ConfigureAwait(false);

            var keys = rowIds
                .Select(rowId => ReadSortKey(dataType, rowId, values.GetValueOrDefault(rowId)))
                .ToList();

            keys.Sort((left, right) => left.CompareTo(right, descending));

            var order = keys.Select(key => key.RowId).ToList();

            if (order.SequenceEqual(rowIds))
            {
                // The rows are in order already, so the table is left exactly as it is rather than written
                // back into the order it is already in.
                return rowIds.Count;
            }

            // Where each row ends up: the first row of the order becomes the first row of the table.
            var places = new Dictionary<int, int>(order.Count);

            for (var index = 0; index < order.Count; index++)
            {
                places[order[index]] = index + 1;
            }

            await TableEditWriter.ReorderRowsAsync(
                _dbContext,
                rowIds,
                rowIds.Select(rowId => places[rowId]).ToList(),
                cancellationToken).ConfigureAwait(false);

            return rowIds.Count;
        }, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    ///     Names the rows that hold a cell <see cref="TrimAsync" /> would change, so what is about to be
    ///     rewritten can be remembered before it is.
    /// </summary>
    public async Task<IReadOnlyList<int>> GetRowIdsToTrimAsync(CancellationToken cancellationToken = default)
    {
        return await ReadRowIdsAsync(RowsToTrimSql, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Names the rows that hold a cell <see cref="FillEmptyCellsAsync" /> would change.
    /// </summary>
    public async Task<IReadOnlyList<int>> GetRowIdsToFillAsync(CancellationToken cancellationToken = default)
    {
        return await ReadRowIdsAsync(RowsToFillSql, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Names the rows <see cref="RemoveEmptyRowsAsync" /> would delete, so what is about to be removed
    ///     can be remembered before it is.
    /// </summary>
    public async Task<IReadOnlyList<int>> GetEmptyRowIdsAsync(CancellationToken cancellationToken = default)
    {
        return await ReadRowIdsAsync(EmptyRowIdsSql, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Names the rows <see cref="RemoveDuplicateRowsAsync" /> would delete, so what is about to be
    ///     removed can be remembered before it is.
    /// </summary>
    public async Task<IReadOnlyList<int>> GetDuplicateRowIdsAsync(CancellationToken cancellationToken = default)
    {
        return await FindDuplicateRowIdsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Names the columns <see cref="RemoveEmptyColumnsAsync" /> would delete, so what is about to be
    ///     removed can be remembered before it is.
    /// </summary>
    public async Task<IReadOnlyList<int>> GetEmptyColumnIdsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Columns
            .AsNoTracking()
            .Where(column => !column.Cells.Any(cell => cell.Value != null && cell.Value.Trim() != ""))
            .OrderBy(column => column.OrdinalPosition)
            .Select(column => column.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Deletes every column whose cells all hold nothing, and closes the gap the deleted columns
    ///     leave in the ordinal positions.
    /// </summary>
    /// <returns>The number of columns that were deleted.</returns>
    /// <remarks>
    ///     The columns are removed through the change tracker rather than with a statement, because a
    ///     deleted column is what the entity changed events tell the open grids to drop their column for.
    /// </remarks>
    public async Task<int> RemoveEmptyColumnsAsync(CancellationToken cancellationToken = default)
    {
        var emptyColumnIds = await GetEmptyColumnIdsAsync(cancellationToken).ConfigureAwait(false);

        if (emptyColumnIds.Count == 0)
        {
            return 0;
        }

        return await _dbContext.Database.RunAsync(async () =>
        {
            var columns = await _dbContext.Columns
                .Where(column => emptyColumnIds.Contains(column.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _dbContext.Columns.RemoveRange(columns);

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await RenumberColumnsAsync(cancellationToken).ConfigureAwait(false);

            return columns.Count;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Appends a row whose cells are each given the default value of the column they belong to, which
    ///     is what the row has to look like for the grid to bind to it by index.
    /// </summary>
    /// <returns>The id of the row that was added.</returns>
    public async Task<int> AddRowAsync(CancellationToken cancellationToken = default)
    {
        var columns = await _dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var row = new RowEntity();

        foreach (var column in columns)
        {
            // A default value that is empty is stored as nothing at all, matching how a missing value was
            // written when the table was imported.
            var value = column.DefaultValueForCell?.Trim();

            row.Cells.Add(new CellEntity
            {
                ColumnId = column.Id,
                Value = string.IsNullOrEmpty(value) ? null : value,
            });
        }

        await _dbContext.Rows.AddAsync(row, cancellationToken).ConfigureAwait(false);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return row.Id;
    }

    /// <summary>
    ///     Turns the whole table a quarter turn, so that what was a row of it becomes a column and what
    ///     was a column becomes a row.
    /// </summary>
    /// <param name="rotation">The direction to turn the table in.</param>
    /// <param name="cancellationToken">Token used to cancel the rotation.</param>
    /// <returns>
    ///     The shape the table has once it has been turned, or <see langword="null" /> when the table has
    ///     nothing to turn.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Nothing is held aside and named afresh. The heading row is a row of the grid like any
    ///         other, so it turns with the rest of it: it becomes a column of the result, and the column
    ///         that was at the edge the turn brought to the top becomes the new heading row. The headings
    ///         of the result are therefore values the table already held. Turning a table one way and then
    ///         the other leaves it exactly as it was, down to the names of its columns.
    ///     </para>
    ///     <para>
    ///         A value ends up in a column decided by where it sat rather than by what it holds, so the
    ///         result is left untyped: a column of the result holds whatever the table put in its way.
    ///     </para>
    /// </remarks>
    public async Task<TableShape?> RotateAsync(
        TableRotation rotation,
        CancellationToken cancellationToken = default)
    {
        var columns = await _dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // A table with no columns holds nothing to turn and nothing to head the result with, so it is
        // left exactly as it is.
        if (columns.Count == 0)
        {
            return null;
        }

        var rowIds = await _dbContext.Rows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // A turn has to know every value of the grid before it can write any of them, so unlike the other
        // passes this one reads the whole table rather than streaming it. Cells are grouped by the place
        // they sit in so a store that somehow holds two cells for one column still turns cleanly.
        var values = (await _dbContext.Cells
                .AsNoTracking()
                .Select(cell => new { cell.RowId, cell.ColumnId, cell.Value })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .GroupBy(cell => (cell.RowId, cell.ColumnId))
            .ToDictionary(group => group.Key, group => group.First().Value);

        string ValueAt(int rowId, int columnId)
        {
            values.TryGetValue((rowId, columnId), out var value);

            return value ?? string.Empty;
        }

        // The grid a user turns is the table's headings plus its rows, which is one row more than the
        // store keeps: the headings live on the columns rather than in a row of their own.
        var gridRowCount = rowIds.Count + 1;

        // The value at a place in the grid before it is turned, read from whichever of the two the place
        // belongs to: the first row is the headings, and every row after it is a row of the store.
        string GridValueAt(int gridRowIndex, int columnIndex)
        {
            return gridRowIndex == 0
                ? columns[columnIndex].Name
                : ValueAt(rowIds[gridRowIndex - 1], columns[columnIndex].Id);
        }

        // A quarter turn stands a grid of so many rows and columns on its side, so the result is as tall
        // as the grid was wide and as wide as the grid was tall. The row the turn leaves at the top is
        // the result's heading row and does not count as one of its rows.
        var turnedRowCount = columns.Count - 1;
        var turnedColumnCount = gridRowCount;

        // Where each place in the turned grid is read from. Turning clockwise brings the left of the grid
        // to the top, so the result reads down the grid's columns and back up its rows; turning
        // anticlockwise brings the right of the grid to the top, which is the same the other way round.
        (int GridRow, int Column) SourceOf(int turnedRowIndex, int turnedColumnIndex)
        {
            return rotation is TableRotation.Clockwise
                ? (rowIds.Count - turnedColumnIndex, turnedRowIndex)
                : (turnedColumnIndex, columns.Count - 1 - turnedRowIndex);
        }

        return await _dbContext.Database.RunAsync(async () =>
        {
            // The cells belong to the rows and columns, so emptying the store also empties it of the
            // cells the turned table is about to replace.
            await _dbContext.Cells.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            await _dbContext.Rows.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            await _dbContext.Columns.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

            var newColumns = new List<ColumnEntity>(turnedColumnCount);

            for (var index = 0; index < turnedColumnCount; index++)
            {
                var (gridRow, column) = SourceOf(0, index);

                newColumns.Add(new ColumnEntity
                {
                    Name = GridValueAt(gridRow, column),
                    DataType = ColumnDataType.Text,
                    OrdinalPosition = index + 1,
                });
            }

            _dbContext.Columns.AddRange(newColumns);

            // Saved first so the new cells have the ids of the columns they are written against.
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var pending = 0;

            // The first row of the turned grid is its headings, which the columns just took, so the rows
            // written are the ones after it, of which there are as many as the grid had columns.
            for (var turnedRowIndex = 1; turnedRowIndex <= turnedRowCount; turnedRowIndex++)
            {
                var row = new RowEntity();

                for (var index = 0; index < turnedColumnCount; index++)
                {
                    var (gridRow, column) = SourceOf(turnedRowIndex, index);

                    row.Cells.Add(new CellEntity
                    {
                        ColumnId = newColumns[index].Id,
                        Value = GridValueAt(gridRow, column),
                    });
                }

                _dbContext.Rows.Add(row);
                pending++;

                if (pending < RowsPerBatch)
                {
                    continue;
                }

                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                pending = 0;
            }

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new TableShape(turnedRowCount, turnedColumnCount);
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     The cells a trim would change. A value that is nothing but whitespace is trimmed to nothing
    ///     rather than left holding the whitespace, which is what lets the passes that treat an empty cell
    ///     as a missing one see it.
    /// </summary>
    private static string TrimmableCellSql(string valueSql)
    {
        return $"{valueSql} IS NOT NULL AND TRIM({valueSql}, {WhitespaceSql}) <> {valueSql}";
    }

    /// <summary>
    ///     The cells a fill would change: one that holds nothing, in a column that holds a default value
    ///     to fill it with. A column without one is left alone, so the pass never invents data.
    /// </summary>
    private static string FillableCellSql => $"""
        {EmptyCellSql("VALUE")}
          AND COALESCE((
              SELECT C.DEFAULT_VALUE_FOR_CELL
              FROM COLUMNS C
              WHERE C.ID = CELLS.COLUMN_ID
          ), '') <> ''
        """;

    /// <summary>
    ///     The rows that hold nothing, which are the rows a de-emptying pass removes.
    /// </summary>
    private static string EmptyRowSql => $"""
        NOT EXISTS (
            SELECT 1
            FROM CELLS
            WHERE CELLS.ROW_ID = ROWS.ID
              AND {NotEmptyCellSql("CELLS.VALUE")}
        )
        """;

    /// <summary>
    ///     Names the rows that repeat a row above them, which is every row but the first of each set of
    ///     rows holding the same values.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    /// <returns>The ids of the rows that repeat another row, in the order the grid shows them.</returns>
    /// <remarks>
    ///     <para>
    ///         Two rows match when every cell of one holds the same value as the cell of the other in the
    ///         same column, and a value is read the way its column's type says it should be read before the
    ///         two are compared. That is what makes 1 and 1.0 one value in a column of numbers, and a date
    ///         written one way the same as the same date written another.
    ///     </para>
    ///     <para>
    ///         A row's values are read a row at a time and made into a signature of that row, and the
    ///         signature is compared by a hash of itself rather than kept whole: a signature holds every
    ///         value of its row, so keeping the whole of one for each row would cost as much as the rows
    ///         themselves. The hash is wide enough that two rows that differ cannot share one, so the rows
    ///         it names really do match.
    ///     </para>
    /// </remarks>
    private async Task<List<int>> FindDuplicateRowIdsAsync(CancellationToken cancellationToken)
    {
        var columnTypes = await _dbContext.Columns
            .AsNoTracking()
            .Select(column => new { column.Id, column.DataType })
            .ToDictionaryAsync(column => column.Id, column => column.DataType, cancellationToken)
            .ConfigureAwait(false);

        // The row a set of repeats leaves behind is the first of them in the order the grid shows, which
        // is the order of their ids, so the rows are read in that order and the first row seen for a
        // signature is the one that is kept.
        var keepers = new Dictionary<(ulong High, ulong Low), int>();
        var duplicates = new List<int>();

        var signature = new StringBuilder();
        var rowId = -1;
        var reading = false;

        var cells = _dbContext.Cells
            .AsNoTracking()
            .OrderBy(cell => cell.RowId)
            .ThenBy(cell => cell.ColumnId)
            .Select(cell => new { cell.RowId, cell.ColumnId, cell.Value });

        // The cells are read a row at a time rather than loaded, so reading them costs what the row being
        // read holds rather than what the table holds.
        await foreach (var cell in cells.AsAsyncEnumerable().WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            if (!reading || cell.RowId != rowId)
            {
                if (reading)
                {
                    Remember(rowId, signature.ToString());
                }

                signature.Clear();
                rowId = cell.RowId;
                reading = true;
            }

            // The column a value belongs to is part of the signature, so a row missing a cell is not read
            // as a row holding the same value in another column. The length of the value is written before
            // it, so no value can be made to read as the join of two others.
            var token = ReadComparisonToken(
                columnTypes.GetValueOrDefault(cell.ColumnId, ColumnDataType.Text), cell.Value);

            signature.Append(cell.ColumnId).Append(':').Append(token.Length).Append(':').Append(token);
        }

        if (reading)
        {
            Remember(rowId, signature.ToString());
        }

        // A row holding no cells at all holds nothing and is not met while the cells are read, so the rows
        // holding none are read on their own and read as holding that same nothing.
        foreach (var emptyRowId in await ReadRowIdsAsync(RowsWithoutCellsSql, cancellationToken)
                     .ConfigureAwait(false))
        {
            Remember(emptyRowId, string.Empty);
        }

        // The rows that are removed are named in the order the grid shows them.
        duplicates.Sort();

        return duplicates;

        void Remember(int id, string values)
        {
            if (!keepers.TryAdd(HashSignature(values), id))
            {
                // The signature has been met before, so this row repeats the row that was kept.
                duplicates.Add(id);
            }
        }
    }

    /// <summary>
    ///     Reads a cell's value as the one form two values in the same column are compared by.
    /// </summary>
    /// <param name="dataType">The type of the column the value belongs to.</param>
    /// <param name="text">The value as it was stored.</param>
    /// <returns>The form of the value that two values are compared by.</returns>
    /// <remarks>
    ///     A value that reads as its column's type is read as that type, so two values a reader would call
    ///     the same value count as the same value however they happen to be written. A value that does not
    ///     read as the type is left as the text it is, marked so that it can never be read as the same
    ///     value as one that does: two rows are never called repeats over a value neither of them holds.
    /// </remarks>
    private static string ReadComparisonToken(ColumnDataType dataType, string? text)
    {
        // A cell holding nothing holds the same nothing whatever its column's type, so a value that is
        // missing and a value that is empty are not told apart.
        if (string.IsNullOrEmpty(text))
        {
            return "n";
        }

        if (!dataType.TryReadValue(text, out var typed) || typed is string)
        {
            return $"x{text}";
        }

        return typed switch
        {
            long integer => $"i{integer.ToString(CultureInfo.InvariantCulture)}",
            decimal number => $"d{number.ToString(DecimalTokenFormat, CultureInfo.InvariantCulture)}",
            bool flag => flag ? "bt" : "bf",
            DateOnly day => $"y{day.ToString("O", CultureInfo.InvariantCulture)}",
            DateTime moment => $"m{moment.ToString("O", CultureInfo.InvariantCulture)}",
            _ => $"x{text}",
        };
    }

    /// <summary>
    ///     Reads a signature as the pair of numbers two signatures are compared by.
    /// </summary>
    /// <param name="signature">The values of one row, read into the one string.</param>
    /// <returns>A hash of the signature, in two halves.</returns>
    /// <remarks>
    ///     A signature holds every value of a row, so keeping the whole of one for each row would cost as
    ///     much as the rows themselves. What is kept is a hash of it instead, and a hash this wide cannot
    ///     be shared by two signatures that differ, so nothing that differs is ever read as a repeat.
    /// </remarks>
    private static (ulong High, ulong Low) HashSignature(string signature)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(signature));

        return (
            BinaryPrimitives.ReadUInt64LittleEndian(digest),
            BinaryPrimitives.ReadUInt64LittleEndian(digest.AsSpan(8)));
    }

    /// <summary>
    ///     Reads a row's value from a column as the place it takes in an order.
    /// </summary>
    /// <param name="dataType">The type of the column the value belongs to.</param>
    /// <param name="rowId">The id of the row the value belongs to.</param>
    /// <param name="text">The value as it was stored.</param>
    /// <returns>The place the row takes in the order of the column's values.</returns>
    /// <remarks>
    ///     A value that reads as its column's type takes its place as that type reads it, so 9 comes
    ///     before 10 and a date takes its place among the dates. A value holding nothing, or one that does
    ///     not read as the type, is given a place before the values that do read - the way a database puts
    ///     a missing value first - and is ordered among the rest by the text it is, so a column whose
    ///     values do not all read as its type still sorts the same way twice rather than at random.
    /// </remarks>
    private static SortKey ReadSortKey(ColumnDataType dataType, int rowId, string? text)
    {
        return dataType.TryReadValue(text, out var typed) && typed is not string
            ? new SortKey(rowId, true, typed, text ?? string.Empty)
            : new SortKey(rowId, false, null, text ?? string.Empty);
    }

    /// <summary>
    ///     Compares two values that have both been read as the same column's type.
    /// </summary>
    /// <param name="left">One value, read as the column's type.</param>
    /// <param name="right">The value to compare it with, read as the column's type.</param>
    /// <returns>Which of the two comes first.</returns>
    /// <remarks>
    ///     The values of one column all read as the one .NET type, so two of them are compared as that
    ///     type rather than as the text they were written as.
    /// </remarks>
    private static int CompareTyped(object? left, object? right)
    {
        return (left, right) switch
        {
            (long leftInteger, long rightInteger) => leftInteger.CompareTo(rightInteger),
            (decimal leftNumber, decimal rightNumber) => leftNumber.CompareTo(rightNumber),
            (bool leftFlag, bool rightFlag) => leftFlag.CompareTo(rightFlag),
            (DateOnly leftDay, DateOnly rightDay) => leftDay.CompareTo(rightDay),
            (DateTime leftMoment, DateTime rightMoment) => leftMoment.CompareTo(rightMoment),
            _ => 0,
        };
    }

    /// <summary>
    ///     Where a row sits in the order of a column: the value read the way the column's type says it
    ///     should be read, with the row's id kept so that rows holding the same value keep the order they
    ///     were already in.
    /// </summary>
    private readonly struct SortKey
    {
        private readonly int _rowId;
        private readonly bool _readsAsType;
        private readonly object? _value;
        private readonly string _text;

        public SortKey(int rowId, bool readsAsType, object? value, string text)
        {
            _rowId = rowId;
            _readsAsType = readsAsType;
            _value = value;
            _text = text;
        }

        /// <summary>The id of the row this is the place of.</summary>
        public int RowId => _rowId;

        /// <summary>
        ///     Puts this place before or after another, running the way <paramref name="descending" /> asks.
        /// </summary>
        /// <param name="other">The place to compare with.</param>
        /// <param name="descending">Whether the order runs from the greatest value to the least.</param>
        /// <returns>Which of the two places comes first.</returns>
        public int CompareTo(SortKey other, bool descending)
        {
            var comparison = CompareValue(other);

            if (comparison != 0)
            {
                return descending ? -comparison : comparison;
            }

            // Two rows holding the same value keep the order they were already in, so a sort moves what it
            // has to and leaves the rest where they were.
            return _rowId.CompareTo(other._rowId);
        }

        private int CompareValue(SortKey other)
        {
            if (_readsAsType != other._readsAsType)
            {
                return _readsAsType ? 1 : -1;
            }

            return _readsAsType
                ? CompareTyped(_value, other._value)
                : string.CompareOrdinal(_text, other._text);
        }
    }


    /// <summary>
    ///     Reads the ids a statement names, which the statement selects as <c>Value</c> so the provider
    ///     reads them the same way it reads a single scalar.
    /// </summary>
    private async Task<List<int>> ReadRowIdsAsync(string sql, CancellationToken cancellationToken)
    {
        return await _dbContext.Database
            .SqlQueryRaw<int>(sql)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     The SQL a cell holds nothing by. A cell reads as nothing whether it is missing, null, empty or
    ///     whitespace, so every pass that looks for an empty cell looks for all of them the same way.
    /// </summary>
    private static string EmptyCellSql(string valueSql)
    {
        return $"({valueSql} IS NULL OR TRIM({valueSql}) = '')";
    }

    private static string NotEmptyCellSql(string valueSql)
    {
        return $"NOT {EmptyCellSql(valueSql)}";
    }

    private async Task TrimColumnNamesAsync(CancellationToken cancellationToken)
    {
        var columns = await _dbContext.Columns.ToListAsync(cancellationToken).ConfigureAwait(false);
        var changed = false;

        foreach (var column in columns)
        {
            var name = column.Name.Trim();

            // A name that is nothing but whitespace is left as it is: trimming it away would leave the
            // column unnamed, which is worse than a name with spaces in it.
            if (name.Length > 0 && name != column.Name)
            {
                column.Name = name;
                changed = true;
            }

            var defaultValue = column.DefaultValueForCell?.Trim();

            if (defaultValue != column.DefaultValueForCell)
            {
                column.DefaultValueForCell = defaultValue;
                changed = true;
            }
        }

        if (changed)
        {
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RenumberColumnsAsync(CancellationToken cancellationToken)
    {
        // The ordinal positions are the grid's column order, so deleting a column has to close the gap
        // it left rather than leaving the ordinals of the columns that follow it untouched.
        await _dbContext.Database
            .ExecuteSqlRawAsync("""
                 WITH Ordered AS (
                     SELECT
                         ID,
                         ROW_NUMBER() OVER (ORDER BY ORDINAL_POSITION) AS RN
                     FROM COLUMNS
                 )
                 UPDATE COLUMNS
                 SET ORDINAL_POSITION = (
                     SELECT RN
                     FROM Ordered
                     WHERE Ordered.ID = COLUMNS.ID
                 );
                 """, cancellationToken)
            .ConfigureAwait(false);
    }
}

