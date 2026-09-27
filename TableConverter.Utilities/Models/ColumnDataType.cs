using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace TableConverter.Utilities.Models;

/// <summary>
///     The kind of value a column holds.
/// </summary>
/// <remarks>
///     The type is metadata: a cell always stores its value as text, and the type says how that text is
///     meant to be read. Nothing is rejected on the strength of it, so a column can be retyped without
///     its values having to be rewritten.
///     The numeric values are part of the on disk format and must not be renumbered.
/// </remarks>
public enum ColumnDataType
{
    /// <summary>
    ///     Anything that reads as text. This is 0 because it is what every store written before columns
    ///     could be typed holds.
    /// </summary>
    Text = 0,

    Integer = 1,

    Decimal = 2,

    Boolean = 3,

    Date = 4,

    DateTime = 5,
}

/// <summary>
///     Reads the values of a <see cref="ColumnDataType" />.
/// </summary>
/// <remarks>
///     Every value is read and written with the invariant culture, matching the formats the file
///     converters produce. Reaching for a value this way is a test of what the text looks like rather
///     than a conversion: the store keeps whatever the user typed.
/// </remarks>
public static class ColumnDataTypeExtensions
{
    /// <summary>
    ///     Thousands separators are accepted so a value copied from a formatted report still reads as a
    ///     number.
    /// </summary>
    private const NumberStyles IntegerStyles = NumberStyles.Integer | NumberStyles.AllowThousands;

    private const NumberStyles DecimalStyles = NumberStyles.Number;

    /// <summary>
    ///     Grouping with as many decimal places as a value has, so a number reads the way a spreadsheet
    ///     would show it without any of its precision being rounded away.
    /// </summary>
    private const string DecimalFormat = "#,0.############################";

    /// <summary>
    ///     The format a date is shown in. A date is shown the way it is stored, which is the one form
    ///     that cannot be misread.
    /// </summary>
    private const string DateFormat = "yyyy-MM-dd";

    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    ///     Whether the values of this type are written right aligned, the way numbers read.
    /// </summary>
    public static bool IsNumeric(this ColumnDataType dataType)
    {
        return dataType is ColumnDataType.Integer or ColumnDataType.Decimal;
    }

    /// <summary>
    ///     Whether <paramref name="value" /> reads as this type.
    /// </summary>
    /// <param name="dataType">The type the column was given.</param>
    /// <param name="value">The stored text to test.</param>
    /// <returns>
    ///     <see langword="true" /> when the value reads as the type. A missing value is always valid,
    ///     because a cell is allowed to hold nothing whatever its column's type is.
    /// </returns>
    public static bool IsValidValue(this ColumnDataType dataType, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        return dataType is ColumnDataType.Text || Accepts(dataType, value);
    }

    /// <summary>
    ///     Reads <paramref name="value" /> as this type.
    /// </summary>
    /// <param name="dataType">The type the column was given.</param>
    /// <param name="value">The stored text to read.</param>
    /// <param name="typed">
    ///     The value as the type's own .NET type: a <see cref="long" />, a <see cref="decimal" />, a
    ///     <see cref="bool" />, a <see cref="DateOnly" />, a <see cref="DateTime" />, or the text
    ///     itself for a text column.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> when the value reads as the type. A missing or unreadable value
    ///     reports <see langword="false" />, so a caller can fall back to writing the text as it stands
    ///     rather than losing what it holds.
    /// </returns>
    /// <remarks>
    ///     This is what lets an exporter write a cell as the kind of thing it holds rather than as
    ///     text, so a number lands in a spreadsheet as a number.
    /// </remarks>
    public static bool TryReadValue(this ColumnDataType dataType, string? value,
        [NotNullWhen(true)] out object? typed)
    {
        typed = null;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        switch (dataType)
        {
            case ColumnDataType.Integer when long.TryParse(value, IntegerStyles, CultureInfo.InvariantCulture,
                out var integer):
                typed = integer;
                return true;

            case ColumnDataType.Decimal when decimal.TryParse(value, DecimalStyles, CultureInfo.InvariantCulture,
                out var number):
                typed = number;
                return true;

            case ColumnDataType.Boolean when bool.TryParse(value, out var flag):
                typed = flag;
                return true;

            case ColumnDataType.Date when ReadsAsDate(value, out var date):
                typed = date;
                return true;

            case ColumnDataType.DateTime when DateTime.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var moment):
                typed = moment;
                return true;

            case ColumnDataType.Text:
                typed = value;
                return true;

            default:
                // A type this build does not know about is left as the text it already is.
                typed = value;
                return true;
        }
    }

    /// <summary>
    ///     Shows <paramref name="value" /> the way its column's type reads.
    /// </summary>
    /// <param name="dataType">The type the column was given.</param>
    /// <param name="value">The stored text to show.</param>
    /// <returns>
    ///     The value laid out for reading, which for a number means grouping its thousands and for a
    ///     date means the one form that cannot be misread.
    /// </returns>
    /// <remarks>
    ///     A value that does not read as its column's type is shown exactly as it was typed, so what is
    ///     wrong with it can be seen. This is display only: nothing writes the result back over what the
    ///     user entered.
    /// </remarks>
    public static string? FormatValue(this ColumnDataType dataType, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return dataType switch
        {
            ColumnDataType.Integer when long.TryParse(value, IntegerStyles, CultureInfo.InvariantCulture,
                out var integer) => integer.ToString("N0", CultureInfo.InvariantCulture),

            ColumnDataType.Decimal when decimal.TryParse(value, DecimalStyles, CultureInfo.InvariantCulture,
                out var number) => number.ToString(DecimalFormat, CultureInfo.InvariantCulture),

            ColumnDataType.Boolean when bool.TryParse(value, out var flag) => flag.ToString(),

            ColumnDataType.Date when ReadsAsDate(value, out var date) =>
                date.ToString(DateFormat, CultureInfo.InvariantCulture),

            ColumnDataType.DateTime when DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var moment) => moment.ToString(DateTimeFormat, CultureInfo.InvariantCulture),

            _ => value,
        };
    }

    /// <summary>
    ///     Whether a non empty value reads as this type on its own, which is the test the inference
    ///     works from. Text and a type this build does not know about take anything.
    /// </summary>
    private static bool Accepts(ColumnDataType dataType, string value)
    {
        return dataType switch
        {
            ColumnDataType.Integer => long.TryParse(value, IntegerStyles, CultureInfo.InvariantCulture, out _),
            ColumnDataType.Decimal => decimal.TryParse(value, DecimalStyles, CultureInfo.InvariantCulture, out _),
            ColumnDataType.Boolean => bool.TryParse(value, out _),
            ColumnDataType.Date => ReadsAsDate(value, out _),
            ColumnDataType.DateTime => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out _),

            _ => true,
        };
    }

    /// <summary>
    ///     Whether <paramref name="value" /> holds a day rather than a day and a time, and reads that day.
    /// </summary>
    /// <param name="value">The stored text to read.</param>
    /// <param name="day">The day the value names.</param>
    /// <returns><see langword="true" /> when the value holds a day and no time of day.</returns>
    /// <remarks>
    ///     A date is only read as a day when no time of day is carried with it.
    ///     <see cref="DateOnly.TryParse(string, IFormatProvider, DateTimeStyles, out DateOnly)" /> accepts
    ///     the whole of a date and time and quietly drops the time, so without this a column of timestamps
    ///     would read as a column of dates and lose the time the moment it was shown or exported. A time
    ///     of midnight counts as no time, so a timestamp that only ever names the start of a day is still
    ///     a date.
    /// </remarks>
    private static bool ReadsAsDate(string value, out DateOnly day)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var moment)
            && moment.TimeOfDay == TimeSpan.Zero)
        {
            day = DateOnly.FromDateTime(moment);
            return true;
        }

        day = default;
        return false;
    }
}

