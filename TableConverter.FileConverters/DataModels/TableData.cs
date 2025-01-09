using System.Text;

namespace TableConverter.FileConverters.DataModels;

public class TableData(List<string> headers, List<string[]> rows)
{
    public IReadOnlyList<string> Headers { get; } = headers;
    public IReadOnlyList<string[]> Rows { get; } = rows;

    public override bool Equals(object? obj)
    {
        if (obj is not TableData other) return false;

        return Headers.SequenceEqual(other.Headers) &&
               !Rows.Where((row, i) => !row.SequenceEqual(other.Rows[i])).Any();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Headers, Rows);
    }

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