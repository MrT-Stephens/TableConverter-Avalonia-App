using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Database.History;

/// <summary>
///     Keeps the edits made to a table store, in the store itself, so the table can be walked back and
///     forwards over them.
/// </summary>
/// <remarks>
///     <para>
///         The history is stored in the same file as the table it describes rather than beside it in memory,
///         because a step holds a copy of the values it is about to change: that belongs on disk with the
///         table, not in the memory of whichever document happens to have the table open.
///     </para>
///     <para>
///         Every write opens its own short lived context from the factory, so the history never shares a
///         change tracker with the operation it is recording - an operation that replaces the whole table
///         would otherwise leave the step describing it holding on to the table it just removed.
///     </para>
/// </remarks>
public sealed class TableStoreHistory(
    ITableStoreDbContextFactory dbContextFactory,
    ILogger<TableStoreHistory> logger,
    int maximumEntries = TableStoreHistory.DefaultMaximumEntries)
    : ITableHistory
{
    /// <summary>
    ///     How many steps are kept when the caller does not say.
    /// </summary>
    public const int DefaultMaximumEntries = 200;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);

    private readonly ITableStoreDbContextFactory _dbContextFactory = dbContextFactory;
    private readonly ILogger<TableStoreHistory> _logger = logger;
    private readonly int _maximumEntries = Math.Max(1, maximumEntries);

    /// <inheritdoc />
    public event EventHandler<HistoryChangedEventArgs>? Changed;

    /// <inheritdoc />
    public int MaximumEntries => _maximumEntries;

    /// <inheritdoc />
    public ITableEditScope BeginEdit(string path, TableEditKind kind, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new TableEditScope(this, path, kind, description);
    }

    /// <inheritdoc />
    public async Task<bool> CanUndoAsync(string path, CancellationToken cancellationToken = default)
    {
        return await GetUndoDepthAsync(path, cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> CanRedoAsync(string path, CancellationToken cancellationToken = default)
    {
        return await GetRedoDepthAsync(path, cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public async Task<int> GetUndoDepthAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(path, cancellationToken)
            .ConfigureAwait(false);

        return await Entries(db).CountAsync(entry => entry.IsApplied, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> GetRedoDepthAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(path, cancellationToken)
            .ConfigureAwait(false);

        return await Entries(db).CountAsync(entry => !entry.IsApplied, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<HistoryEntry>> GetEntriesAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(path, cancellationToken)
            .ConfigureAwait(false);

        var entries = await Entries(db)
            .AsNoTracking()
            .OrderBy(entry => entry.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. entries.Select(ToEntry)];
    }

    /// <inheritdoc />
    public Task<HistoryEntry?> UndoAsync(string path, CancellationToken cancellationToken = default)
    {
        return MoveAsync(path, applied: true, cancellationToken);
    }

    /// <inheritdoc />
    public Task<HistoryEntry?> RedoAsync(string path, CancellationToken cancellationToken = default)
    {
        return MoveAsync(path, applied: false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ClearAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(path, cancellationToken)
            .ConfigureAwait(false);

        var entries = await Entries(db).ToListAsync(cancellationToken).ConfigureAwait(false);

        if (entries.Count == 0)
        {
            return;
        }

        Entries(db).RemoveRange(entries);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Changed?.Invoke(this, new HistoryChangedEventArgs(path, HistoryAction.Cleared, null));
    }

    /// <summary>
    ///     Writes a step to the history, throwing away whatever could be redone before it.
    /// </summary>
    internal async Task<HistoryEntry> RecordAsync(
        string path,
        TableEdit edit,
        string description,
        TableEditKind kind,
        CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(path, cancellationToken)
            .ConfigureAwait(false);

        var recorded = await db.Database.RunAsync(async () =>
        {
            var entries = Entries(db);

            // A redo is only meaningful while the table is still where the step left it. Taking a new step
            // moves the table on, so the steps that could have been redone are dropped rather than left to
            // be applied to a table that has gone a different way.
            var discarded = await entries
                .Where(entry => !entry.IsApplied)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (discarded.Count > 0)
            {
                entries.RemoveRange(discarded);
            }

            // The sequence is read from the store before the discarded steps are saved away, so it keeps
            // rising as the table is worked on and a step never sorts itself in front of an older one.
            var lastSequence = await entries
                .OrderByDescending(entry => entry.Sequence)
                .Select(entry => (int?)entry.Sequence)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false) ?? 0;

            var entity = new HistoryEntryEntity
            {
                Sequence = lastSequence + 1,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),

                // The kind is the one the operation declared rather than the one the payload reports: an
                // operation is what the user did, and a step holding several payloads has no one payload
                // kind to be named by. A caller that does not say falls back to what the payload says.
                Kind = (kind is TableEditKind.Unknown ? edit.Kind : kind).ToString(),
                Description = description,
                Payload = JsonSerializer.Serialize(edit, SerializerOptions),
                IsApplied = true,
            };

            entries.Add(entity);

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await TrimAsync(db, cancellationToken).ConfigureAwait(false);

            return ToEntry(entity);
        }, cancellationToken).ConfigureAwait(false);

        Changed?.Invoke(this, new HistoryChangedEventArgs(path, HistoryAction.Recorded, recorded));

        return recorded;
    }

    /// <summary>
    ///     Takes the newest applied step back, or puts the oldest step that is not applied in place again.
    /// </summary>
    private async Task<HistoryEntry?> MoveAsync(
        string path,
        bool applied,
        CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(path, cancellationToken)
            .ConfigureAwait(false);

        var entries = Entries(db);

        var entity = applied
            ? await entries.Where(entry => entry.IsApplied)
                .OrderByDescending(entry => entry.Sequence)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false)
            : await entries.Where(entry => !entry.IsApplied)
                .OrderBy(entry => entry.Sequence)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        var edit = Deserialize(entity.Payload);

        if (edit is null)
        {
            // A step that cannot be read cannot be moved over either, so the history stops here rather than
            // leaving the cursor pointing at a step that is neither done nor undone.
            _logger.LogWarning(
                "The history step {0} of the store at {1} could not be read and cannot be replayed.",
                entity.Sequence, path);

            return null;
        }

        var moved = await db.Database.RunAsync(async () =>
        {
            if (applied)
            {
                await edit.RevertAsync(db, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await edit.ApplyAsync(db, cancellationToken).ConfigureAwait(false);
            }

            // The cursor moves only once the table has actually moved, so a step that failed part way is
            // left where it was rather than recorded as having been undone.
            entity.IsApplied = !applied;

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return ToEntry(entity);
        }, cancellationToken).ConfigureAwait(false);

        if (moved is not null)
        {
            Changed?.Invoke(this, new HistoryChangedEventArgs(
                path,
                applied ? HistoryAction.Undone : HistoryAction.Redone,
                moved));
        }

        return moved;
    }

    /// <summary>
    ///     Drops the oldest steps once the history is longer than the store is asked to keep.
    /// </summary>
    private async Task TrimAsync(TableStoreDbContext db, CancellationToken cancellationToken)
    {
        var entries = Entries(db);

        var excess = await entries.CountAsync(cancellationToken).ConfigureAwait(false) - _maximumEntries;

        if (excess <= 0)
        {
            return;
        }

        // The oldest steps are all applied ones: undoing marks steps from the newest end, so the steps
        // that are not applied are the newest rather than the oldest.
        var oldest = await entries
            .OrderBy(entry => entry.Sequence)
            .Take(excess)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        entries.RemoveRange(oldest);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static DbSet<HistoryEntryEntity> Entries(TableStoreDbContext db) => db.History;

    private static HistoryEntry ToEntry(HistoryEntryEntity entity)
    {
        return new HistoryEntry(
            entity.Sequence,
            DateTimeOffset.FromUnixTimeSeconds(entity.Timestamp),
            entity.Description,
            Enum.TryParse<TableEditKind>(entity.Kind, out var kind) ? kind : TableEditKind.Unknown,
            entity.IsApplied);
    }

    private static TableEdit? Deserialize(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<TableEdit>(payload, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    ///     One operation being recorded.
    /// </summary>
    private sealed class TableEditScope(
        TableStoreHistory history,
        string path,
        TableEditKind kind,
        string description)
        : ITableEditScope
    {
        private readonly Dictionary<TableRegion, TableRegionState> _before = [];
        private readonly Dictionary<TableRegion, TableRegionState> _after = [];
        private bool _committed;

        public async Task CaptureBeforeAsync(
            TableRegion region,
            CancellationToken cancellationToken = default)
        {
            _before[region] = await ReadAsync(region, cancellationToken).ConfigureAwait(false);
        }

        public async Task CaptureAfterAsync(
            TableRegion region,
            CancellationToken cancellationToken = default)
        {
            _after[region] = await ReadAsync(region, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HistoryEntry?> CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_committed)
            {
                return null;
            }

            _committed = true;

            if (_before.Count == 0 && _after.Count == 0)
            {
                return null;
            }

            var edits = new List<TableEdit>();

            if (_before.Count > 0)
            {
                await using var db = await history._dbContextFactory
                    .CreateDbContextAsync(path, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var (region, before) in _before)
                {
                    var after = await TableRegionCapture.CaptureAsync(db, region, cancellationToken)
                        .ConfigureAwait(false);

                    if (TableRegionCapture.Diff(region, before, after) is { } edit)
                    {
                        edits.Add(edit);
                    }
                }
            }

            // A region captured after the operation had nothing in front of it, so what it holds now is the
            // whole of what the step did to that region.
            foreach (var (region, after) in _after)
            {
                if (TableRegionCapture.Diff(region, new TableRegionState(), after) is { } edit)
                {
                    edits.Add(edit);
                }
            }

            if (edits.Count == 0)
            {
                return null;
            }

            var combined = edits.Count == 1
                ? edits[0]
                : new CompositeEdit { Edits = edits };

            // Filling a table that held nothing is the one step that is never recorded. Taking it back
            // would only return the table to empty, and keeping the whole of a newly imported table in the
            // history would double the size of the store to buy a step nobody wants.
            if (IsFirstFill(combined))
            {
                return null;
            }

            return await history.RecordAsync(path, combined, description, kind, cancellationToken)
                .ConfigureAwait(false);
        }

        public ValueTask DisposeAsync()
        {
            // A recording that was never committed describes an operation that did not happen, so there is
            // nothing to release and nothing to write.
            _before.Clear();
            _after.Clear();

            return ValueTask.CompletedTask;
        }

        private async Task<TableRegionState> ReadAsync(TableRegion region, CancellationToken cancellationToken)
        {
            await using var db = await history._dbContextFactory
                .CreateDbContextAsync(path, cancellationToken)
                .ConfigureAwait(false);

            return await TableRegionCapture.CaptureAsync(db, region, cancellationToken).ConfigureAwait(false);
        }

        private static bool IsFirstFill(TableEdit edit)
        {
            return edit switch
            {
                TableReplaceEdit replace => replace.Before.Columns.Count == 0 && replace.Before.Rows.Count == 0,
                CompositeEdit composite => composite.Edits.All(IsFirstFill),
                _ => false,
            };
        }
    }
}

