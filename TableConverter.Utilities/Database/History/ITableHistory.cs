namespace TableConverter.Utilities.Database.History;

/// <summary>
///     A step in a store's history, as it is read rather than as it is stored.
/// </summary>
/// <param name="Sequence">Where the step sits in the store's history.</param>
/// <param name="Timestamp">When the step was taken.</param>
/// <param name="Description">What the step did, in a form short enough to list.</param>
/// <param name="Kind">What kind of change the step made.</param>
/// <param name="IsApplied">
///     Whether the step is currently done to the table. The last step that is applied is the one undoing
///     takes back; the first step that is not is the one redoing puts in place again.
/// </param>
public sealed record HistoryEntry(
    int Sequence,
    DateTimeOffset Timestamp,
    string Description,
    TableEditKind Kind,
    bool IsApplied);

/// <summary>
///     What happened to a store's history.
/// </summary>
public enum HistoryAction
{
    /// <summary>A new step was written to the history.</summary>
    Recorded,

    /// <summary>The newest applied step was taken back.</summary>
    Undone,

    /// <summary>The oldest step that was not applied was put in place again.</summary>
    Redone,

    /// <summary>The whole history was thrown away.</summary>
    Cleared,
}

/// <summary>
///     Told to whoever is showing a store's history when it changes.
/// </summary>
/// <param name="Path">The store whose history changed.</param>
/// <param name="Action">What happened to it.</param>
/// <param name="Entry">
///     The step the change was about, or <see langword="null" /> when the history was thrown away.
/// </param>
public sealed record HistoryChangedEventArgs(string Path, HistoryAction Action, HistoryEntry? Entry);

/// <summary>
///     The edits made to one table store, and the cursor that walks back and forth over them.
/// </summary>
/// <remarks>
///     <para>
///         The history belongs to the store rather than to the document that opened it, so it is written
///         beside the table it describes and survives the document being closed. That is also what lets it
///         hold a copy of the values an operation is about to change without the table itself being held in
///         memory to compare against.
///     </para>
///     <para>
///         Only the newest <see cref="MaximumEntries" /> steps are kept, because a history that can never be
///         trimmed would grow the store file for as long as the table is worked on, and the steps a user
///         wants to take back are the recent ones.
///     </para>
/// </remarks>
public interface ITableHistory
{
    /// <summary>
    ///     Raised whenever the history of a store changes, so whatever is showing that history can read it
    ///     again. A recorder never has to know who is watching it, which is what lets an operation record
    ///     itself without also having to tell the interface about it.
    /// </summary>
    event EventHandler<HistoryChangedEventArgs>? Changed;

    /// <summary>
    ///     How many steps of the history are kept before the oldest are dropped.
    /// </summary>
    int MaximumEntries { get; }

    /// <summary>
    ///     Starts recording what an operation does to the table at <paramref name="path" />.
    /// </summary>
    /// <param name="path">The store the operation will change.</param>
    /// <param name="kind">What kind of change the operation makes.</param>
    /// <param name="description">What the operation does, in a form short enough to list.</param>
    /// <returns>
    ///     The recording, which the caller describes the operation to and then commits. Nothing is written
    ///     to the history unless the commit is reached, so an operation that fails or is cancelled leaves no
    ///     trace.
    /// </returns>
    ITableEditScope BeginEdit(string path, TableEditKind kind, string description);

    /// <summary>
    ///     Whether there is a step that can be taken back.
    /// </summary>
    Task<bool> CanUndoAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Whether there is a step that can be put in place again.
    /// </summary>
    Task<bool> CanRedoAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Takes the last step back.
    /// </summary>
    /// <returns>The step that was taken back, or <see langword="null" /> when there was none.</returns>
    Task<HistoryEntry?> UndoAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Puts the next step in place again.
    /// </summary>
    /// <returns>The step that was put in place, or <see langword="null" /> when there was none.</returns>
    Task<HistoryEntry?> RedoAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the whole history of the store, oldest step first.
    /// </summary>
    Task<IReadOnlyList<HistoryEntry>> GetEntriesAsync(
        string path,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     How many steps can be taken back.
    /// </summary>
    Task<int> GetUndoDepthAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     How many steps can be put in place again.
    /// </summary>
    Task<int> GetRedoDepthAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Throws the whole history away, leaving the table as it is.
    /// </summary>
    Task ClearAsync(string path, CancellationToken cancellationToken = default);
}

/// <summary>
///     One operation being recorded.
/// </summary>
/// <remarks>
///     The caller says which parts of the table it is about to change before it changes them, and the
///     recording reads those parts again once it is done. What changed between the two readings is what the
///     history keeps, so an operation only has to know what it touches rather than what it does, and an
///     operation that turns out to change nothing leaves no entry behind.
/// </remarks>
public interface ITableEditScope : IAsyncDisposable
{
    /// <summary>
    ///     Describes a part of the table by what it holds now, before the operation changes it.
    /// </summary>
    Task CaptureBeforeAsync(TableRegion region, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Describes a part of the table by what it holds now, after the operation created it.
    /// </summary>
    /// <remarks>
    ///     Used for rows and tables that did not exist before the operation, and whose identity is therefore
    ///     only known once it has run.
    /// </remarks>
    Task CaptureAfterAsync(TableRegion region, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Writes the step to the history.
    /// </summary>
    /// <returns>
    ///     The step that was written, or <see langword="null" /> when the operation changed nothing, or
    ///     changed nothing that can be taken back.
    /// </returns>
    Task<HistoryEntry?> CommitAsync(CancellationToken cancellationToken = default);
}
