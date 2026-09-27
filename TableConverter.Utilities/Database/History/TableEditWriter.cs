using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Database.History;

/// <summary>
///     The writes a history entry is replayed with.
/// </summary>
/// <remarks>
///     <para>
///         Every write a table can be changed by is expressed here once, so an entry that has to take a
///         step back and an entry that has to put one in place again both go through the same code and can
///         never disagree about what a step means.
///     </para>
///     <para>
///         The writes go through the change tracker where the number of rows involved is the number the
///         entry remembered, and use statements where a step can touch as many rows as the table has. The
///         store is opened by whoever asked for the entry to be replayed, so a write never commits a
///         transaction it did not open and never disposes a context it did not create.
///     </para>
/// </remarks>
internal static class TableEditWriter
{
    /// <summary>
    ///     How many rows are written per change set, so replaying a step that touched a whole table does
    ///     not have to hold the whole of it as one change.
    /// </summary>
    private const int RowsPerBatch = 500;

    /// <summary>
    ///     Writes the given value into each of the given cells, adding a cell that is not there.
    /// </summary>
    public static async Task WriteCellValuesAsync(
        TableStoreDbContext db,
        IReadOnlyCollection<CellValue> values,
        CancellationToken cancellationToken = default)
    {
        foreach (var batch in values.GroupBy(value => value.RowId).Chunk(RowsPerBatch))
        {
            var rowIds = batch.Select(group => group.Key).ToArray();

            var cells = await db.Cells
                .Where(cell => rowIds.Contains(cell.RowId))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var existing = cells.ToDictionary(cell => (cell.RowId, cell.ColumnId));

            foreach (var value in batch.SelectMany(group => group))
            {
                if (existing.TryGetValue((value.RowId, value.ColumnId), out var cell))
                {
                    cell.Value = value.Value;
                    continue;
                }

                // A cell the table no longer holds is put back rather than skipped, because the step being
                // taken back may be the one that removed it.
                db.Cells.Add(new CellEntity
                {
                    RowId = value.RowId,
                    ColumnId = value.ColumnId,
                    Value = value.Value,
                });
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Puts the given rows back, each with the values it held and under the id it held them under.
    /// </summary>
    /// <remarks>
    ///     The ids are written rather than left to the store because a row's id is the order the grid shows
    ///     it in, so a row put back under a new id would come back in the wrong place.
    /// </remarks>
    public static async Task InsertRowsAsync(
        TableStoreDbContext db,
        IReadOnlyCollection<RowValues> rows,
        CancellationToken cancellationToken = default)
    {
        foreach (var batch in rows.Chunk(RowsPerBatch))
        {
            foreach (var row in batch)
            {
                var entity = new RowEntity { Id = row.RowId };

                foreach (var (columnId, value) in row.Cells)
                {
                    entity.Cells.Add(new CellEntity
                    {
                        ColumnId = columnId,
                        Value = value,
                    });
                }

                db.Rows.Add(entity);
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Takes the given rows, and the cells that belong to them, out of the table.
    /// </summary>
    public static async Task DeleteRowsAsync(
        TableStoreDbContext db,
        IReadOnlyCollection<int> rowIds,
        CancellationToken cancellationToken = default)
    {
        foreach (var batch in rowIds.Distinct().Chunk(RowsPerBatch))
        {
            var ids = batch;

            // The cells are removed explicitly rather than left to the cascade, so what the table ends up
            // holding never depends on how the connection it was opened with happened to be configured.
            if (db.Database.IsRelational())
            {
                await db.Cells
                    .Where(cell => ids.Contains(cell.RowId))
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);

                await db.Rows
                    .Where(row => ids.Contains(row.Id))
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);

                continue;
            }

            db.Cells.RemoveRange(await db.Cells
                .Where(cell => ids.Contains(cell.RowId))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false));

            db.Rows.RemoveRange(await db.Rows
                .Where(row => ids.Contains(row.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false));

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Makes the table's columns, and the values of the ones that were captured, match the given
    ///     snapshot.
    /// </summary>
    /// <remarks>
    ///     The whole set of columns is written rather than only the ones that changed, because a column
    ///     being removed closes the gap it leaves in the ordinal positions, which moves the columns that
    ///     followed it.
    /// </remarks>
    public static async Task WriteColumnsAsync(
        TableStoreDbContext db,
        IReadOnlyList<ColumnValues> columns,
        CancellationToken cancellationToken = default)
    {
        await WriteColumnDefinitionsAsync(db, columns, cancellationToken).ConfigureAwait(false);

        foreach (var column in columns.Where(column => column.Cells is not null))
        {
            await WriteColumnCellsAsync(db, column, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Makes the table's columns match the given snapshot, leaving the values under them alone.
    /// </summary>
    /// <remarks>
    ///     Writing the values under a column needs the rows they belong to to be there already, so an
    ///     operation that replaces the whole table writes the columns first and the rows after them, and
    ///     leaves the values to the rows that hold them.
    /// </remarks>
    public static async Task WriteColumnDefinitionsAsync(
        TableStoreDbContext db,
        IReadOnlyList<ColumnValues> columns,
        CancellationToken cancellationToken = default)
    {
        var targetIds = columns.Select(column => column.ColumnId).ToHashSet();

        var currentColumns = await db.Columns.ToListAsync(cancellationToken).ConfigureAwait(false);

        var removed = currentColumns.Where(column => !targetIds.Contains(column.Id)).ToList();

        if (removed.Count > 0)
        {
            db.Columns.RemoveRange(removed);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var existing = currentColumns
            .Where(column => targetIds.Contains(column.Id))
            .ToDictionary(column => column.Id);

        foreach (var column in columns)
        {
            if (existing.TryGetValue(column.ColumnId, out var entity))
            {
                entity.Name = column.Name;
                entity.DataType = column.DataType;
                entity.DefaultValueForCell = column.DefaultValueForCell;
                entity.OrdinalPosition = column.OrdinalPosition;
                continue;
            }

            db.Columns.Add(new ColumnEntity
            {
                Id = column.ColumnId,
                Name = column.Name,
                DataType = column.DataType,
                DefaultValueForCell = column.DefaultValueForCell,
                OrdinalPosition = column.OrdinalPosition,
            });
        }

        // Saved before any cell is written, so the cells have the columns they are written against.
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Puts the rows of the table in the given order.
    /// </summary>
    /// <param name="db">The store to reorder.</param>
    /// <param name="before">The ids of the rows in the order they were in.</param>
    /// <param name="after">The ids of the rows in the order they are to be in.</param>
    /// <param name="cancellationToken">Token used to cancel the write.</param>
    /// <remarks>
    ///     The order the store keeps its rows in is their ids, so putting them in a different order means
    ///     renumbering them, and a renumbering cannot be done a row at a time without two rows colliding
    ///     over the same id part way through. The rows are therefore read into a temporary table, the table
    ///     is emptied, and the rows are written back under their new ids - which is how the sort that this
    ///     takes back works in the first place.
    /// </remarks>
    public static async Task ReorderRowsAsync(
        TableStoreDbContext db,
        IReadOnlyList<int> before,
        IReadOnlyList<int> after,
        CancellationToken cancellationToken = default)
    {
        if (!db.Database.IsRelational())
        {
            throw new NotSupportedException(
                "Putting the rows of a table back in a different order needs statements, which this " +
                "provider does not support.");
        }

        if (before.Count != after.Count)
        {
            throw new ArgumentException("The two orders must hold the same rows.", nameof(after));
        }

        await db.Database.ExecuteSqlRawAsync(
            "DROP TABLE IF EXISTS T_REORDER_MAP; DROP TABLE IF EXISTS T_REORDERED_CELLS;",
            cancellationToken).ConfigureAwait(false);

        await db.Database.ExecuteSqlRawAsync(
            "CREATE TEMP TABLE T_REORDER_MAP (CURRENT_ID INTEGER PRIMARY KEY, TARGET_ID INTEGER NOT NULL);",
            cancellationToken).ConfigureAwait(false);

        foreach (var batch in before.Zip(after).Chunk(RowsPerBatch))
        {
            // The values are row ids that were read out of the store, so there is nothing in them that
            // could be read as anything but a number.
#pragma warning disable EF1002
            var pairs = string.Join(",", batch.Select(pair => $"({pair.First},{pair.Second})"));

            await db.Database.ExecuteSqlRawAsync(
                $"INSERT INTO T_REORDER_MAP (CURRENT_ID, TARGET_ID) VALUES {pairs};",
                cancellationToken).ConfigureAwait(false);
#pragma warning restore EF1002
        }

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TEMP TABLE T_REORDERED_CELLS AS
            SELECT O.TARGET_ID AS ROW_ID, C.COLUMN_ID, C.VALUE
            FROM CELLS C
            JOIN T_REORDER_MAP O ON O.CURRENT_ID = C.ROW_ID;

            DELETE FROM CELLS;
            DELETE FROM ROWS;

            INSERT INTO ROWS (ID) SELECT TARGET_ID FROM T_REORDER_MAP ORDER BY TARGET_ID;

            INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE)
            SELECT ROW_ID, COLUMN_ID, VALUE FROM T_REORDERED_CELLS ORDER BY ROW_ID, COLUMN_ID;

            DROP TABLE T_REORDERED_CELLS;
            DROP TABLE T_REORDER_MAP;
            """, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Empties the table of its columns, its rows and its cells.
    /// </summary>
    public static async Task ClearAsync(
        TableStoreDbContext db,
        CancellationToken cancellationToken = default)
    {
        if (db.Database.IsRelational())
        {
            await db.Cells.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            await db.Rows.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            await db.Columns.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        db.Cells.RemoveRange(await db.Cells.ToListAsync(cancellationToken).ConfigureAwait(false));
        db.Rows.RemoveRange(await db.Rows.ToListAsync(cancellationToken).ConfigureAwait(false));
        db.Columns.RemoveRange(await db.Columns.ToListAsync(cancellationToken).ConfigureAwait(false));

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Replaces the whole of the table with the given snapshot.
    /// </summary>
    public static async Task WriteTableAsync(
        TableStoreDbContext db,
        TableSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        await ClearAsync(db, cancellationToken).ConfigureAwait(false);

        // The columns are written without their values, because the values belong to the rows and the
        // rows are not there yet: a cell naming a row the table does not hold would be a cell the
        // foreign key refuses.
        await WriteColumnDefinitionsAsync(db, snapshot.Columns, cancellationToken).ConfigureAwait(false);

        foreach (var batch in snapshot.Rows.Chunk(RowsPerBatch))
        {
            foreach (var row in batch)
            {
                var entity = new RowEntity { Id = row.RowId };

                foreach (var (columnId, value) in row.Cells)
                {
                    entity.Cells.Add(new CellEntity
                    {
                        ColumnId = columnId,
                        Value = value,
                    });
                }

                db.Rows.Add(entity);
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WriteColumnCellsAsync(
        TableStoreDbContext db,
        ColumnValues column,
        CancellationToken cancellationToken)
    {
        var cells = column.Cells ?? [];

        await db.Cells
            .Where(cell => cell.ColumnId == column.ColumnId)
            .ExecuteDeleteIfSupportedAsync(db, cancellationToken)
            .ConfigureAwait(false);

        foreach (var batch in cells.Chunk(RowsPerBatch))
        {
            foreach (var (rowId, value) in batch)
            {
                db.Cells.Add(new CellEntity
                {
                    RowId = rowId,
                    ColumnId = column.ColumnId,
                    Value = value,
                });
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Removes the cells of one column, using a statement where the provider has them and the change
    ///     tracker where it does not.
    /// </summary>
    private static async Task ExecuteDeleteIfSupportedAsync(
        this IQueryable<CellEntity> cells,
        TableStoreDbContext db,
        CancellationToken cancellationToken)
    {
        if (db.Database.IsRelational())
        {
            await cells.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        db.Cells.RemoveRange(await cells.ToListAsync(cancellationToken).ConfigureAwait(false));
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

