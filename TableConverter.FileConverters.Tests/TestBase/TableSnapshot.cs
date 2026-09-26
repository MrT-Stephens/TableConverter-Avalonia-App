using System.Runtime.CompilerServices;
using System.Text;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Tests.TestBase;

/// <summary>
///     An in-memory table a test can stand on either side of the streaming contract with.
/// </summary>
/// <remarks>
///     The converters stream rows rather than handing over a whole table, so a test needs a value that
///     can capture what an input handler pushes (the sink half), supply what an output handler pulls (the
///     source half) and compare by content so it can also be the expected value of a read. It is
///     test-local on purpose: the production code no longer has a table type of its own.
/// </remarks>
public sealed class TableSnapshot : ITableRowSink, ITableRowSource, IEquatable<TableSnapshot>
{
    private readonly List<string> _headers = [];
    private readonly List<string[]> _rows = [];

    /// <summary>
    ///     Creates an empty snapshot, which is what a test capturing a read starts from.
    /// </summary>
    public TableSnapshot()
    {
    }

    /// <summary>
    ///     Creates a snapshot that already holds <paramref name="headers" /> and <paramref name="rows" />,
    ///     which is what a test writing a table, or expecting a read, starts from.
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

        // A missing value is flattened to an empty string here, which is how a missing cell has always
        // been treated by the time it reaches a file.
        _rows.Add(cells.Select(cell => cell ?? string.Empty).ToArray());

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        IsCompleted = true;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetHeadersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(_headers.ToArray());
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string?[]> ReadRowsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var row in _rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return row;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Compares the headers and every row cell by cell, which is what makes a snapshot usable as the
    ///     expected value of a read.
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

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append(string.Join(',', _headers));

        foreach (var row in _rows)
        {
            builder.AppendLine();
            builder.Append(string.Join(',', row.Select(cell => cell.Replace(",", "\\,"))));
        }

        return builder.ToString();
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
}

