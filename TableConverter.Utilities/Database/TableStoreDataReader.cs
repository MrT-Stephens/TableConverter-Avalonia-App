using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Database;

/// <summary>
/// Reads the whole contents of a table store back into a <see cref="TableData" />.
/// </summary>
/// <remarks>
/// The table is loaded in full because the file converters write a whole table at a time. Callers
/// that only need part of a table (the grid, search) should query the store directly instead.
/// </remarks>
public static class TableStoreDataReader
{
    /// <summary>
    /// Reads the columns and rows of the store behind <paramref name="dbContext" />.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to read.</param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    /// <returns>
    /// The stored table, with a cell per column on every row. A cell that was stored as NULL comes
    /// back as <see langword="null" />.
    /// </returns>
    public static async Task<TableData> ReadAsync(
        TableStoreDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var columns = await dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var headers = columns.ConvertAll(column => column.Name);

        // A cell is positioned by the ordinal of its column rather than by its id, because a column
        // can be deleted and re-added, which leaves gaps in the ids.
        var indexByColumnId = new Dictionary<int, int>(columns.Count);

        for (var index = 0; index < columns.Count; index++)
        {
            indexByColumnId[columns[index].Id] = index;
        }

        var entities = await dbContext.Rows
            .AsNoTracking()
            .Include(row => row.Cells)
            .OrderBy(row => row.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rows = new List<string[]>(entities.Count);

        foreach (var entity in entities)
        {
            var cells = new string[columns.Count];

            foreach (var cell in entity.Cells)
            {
                if (indexByColumnId.TryGetValue(cell.ColumnId, out var index))
                {
                    // The model does not mark the value nullable, but a store write does.
                    cells[index] = cell.Value!;
                }
            }

            rows.Add(cells);
        }

        return new TableData(headers, rows);
    }
}

