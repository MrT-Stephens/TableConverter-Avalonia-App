using System.Globalization;

namespace TableConverter.Utilities.Database.Models.TableStore;

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
public static class ColumnDataTypeExtensions
{
    /// <summary>
    ///     Thousands separators are accepted so a value copied from a formatted report still reads as a
    ///     number.
    /// </summary>
    private const NumberStyles IntegerStyles = NumberStyles.Integer | NumberStyles.AllowThousands;

    private const NumberStyles DecimalStyles = NumberStyles.Number;

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
    /// <remarks>
    ///     Values are parsed with the invariant culture, matching the formats the file converters
    ///     produce. This is a test of what the value looks like, not a conversion: the store keeps
    ///     whatever the user typed.
    /// </remarks>
    public static bool IsValidValue(this ColumnDataType dataType, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        return dataType switch
        {
            ColumnDataType.Integer => long.TryParse(value, IntegerStyles, CultureInfo.InvariantCulture, out _),
            ColumnDataType.Decimal => decimal.TryParse(value, DecimalStyles, CultureInfo.InvariantCulture, out _),
            ColumnDataType.Boolean => bool.TryParse(value, out _),
            ColumnDataType.Date => DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            ColumnDataType.DateTime => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out _),

            // Text takes anything, and so does a type this build does not know about.
            _ => true,
        };
    }
}

