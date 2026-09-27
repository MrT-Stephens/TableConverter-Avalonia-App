namespace TableConverter.Utilities.Database.Models.TableStore;

/// <summary>
///     One step in a store's edit history: what was done to the table, and everything needed to take
///     that step back or to put it in place again.
/// </summary>
/// <remarks>
///     <para>
///         An entry is a <em>description</em> plus a <em>payload</em>. The description is what the history
///         is read by, and the payload is the serialized <see cref="History.TableEdit" /> which knows how
///         to undo and redo itself. Nothing about a particular kind of edit is understood here, which is
///         what lets a new kind of edit be added without this table changing.
///     </para>
///     <para>
///         <see cref="IsApplied" /> is the cursor. Everything up to it has been done to the table and
///         everything after it has been undone, so undoing walks backwards over the applied entries and
///         redoing walks forwards over the rest. Making a new edit throws away the entries after the
///         cursor, which is what stops a redo from putting back a step the table has moved on from.
///     </para>
/// </remarks>
public sealed class HistoryEntryEntity : EntityBase<int>
{
    private string _description = string.Empty;
    private bool _isApplied;
    private string _kind = string.Empty;
    private string _payload = string.Empty;
    private int _sequence;
    private long _timestamp;

    /// <summary>
    ///     Where the step sits in the store's history, counting from the first step taken.
    /// </summary>
    /// <remarks>
    ///     The order steps are applied in is this rather than <see cref="EntityBase{T}.Id" />, because a
    ///     step that is recorded after earlier ones have been undone has to sort after them even though
    ///     its id is higher than the whole of the history.
    /// </remarks>
    public int Sequence
    {
        get => _sequence;
        set => SetField(ref _sequence, value);
    }

    /// <summary>
    ///     When the step was taken, as the number of seconds since the Unix epoch.
    /// </summary>
    /// <remarks>
    ///     Stored as a number rather than as a date because SQLite has no date type and every provider
    ///     agrees on what an integer is.
    /// </remarks>
    public long Timestamp
    {
        get => _timestamp;
        set => SetField(ref _timestamp, value);
    }

    /// <summary>
    ///     What the step did, in a form short enough to list.
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>
    ///     The kind of edit the payload holds, so a history can be filtered or grouped without every
    ///     payload being deserialized.
    /// </summary>
    public string Kind
    {
        get => _kind;
        set => SetField(ref _kind, value);
    }

    /// <summary>
    ///     The serialized <see cref="History.TableEdit" /> that takes the step back and puts it in place
    ///     again.
    /// </summary>
    public string Payload
    {
        get => _payload;
        set => SetField(ref _payload, value);
    }

    /// <summary>
    ///     Whether the step is currently done to the table.
    /// </summary>
    public bool IsApplied
    {
        get => _isApplied;
        set => SetField(ref _isApplied, value);
    }
}

