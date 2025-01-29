using System.Text;

namespace TableConverter.DataGeneration.DataModels;

/// <summary>
///     Represents a table containing headers and rows of data.
/// </summary>
public class TableData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TableData" /> class with headers and rows.
    /// </summary>
    /// <param name="headers">The headers of the table.</param>
    /// <param name="rows">The rows of data in the table.</param>
    public TableData(List<string> headers, List<string[]> rows)
    {
        Headers = headers;
        Rows = rows;
    }

    /// <summary>
    ///     Gets the headers of the table.
    /// </summary>
    public IReadOnlyList<string> Headers { get; }

    /// <summary>
    ///     Gets the rows of data in the table.
    /// </summary>
    public IReadOnlyList<string[]> Rows { get; }

    /// <summary>
    ///     Determines whether the current <see cref="TableData" /> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>True if the current instance is equal to the specified object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not TableData other) return false;

        return Headers.SequenceEqual(other.Headers) &&
               !Rows.Where((row, i) => !row.SequenceEqual(other.Rows[i])).Any();
    }

    /// <summary>
    ///     Returns a hash code for the current <see cref="TableData" /> instance.
    /// </summary>
    /// <returns>A hash code representing the current instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Headers, Rows);
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