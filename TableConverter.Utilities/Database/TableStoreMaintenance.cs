using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;

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
    ///     Separates the cells of a row in the signature that rows are grouped by when duplicates are
    ///     looked for. A unit separator is used rather than a printable character because a stored value
    ///     can contain any printable one, which would let two different rows share a signature.
    /// </summary>
    private const string SignatureSeparatorSql = "CHAR(31)";

    /// <summary>
    ///     How many rows a rotation writes per change set, so turning a table that is very wide does not
    ///     have to hold the whole of its other direction as one change.
    /// </summary>
    private const int RowsPerBatch = 250;

    private static readonly string TrimSql = $"""
        UPDATE CELLS
        SET VALUE = TRIM(VALUE, {WhitespaceSql})
        WHERE VALUE IS NOT NULL
          AND TRIM(VALUE, {WhitespaceSql}) <> VALUE;
        """;

    /// <summary>
    ///     Reads each row's cells back as one value, which is what two rows are compared by. A row with no
    ///     cells at all and a cell that holds nothing both read as nothing, so the rows of a table that
    ///     holds nothing all read as the one repeated signature.
    /// </summary>
    private static readonly string DuplicateSignaturesSql = $"""
        SELECT
            R.ID AS ROW_ID,
            COALESCE((
                SELECT GROUP_CONCAT(COALESCE(C.VALUE, ''), {SignatureSeparatorSql})
                FROM (
                    SELECT VALUE
                    FROM CELLS
                    WHERE ROW_ID = R.ID
                    ORDER BY COLUMN_ID
                ) AS C
            ), '') AS SIGNATURE
        FROM ROWS R
        """;

    /// <summary>
    ///     Keeps the lowest row id of every group of signatures, so the first of a set of duplicates is the
    ///     copy that survives, which is the row a user reading top to bottom would keep.
    /// </summary>
    private static readonly string KeepFirstOfEachSignatureSql = $"""
        WITH Signatures AS (
            {DuplicateSignaturesSql}
        ),
        Keepers AS (
            SELECT MIN(ROW_ID) AS ROW_ID
            FROM Signatures
            GROUP BY SIGNATURE
        )
        """;

    private static readonly string RemoveDuplicateRowsSql = $"""
        {KeepFirstOfEachSignatureSql}
        DELETE FROM ROWS
        WHERE ID NOT IN (SELECT ROW_ID FROM Keepers);
        """;

    /// <summary>
    ///     Counts the rows <see cref="RemoveDuplicateRowsSql" /> deletes, so what a user is told they are
    ///     about to lose is worked out the same way as what they lose.
    /// </summary>
    private static readonly string CountDuplicateRowsSql = $"""
        {KeepFirstOfEachSignatureSql}
        SELECT COUNT(*) AS Value
        FROM Signatures
        WHERE ROW_ID NOT IN (SELECT ROW_ID FROM Keepers);
        """;

    private static readonly string FillEmptyCellsSql = $"""
        UPDATE CELLS
        SET VALUE = (
            SELECT C.DEFAULT_VALUE_FOR_CELL
            FROM COLUMNS C
            WHERE C.ID = CELLS.COLUMN_ID
        )
        WHERE {EmptyCellSql("VALUE")}
          AND COALESCE((
              SELECT C.DEFAULT_VALUE_FOR_CELL
              FROM COLUMNS C
              WHERE C.ID = CELLS.COLUMN_ID
          ), '') <> '';
        """;

    private static readonly string RemoveEmptyRowsSql = $"""
        DELETE FROM ROWS
        WHERE NOT EXISTS (
            SELECT 1
            FROM CELLS
            WHERE CELLS.ROW_ID = ROWS.ID
              AND {NotEmptyCellSql("CELLS.VALUE")}
        );
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
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            // A value that is nothing but whitespace is trimmed to nothing rather than left holding the
            // whitespace, which is what lets the passes that treat an empty cell as a missing one see it.
            var trimmedCells = await _dbContext.Database
                .ExecuteSqlRawAsync(TrimSql, cancellationToken)
                .ConfigureAwait(false);

            await TrimColumnNamesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return trimmedCells;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
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
        // The count is aliased to the name the provider reads a scalar result from, so the one row the
        // statement produces comes back as the number.
        return await _dbContext.Database
            .SqlQueryRaw<int>(CountDuplicateRowsSql)
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Deletes every row that repeats a row above it, keeping the first of each set.
    /// </summary>
    /// <returns>The number of rows that were deleted.</returns>
    public async Task<int> RemoveDuplicateRowsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database
            .ExecuteSqlRawAsync(RemoveDuplicateRowsSql, cancellationToken)
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
        var emptyColumnIds = await _dbContext.Columns
            .AsNoTracking()
            .Where(column => !column.Cells.Any(cell => cell.Value != null && cell.Value.Trim() != ""))
            .Select(column => column.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (emptyColumnIds.Count == 0)
        {
            return 0;
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var columns = await _dbContext.Columns
                .Where(column => emptyColumnIds.Contains(column.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            _dbContext.Columns.RemoveRange(columns);

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await RenumberColumnsAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return columns.Count;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
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

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
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
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        return new TableShape(turnedRowCount, turnedColumnCount);
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

