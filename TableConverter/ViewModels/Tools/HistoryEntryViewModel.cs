using Avalonia;
using Avalonia.Media;
using TableConverter.Utilities.Database.History;

namespace TableConverter.ViewModels.Tools;

/// <summary>
///     One step of a table's history, as the history panel shows it.
/// </summary>
/// <remarks>
///     The step itself is immutable and carries everything that has to be remembered about the change, so
///     this adds only what a row of the list needs to draw: the time in a form short enough to sit beside
///     the description, and an icon standing for the kind of change the step made.
/// </remarks>
public sealed class HistoryEntryViewModel
{
    public HistoryEntryViewModel(HistoryEntry entry)
    {
        Entry = entry;
    }

    /// <summary>
    ///     The step this row stands for.
    /// </summary>
    public HistoryEntry Entry { get; }

    public int Sequence => Entry.Sequence;

    public string Description => Entry.Description;

    public TableEditKind Kind => Entry.Kind;

    /// <summary>
    ///     Whether the step is currently done to the table, which is what tells the rows above the cursor
    ///     apart from the rows below it.
    /// </summary>
    public bool IsApplied => Entry.IsApplied;

    /// <summary>
    ///     The time the step was taken, to the second, in the reader's own time zone.
    /// </summary>
    public string Time => Entry.Timestamp.LocalDateTime.ToString("HH:mm:ss");

    /// <summary>
    ///     An icon standing for the kind of change the step made.
    /// </summary>
    public StreamGeometry? Icon => ResolveIcon(Entry.Kind);

    /// <summary>
    ///     Picks the icon for a kind of change from the application's resources.
    /// </summary>
    /// <remarks>
    ///     A step whose icon has gone missing is still worth listing, so the history marker is fallen back
    ///     on rather than the lookup being allowed to throw while the list is being drawn.
    /// </remarks>
    private static StreamGeometry? ResolveIcon(TableEditKind kind)
    {
        var name = kind switch
        {
            TableEditKind.RowsAdded => "DataAddIcon",
            TableEditKind.RowsDeleted => "DeleteIcon",
            TableEditKind.CellsChanged => "EditIcon",
            TableEditKind.ColumnsChanged => "TableIcon",
            TableEditKind.RowOrderChanged => "UpDownIcon",
            TableEditKind.TableRotated => "RotateClockwiseIcon",
            TableEditKind.TableReplaced => "ArrowReturnIcon",
            TableEditKind.Composite => "GearIcon",
            _ => "HistoryIcon",
        };

        return Application.Current?.Resources[name] as StreamGeometry
               ?? Application.Current?.Resources["HistoryIcon"] as StreamGeometry;
    }
}

