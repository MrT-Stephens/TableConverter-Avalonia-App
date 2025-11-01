using System.Text;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities;

/// <summary>
///     Represents a table containing headers and rows of data.
/// </summary>
public class TableData : ITableData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TableData" /> class with headers and rows.
    /// </summary>
    /// <param name="headers">The headers of the table.</param>
    /// <param name="rows">The rows of data in the table.</param>
    public TableData(IEnumerable<string> headers, IEnumerable<IEnumerable<object>> rows)
    {
        Headers = [.. headers];
        Rows = [.. rows.Select(row => row.ToList())];
    }

    /// <summary>
    ///    Initializes a new instance of the <see cref="TableData" /> class with headers and rows.
    /// </summary>
    /// <param name="headers">
    /// <param name="headers">The headers of the table.</param>
    /// <param name="rows">The rows of data in the table.</param>
    public TableData(List<string> headers, List<List<object>> rows)
    {
        Headers = headers;
        Rows = rows;
    }

    /// <summary>
    ///     Gets the headers of the table.
    /// </summary>
    public List<string> Headers { get; }

    /// <summary>
    ///     Gets the rows of data in the table.
    /// </summary>
    public List<List<object>> Rows { get; }

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

        Rows.ForEach(row =>
        {
            sb.AppendJoin(',', row);
            sb.AppendLine();
        });

        return sb.ToString();
    }

    public TCollection GetRows<TCollection>() where TCollection : IEnumerable<IEnumerable<object>>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///    Gets an empty <see cref="TableData" /> instance.
    /// </summary>
    public static TableData Empty => new(new List<string>(), new List<object[]>());

    /// <summary>
    ///     Determines whether two <see cref="TableData" /> instances are equal.
    /// </summary>
    /// <param name="left">
    ///     The first <see cref="TableData" /> instance to compare.
    /// </param>
    /// <param name="right">
    ///     The second <see cref="TableData" /> instance to compare.
    /// </param>
    /// <returns>
    ///     Whether the two <see cref="TableData" /> instances are equal.
    /// </returns>
    public static bool operator ==(TableData? left, TableData? right)
    {
        if (left is null && right is null) 
            return true;

        if (left is null || right is null) 
            return false;

        return left.Equals(right);
    }


    /// <summary>
    ///     Determines whether two <see cref="TableData" /> instances are not equal.
    /// </summary>
    /// <param name="left">
    ///     The first <see cref="TableData" /> instance to compare.
    /// </param>
    /// <param name="right">
    ///     The second <see cref="TableData" /> instance to compare.
    /// </param>
    /// <returns>
    ///     Whether the two <see cref="TableData" /> instances are not equal.
    /// </returns>
    public static bool operator !=(TableData? left, TableData? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Gets the headers of the table.
    /// </summary>
    /// <returns>
    ///     An enumerable collection of header strings.
    /// </returns>
    public IEnumerable<string> GetHeaders()
    {
        return Headers;
    }

    /// <summary>
    ///     Gets the rows of data in the table.
    /// </summary>
    /// <returns>
    ///     An enumerable collection of rows, where each row is an enumerable collection of objects.
    /// </returns>
    public IEnumerable<IEnumerable<object>> GetRows()
    {
        return Rows;
    }

    public bool IsEmpty()
    {
        return !Rows.Any() || !Headers.Any();
    }

    public int GetRowCount()
    {
        return Rows.Count;
    }

    public int GetHeaderCount()
    {
        return Headers.Count;
    }
}