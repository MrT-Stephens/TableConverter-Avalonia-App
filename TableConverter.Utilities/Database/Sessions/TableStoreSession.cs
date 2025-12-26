using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models;

namespace TableConverter.Utilities.Database.Sessions;

public class TableStoreSession(TableStoreDbContext dbContext) : IDbSession<TableStoreDbContext>
{
    public TableStoreDbContext DbContext { get; } = dbContext;

    public async Task<ColumnEntity> AddColumnAsync(
        string name,
        int dataType,
        int? ordinal = null,
        bool autoSaveChanges = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Column name must be provided.", nameof(name));

        var nextOrdinal = ordinal ?? 
            (await DbContext.Columns.MaxAsync(c => (int?)c.Ordinal, ct) ?? -1) + 1;

        var entity = new ColumnEntity
        {
            Name = name,
            DataType = dataType,
            Ordinal = nextOrdinal
        };

        DbContext.Columns.Add(entity);

        if (autoSaveChanges)
            await DbContext.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<RowEntity> AddRowAsync(bool autoSaveChanges = true, CancellationToken ct = default)
    {
        var entity = new RowEntity();
        DbContext.Rows.Add(entity);

        if (autoSaveChanges)
            await DbContext.SaveChangesAsync(ct);

        return entity;
    }

    public async Task SetCellAsync(
        long rowId,
        long columnId,
        string? value,
        bool autoSaveChanges = true,
        CancellationToken ct = default)
    {
        var cell = await DbContext.Cells.FindAsync([rowId, columnId], ct);

        if (cell is null)
        {
            DbContext.Cells.Add(new CellEntity
            {
                RowId = rowId,
                ColumnId = columnId,
                Value = value
            });
        }
        else
        {
            cell.Value = value;
        }

        if (autoSaveChanges)
            await DbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteColumnAsync(long columnId, bool autoSaveChanges = true, CancellationToken ct = default)
    {
        var column = await DbContext.Columns.FindAsync([columnId], ct);
        if (column is null) return;

        DbContext.Columns.Remove(column);

        if (autoSaveChanges)
            await DbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteRowAsync(long rowId, bool autoSaveChanges = true, CancellationToken ct = default)
    {
        var row = await DbContext.Rows.FindAsync([rowId], ct);
        if (row is null) return;

        DbContext.Rows.Remove(row);

        if (autoSaveChanges)
            await DbContext.SaveChangesAsync(ct);
    }

    public async ValueTask DisposeAsync() => await DbContext.DisposeAsync();
}