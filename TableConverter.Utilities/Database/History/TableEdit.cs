using System.Text.Json.Serialization;
using TableConverter.Utilities.Database.Contexts;

namespace TableConverter.Utilities.Database.History;

/// <summary>
///     The kind of change a history entry made, so a history can be listed, filtered or grouped without
///     every payload having to be read.
/// </summary>
public enum TableEditKind
{
    Unknown = 0,
    RowsAdded = 1,
    RowsDeleted = 2,
    CellsChanged = 3,
    ColumnsChanged = 4,
    RowOrderChanged = 5,
    TableRotated = 6,
    TableReplaced = 7,
    Composite = 8,
}

/// <summary>
///     One step that can be taken back, or put in place again, on a table.
/// </summary>
/// <remarks>
///     <para>
///         Everything an entry needs in order to be replayed is the payload this describes, which is why
///         the payload is stored as text and not as a shape the table's schema has to know about. Adding a
///         kind of step to the history means writing one more record here and naming it in
///         <see cref="JsonDerivedTypeAttribute" /> below; nothing in the store, the table or the history
///         itself has to change for it.
///     </para>
///     <para>
///         A step holds the values it is about to change rather than the table around them, so the size of
///         an entry follows the size of the change rather than the size of the table.
///     </para>
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$edit")]
[JsonDerivedType(typeof(CellValuesEdit), "cells")]
[JsonDerivedType(typeof(RowsEdit), "rows")]
[JsonDerivedType(typeof(ColumnsEdit), "columns")]
[JsonDerivedType(typeof(RowOrderEdit), "rowOrder")]
[JsonDerivedType(typeof(RotationEdit), "rotation")]
[JsonDerivedType(typeof(TableReplaceEdit), "table")]
[JsonDerivedType(typeof(CompositeEdit), "composite")]
public abstract record TableEdit
{
    /// <summary>
    ///     The kind of change this step makes.
    /// </summary>
    [JsonIgnore]
    public abstract TableEditKind Kind { get; }

    /// <summary>
    ///     What the step did, in a form short enough to list.
    /// </summary>
    [JsonIgnore]
    public abstract string Summary { get; }

    /// <summary>
    ///     Puts the step in place again.
    /// </summary>
    public abstract Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Takes the step back, leaving the table as it was before the step was taken.
    /// </summary>
    public abstract Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default);
}

/// <summary>
///     A value that was put into a cell, and the value that was there before it.
/// </summary>
public sealed record CellChange(int RowId, int ColumnId, string? Before, string? After);

/// <summary>
///     Values put into cells that already existed.
/// </summary>
public sealed record CellValuesEdit : TableEdit
{
    public required List<CellChange> Changes { get; init; }

    [JsonIgnore]
    public override TableEditKind Kind => TableEditKind.CellsChanged;

    [JsonIgnore]
    public override string Summary => $"Changed {Changes.Count} cell(s)";

    public override Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        return TableEditWriter.WriteCellValuesAsync(
            db,
            [.. Changes.Select(change => new CellValue(change.RowId, change.ColumnId, change.After))],
            cancellationToken);
    }

    public override Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        return TableEditWriter.WriteCellValuesAsync(
            db,
            [.. Changes.Select(change => new CellValue(change.RowId, change.ColumnId, change.Before))],
            cancellationToken);
    }
}

/// <summary>
///     Rows that were added to the table, rows that were taken out of it, or both at once.
/// </summary>
/// <param name="Removed">The rows that were taken out, with the values and the ids they had.</param>
/// <param name="Added">The rows that were put in, with the values and the ids they were given.</param>
/// <remarks>
///     Both directions are held by the one record because taking a row out and putting a row in are the
///     same step read backwards, so an operation that does both - replacing a row, say - is recorded once
///     rather than as two entries that would have to be undone together.
/// </remarks>
public sealed record RowsEdit : TableEdit
{
    public required List<RowValues> Removed { get; init; }

    public required List<RowValues> Added { get; init; }

    [JsonIgnore]
    public override TableEditKind Kind => Added.Count > 0 && Removed.Count == 0
        ? TableEditKind.RowsAdded
        : TableEditKind.RowsDeleted;

    [JsonIgnore]
    public override string Summary => (Added.Count, Removed.Count) switch
    {
        (> 0, > 0) => $"Added {Added.Count} and removed {Removed.Count} row(s)",
        (> 0, _) => $"Added {Added.Count} row(s)",
        (_, > 0) => $"Removed {Removed.Count} row(s)",
        _ => "Changed rows",
    };

    public override async Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        if (Removed.Count > 0)
        {
            await TableEditWriter.DeleteRowsAsync(db, [.. Removed.Select(row => row.RowId)], cancellationToken)
                .ConfigureAwait(false);
        }

        if (Added.Count > 0)
        {
            await TableEditWriter.InsertRowsAsync(db, Added, cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        if (Added.Count > 0)
        {
            await TableEditWriter.DeleteRowsAsync(db, [.. Added.Select(row => row.RowId)], cancellationToken)
                .ConfigureAwait(false);
        }

        if (Removed.Count > 0)
        {
            await TableEditWriter.InsertRowsAsync(db, Removed, cancellationToken).ConfigureAwait(false);
        }
    }
}

/// <summary>
///     What the table's columns were and what they became.
/// </summary>
/// <param name="Before">Every column as it was, with the values of any column that was removed.</param>
/// <param name="After">Every column as it became, with the values of any column that was added.</param>
public sealed record ColumnsEdit : TableEdit
{
    public required List<ColumnValues> Before { get; init; }

    public required List<ColumnValues> After { get; init; }

    [JsonIgnore]
    public override TableEditKind Kind => TableEditKind.ColumnsChanged;

    [JsonIgnore]
    public override string Summary => After.Count >= Before.Count
        ? $"Added {After.Count - Before.Count} column(s)"
        : $"Removed {Before.Count - After.Count} column(s)";

    public override Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        return TableEditWriter.WriteColumnsAsync(db, After, cancellationToken);
    }

    public override Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        return TableEditWriter.WriteColumnsAsync(db, Before, cancellationToken);
    }
}

/// <summary>
///     The order the rows were in, and the order they were put in.
/// </summary>
public sealed record RowOrderEdit : TableEdit
{
    public required List<int> Before { get; init; }

    public required List<int> After { get; init; }

    [JsonIgnore]
    public override TableEditKind Kind => TableEditKind.RowOrderChanged;

    [JsonIgnore]
    public override string Summary => "Reordered rows";

    public override Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        // The rows that are in the "before" order now have to be in the "after" order, so the order they
        // are currently in is the one the pair is read from.
        return TableEditWriter.ReorderRowsAsync(db, Before, After, cancellationToken);
    }

    public override Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        return TableEditWriter.ReorderRowsAsync(db, After, Before, cancellationToken);
    }
}

/// <summary>
///     A quarter turn of the whole table.
/// </summary>
/// <remarks>
///     A turn is its own record rather than a snapshot of the table it produces, because turning a table
///     one way and then the other leaves it exactly as it was, so the direction alone is enough to take
///     the step back and putting a copy of the table aside would cost far more than it is worth.
/// </remarks>
public sealed record RotationEdit : TableEdit
{
    public required TableRotation Direction { get; init; }

    [JsonIgnore]
    public override TableEditKind Kind => TableEditKind.TableRotated;

    [JsonIgnore]
    public override string Summary => $"Transposed {Direction}";

    public override async Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        await TableStoreMaintenance.Create(db).RotateAsync(Direction, cancellationToken).ConfigureAwait(false);
    }

    public override async Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        var inverse = Direction is TableRotation.Clockwise
            ? TableRotation.CounterClockwise
            : TableRotation.Clockwise;

        await TableStoreMaintenance.Create(db).RotateAsync(inverse, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
///     The whole of the table, as it was and as it became.
/// </summary>
/// <remarks>
///     This is the one kind of entry whose size follows the size of the table rather than the size of the
///     change, so it is only recorded by the operations that genuinely replace a table.
/// </remarks>
public sealed record TableReplaceEdit : TableEdit
{
    public required TableSnapshot Before { get; init; }

    public required TableSnapshot After { get; init; }

    [JsonIgnore]
    public override TableEditKind Kind => TableEditKind.TableReplaced;

    [JsonIgnore]
    public override string Summary => $"Replaced the table ({After.Rows.Count} row(s))";

    public override Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        return TableEditWriter.WriteTableAsync(db, After, cancellationToken);
    }

    public override Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        return TableEditWriter.WriteTableAsync(db, Before, cancellationToken);
    }
}

/// <summary>
///     Several steps that were taken as one.
/// </summary>
/// <remarks>
///     An operation rarely changes only one thing: cleaning a table up renames a column and changes the
///     values under it, and the user thinks of that as one step they want to be able to take back. The
///     steps are applied in the order they were taken and taken back in the order they were taken, so the
///     ones that depend on each other still work.
/// </remarks>
public sealed record CompositeEdit : TableEdit
{
    public required List<TableEdit> Edits { get; init; }

    [JsonIgnore]
    public override TableEditKind Kind => TableEditKind.Composite;

    [JsonIgnore]
    public override string Summary => string.Join(", ", Edits.Select(edit => edit.Summary));

    public override async Task ApplyAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var edit in Edits)
        {
            await edit.ApplyAsync(db, cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task RevertAsync(TableStoreDbContext db, CancellationToken cancellationToken = default)
    {
        for (var index = Edits.Count - 1; index >= 0; index--)
        {
            await Edits[index].RevertAsync(db, cancellationToken).ConfigureAwait(false);
        }
    }
}

