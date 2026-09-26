using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.ViewModels.Documents;

namespace TableConverter.Commands.Extensions;

/// <summary>
/// The store access the table data commands share: working out which table a command applies to, and
/// taking rows out of a store.
/// </summary>
internal static class TableDataCommandExtensions
{
    /// <summary>
    /// How many rows are removed per statement. A statement can only name so many parameters, and a table
    /// holds far more rows than that, so a large removal is cut into batches rather than attempted at once.
    /// </summary>
    private const int RowsPerDeleteBatch = 500;

    /// <summary>
    /// Resolves the table document a command applies to.
    /// </summary>
    /// <param name="context">The context the command is running in.</param>
    /// <param name="document">The selected table document, when there is one with a store.</param>
    /// <returns><see langword="true" /> when there is a table with a store to work on.</returns>
    /// <remarks>
    /// A document without a store is no use to a table command: everything the commands do is done to the
    /// store rather than to the grid, so there has to be a store to do it to.
    /// </remarks>
    public static bool TryGetTableDocument(this ICommandContext context, out TableDataViewModel document)
    {
        if (context.TryGetSelectedItem<TableDataViewModel>(out var selected)
            && !string.IsNullOrEmpty(selected.Path))
        {
            document = selected;
            return true;
        }

        document = null!;
        return false;
    }

    /// <summary>
    /// Works out which rows a set of positions in the table names.
    /// </summary>
    /// <param name="db">The store to read the rows from.</param>
    /// <param name="positions">The positions to resolve, counting from the first row.</param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    /// <returns>The ids of the rows at those positions, skipping any position the table does not reach.</returns>
    /// <remarks>
    /// The grid only reads the rows it has scrolled to, so a row the user has selected may not have been
    /// read yet and carries no id: only its position says which row it is. Ids are read in the table's own
    /// order - which is what the grid shows them in - so they can be matched to the positions by counting.
    /// Only as many ids as the furthest position asks for are read, so a selection near the top of a long
    /// table stays cheap.
    /// </remarks>
    public static async Task<IReadOnlyList<int>> GetRowIdsAtPositionsAsync(this TableStoreDbContext db,
        IReadOnlyCollection<int> positions,
        CancellationToken cancellationToken = default)
    {
        var wanted = positions.Where(position => position >= 0).Distinct().ToArray();

        if (wanted.Length == 0)
        {
            return [];
        }

        var ids = await db.Rows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .Select(row => row.Id)
            .Take(wanted.Max() + 1)
            .ToListAsync(cancellationToken);

        return [.. wanted.Where(position => position < ids.Count).Select(position => ids[position])];
    }

    /// <summary>
    /// Removes rows, and the cells that belong to them, from a store.
    /// </summary>
    /// <param name="db">The store to remove the rows from.</param>
    /// <param name="rowIds">The rows to remove.</param>
    /// <param name="cancellationToken">Token used to cancel the removal.</param>
    /// <returns>How many rows were removed.</returns>
    /// <remarks>
    /// The cells are removed explicitly rather than left to the cascade, so what a store ends up holding
    /// never depends on how the connection it was opened with happened to be configured.
    /// </remarks>
    public static async Task<int> DeleteRowsAsync(this TableStoreDbContext db,
        IReadOnlyCollection<int> rowIds,
        CancellationToken cancellationToken = default)
    {
        var deleted = 0;

        foreach (var batch in rowIds.Distinct().Chunk(RowsPerDeleteBatch))
        {
            var ids = batch.ToArray();

            await db.Cells
                .Where(cell => ids.Contains(cell.RowId))
                .ExecuteDeleteAsync(cancellationToken);

            deleted += await db.Rows
                .Where(row => ids.Contains(row.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        return deleted;
    }
}
