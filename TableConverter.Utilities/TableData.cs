using System.Text;

namespace TableConverter.Utilities;

/// <summary>
///     Represents a table containing headers and rows of data.
/// </summary>
public class TableData : IEquatable<TableData>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TableData" /> class with headers and rows.
    /// </summary>
    /// <param name="headers">The headers of the table.</param>
    /// <param name="rows">The rows of data in the table.</param>
    public TableData(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
    {
        Headers = new List<string>(headers);
        Rows = new List<string[]>(rows.Select(x => x.ToArray()));
    }

    /// <summary>
    ///     Copies a table data from another.
    /// </summary>
    /// <param name="other">The table data to copy.</param>
    public TableData(TableData other)
    {
        ArgumentNullException.ThrowIfNull(other);

        Headers = new List<string>(other.Headers);
        // Deep copy the rows so mutating a copied table can never affect the original.
        Rows = other.Rows.Select(row => row.ToArray()).ToList();
    }

    /// <summary>
    ///     Gets the headers of the table.
    /// </summary>
    public List<string> Headers { get; }

    /// <summary>
    ///     Gets the rows of data in the table.
    /// </summary>
    public List<string[]> Rows { get; }

    /// <summary>
    ///     Determines whether the current <see cref="TableData" /> is equal to another <see cref="TableData" />.
    /// </summary>
    /// <param name="other">The table data to compare with the current instance.</param>
    /// <returns>True if both tables have identical headers and rows; otherwise, false.</returns>
    public bool Equals(TableData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Headers.Count != other.Headers.Count) return false;
        if (Rows.Count != other.Rows.Count) return false;
        if (!Headers.SequenceEqual(other.Headers, StringComparer.Ordinal)) return false;

        for (var i = 0; i < Rows.Count; i++)
        {
            if (!Rows[i].SequenceEqual(other.Rows[i], StringComparer.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Determines whether the current <see cref="TableData" /> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>True if the current instance is equal to the specified object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is TableData other && Equals(other);
    }

    /// <summary>
    ///     Returns a hash code for the current <see cref="TableData" /> instance.
    /// </summary>
    /// <returns>A content-based hash code representing the current instance.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var header in Headers)
        {
            hash.Add(header);
        }

        foreach (var row in Rows)
        {
            hash.Add(row.Length);

            foreach (var cell in row)
            {
                hash.Add(cell);
            }
        }

        return hash.ToHashCode();
    }

    /// <summary>
    ///     Returns a string representation of the <see cref="TableData" /> instance.
    /// </summary>
    /// <returns>A string that represents the table in CSV-like format.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendJoin(',', Headers);
        sb.AppendLine();

        foreach (var row in Rows)
        {
            sb.AppendJoin(',', row);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}