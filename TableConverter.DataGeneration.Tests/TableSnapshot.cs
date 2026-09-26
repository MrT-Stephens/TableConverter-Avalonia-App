using System.Text;
using TableConverter.Utilities;

namespace TableConverter.DataGeneration.Tests;

/// <summary>
///     Captures the rows a builder generates so a test can compare them with an expected table.
/// </summary>
/// <remarks>
///     The builder streams its rows into a sink rather than returning a whole table, so a test needs a
///     sink that keeps what it is given and compares by content. It is test-local on purpose: the
///     production code no longer has a table type of its own.
/// </remarks>
public sealed class TableSnapshot : ITableRowSink, IEquatable<TableSnapshot>
{
    private readonly List<string> _headers = [];
    private readonly List<string[]> _rows = [];

    /// <summary>
    ///     Creates an empty snapshot, which is what a test capturing a build starts from.
    /// </summary>
    public TableSnapshot()
    {
    }

    /// <summary>
    ///     Creates a snapshot that already holds <paramref name="headers" /> and <paramref name="rows" />,
    ///     which is what a test expecting a build starts from.
    /// </summary>
    /// <param name="headers">The column names of the table.</param>
    /// <param name="rows">The rows of the table, positionally matching <paramref name="headers" />.</param>
    public TableSnapshot(IReadOnlyList<string> headers, IEnumerable<string[]> rows)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);

        _headers.AddRange(headers);

        foreach (var row in rows)
        {
            _rows.Add(row.ToArray());
        }
    }

    /// <summary>
    ///     Gets the column names, in the order the values of each row are held.
    /// </summary>
    public IReadOnlyList<string> Headers => _headers;

    /// <summary>
    ///     Gets the rows, each positionally matching <see cref="Headers" />.
    /// </summary>
    public IReadOnlyList<string[]> Rows => _rows;

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public Task BeginAsync(IReadOnlyList<string> headers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headers);

        _headers.Clear();
        _headers.AddRange(headers);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task WriteRowAsync(IReadOnlyList<string?> cells, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cells);

        _rows.Add(cells.Select(cell => cell ?? string.Empty).ToArray());

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        IsCompleted = true;

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Compares the headers and every row cell by cell.
    /// </summary>
    public bool Equals(TableSnapshot? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _headers.SequenceEqual(other._headers) && RowsEqual(other);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TableSnapshot other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var header in _headers)
        {
            hash.Add(header);
        }

        foreach (var row in _rows)
        {
            foreach (var cell in row)
            {
                hash.Add(cell);
            }
        }

        return hash.ToHashCode();
    }

    private bool RowsEqual(TableSnapshot other)
    {
        if (_rows.Count != other._rows.Count)
        {
            return false;
        }

        for (var index = 0; index < _rows.Count; index++)
        {
            if (!_rows[index].SequenceEqual(other._rows[index]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns a string representation of the table.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("TableSnapshot: ");
        sb.Append(_headers.Count);
        sb.Append(" headers, ");
        sb.Append(_rows.Count);
        sb.Append(" rows");
        return sb.ToString();
    }
}
