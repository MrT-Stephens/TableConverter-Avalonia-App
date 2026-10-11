namespace TableConverter.Utilities.Models;

/// <summary>
///     Works out the kind of value a column holds by reading the values that were put in it.
/// </summary>
/// <remarks>
///     <para>
///         A file being read says nothing about what its columns hold, so every column of an imported
///         table would otherwise be left as text. Reading the values back gives a type that is worth
///         something - a column of whole numbers is an integer column, so it lines up on the right and
///         is exported as numbers rather than as text.
///     </para>
///     <para>
///         The type is only settled when <em>every</em> value reads as it, so a single value that does
///         not fit leaves the column as text rather than mistyping it. Nothing is changed about the
///         values themselves, and the type can be set by hand afterwards, so a column that is read
///         wrongly costs nothing more than an edit.
///     </para>
/// </remarks>
public sealed class ColumnTypeInference
{
    /// <summary>
    ///     The kinds of value the column could still be holding. A column whose set has been emptied
    ///     cannot be anything but text, and is not looked at again.
    /// </summary>
    private readonly Dictionary<int, Candidates> _candidates = [];

    /// <summary>
    ///     Reads one value into the type being worked out for its column.
    /// </summary>
    /// <param name="columnIndex">The position of the column within a row's cells.</param>
    /// <param name="value">The value that was written to the column.</param>
    /// <remarks>
    ///     A missing value is passed over rather than counted: a cell is allowed to hold nothing
    ///     whatever its column's type is, so it says nothing about what the column holds.
    /// </remarks>
    public void Observe(int columnIndex, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!_candidates.TryGetValue(columnIndex, out var candidates))
        {
            candidates = Candidates.All;
        }

        _candidates[columnIndex] = candidates & CandidatesFor(value);
    }

    /// <summary>
    ///     The type the column has been read to hold.
    /// </summary>
    /// <param name="columnIndex">The position of the column within a row's cells.</param>
    /// <returns>
    ///     The narrowest type every value written to the column reads as, or
    ///     <see cref="ColumnDataType.Text" /> when they do not agree, when nothing was written, or when
    ///     only whitespace was.
    /// </returns>
    public ColumnDataType Result(int columnIndex)
    {
        if (!_candidates.TryGetValue(columnIndex, out var candidates))
        {
            return ColumnDataType.Text;
        }

        // Narrowest first, so a column of whole numbers is an integer column rather than a decimal one,
        // and a column of dates is a date column rather than a date and time one.
        if (candidates.HasFlag(Candidates.Integer))
        {
            return ColumnDataType.Integer;
        }

        if (candidates.HasFlag(Candidates.Decimal))
        {
            return ColumnDataType.Decimal;
        }

        if (candidates.HasFlag(Candidates.Boolean))
        {
            return ColumnDataType.Boolean;
        }

        if (candidates.HasFlag(Candidates.Date))
        {
            return ColumnDataType.Date;
        }

        return candidates.HasFlag(Candidates.DateTime) ? ColumnDataType.DateTime : ColumnDataType.Text;
    }

    /// <summary>
    ///     The kinds of value <paramref name="value" /> reads as on its own.
    /// </summary>
    /// <remarks>
    ///     Several of these overlap on purpose: <c>42</c> is both an integer and a decimal, and
    ///     <c>2026-09-26</c> is both a date and a date and time. Which one a column ends up with is
    ///     settled by <see cref="Result" /> once every value has been read, so what matters here is only
    ///     that a value rules out the kinds it cannot be.
    /// </remarks>
    private static Candidates CandidatesFor(string value)
    {
        var candidates = Candidates.None;

        if (ColumnDataType.Integer.IsValidValue(value))
        {
            candidates |= Candidates.Integer;
        }

        if (ColumnDataType.Decimal.IsValidValue(value))
        {
            candidates |= Candidates.Decimal;
        }

        if (ColumnDataType.Boolean.IsValidValue(value))
        {
            candidates |= Candidates.Boolean;
        }

        if (ColumnDataType.Date.IsValidValue(value))
        {
            candidates |= Candidates.Date;
        }

        if (ColumnDataType.DateTime.IsValidValue(value))
        {
            candidates |= Candidates.DateTime;
        }

        return candidates;
    }

    [Flags]
    private enum Candidates
    {
        None = 0,

        Integer = 1 << 0,

        Decimal = 1 << 1,

        Boolean = 1 << 2,

        Date = 1 << 3,

        DateTime = 1 << 4,

        All = Integer | Decimal | Boolean | Date | DateTime,
    }
}

