using System.Diagnostics.CodeAnalysis;

namespace TableConverter.Utilities.Models;

/// <summary>
///     A column of a table: the name it goes by and the kind of value it holds.
/// </summary>
/// <remarks>
///     This is what travels with a table in place of a bare column name, so the kind of value a column
///     holds is known on both sides of the streaming contract. Everything that has a use for it - the
///     grid, the search, the exporters - reads it from here rather than working it out again.
/// </remarks>
/// <param name="Name">The column's name, which is what the column is headed with.</param>
/// <param name="DataType">
///     The kind of value the column holds, or <see langword="null" /> when whoever supplied the table
///     did not know and the values are to be read to find out.
/// </param>
public readonly record struct TableColumn(string Name, ColumnDataType? DataType)
{
    /// <summary>
    ///     The type to write for the column before anything has been read, which is text.
    /// </summary>
    public ColumnDataType EffectiveDataType => DataType ?? ColumnDataType.Text;

    /// <summary>
    ///     Declares a column whose kind of value is not known to the caller.
    /// </summary>
    /// <param name="name">The column's name.</param>
    /// <remarks>
    ///     A file being read carries no type information, so an importer declares its columns this way
    ///     and leaves the sink to read the values and settle the type itself.
    /// </remarks>
    public static TableColumn Untyped(string name)
    {
        return new TableColumn(name, null);
    }

    /// <summary>
    ///     Declares a set of columns whose kind of value is not known to the caller.
    /// </summary>
    /// <param name="names">The column names, in order.</param>
    public static IReadOnlyList<TableColumn> Untyped(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        return [.. names.Select(Untyped)];
    }

    /// <summary>
    ///     Reads a stored value as the column's own kind of value.
    /// </summary>
    /// <param name="value">The stored text to read.</param>
    /// <param name="typed">
    ///     The value as the type's own .NET type, which is what lets an exporter write a real cell rather
    ///     than text.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the column's type is known and the value reads as it. A column
    ///     whose type is not known, or a value that does not read, reports <see langword="false" /> so a
    ///     caller falls back to writing the text as it stands.
    /// </returns>
    public bool TryReadValue(string? value, [NotNullWhen(true)] out object? typed)
    {
        if (DataType is { } dataType && dataType.TryReadValue(value, out var read))
        {
            typed = read;
            return true;
        }

        typed = null;
        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return DataType is { } dataType ? $"{Name}: {dataType}" : Name;
    }
}

/// <summary>
///     Reads the columns of a table.
/// </summary>
public static class TableColumnExtensions
{
    /// <summary>
    ///     The names of <paramref name="columns" />, in order, which is what a table that has no use for
    ///     the types wants.
    /// </summary>
    public static IReadOnlyList<string> Names(this IEnumerable<TableColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        return [.. columns.Select(column => column.Name)];
    }
}
