using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TableConverter.Utilities.Database;

/// <summary>
///     Which arithmetic is applied across the values of a row.
/// </summary>
public enum MathOperator
{
    /// <summary>The values are added together.</summary>
    Add,

    /// <summary>Each value after the first is taken away from the one before it.</summary>
    Subtract,

    /// <summary>The values are multiplied together.</summary>
    Multiply,

    /// <summary>Each value after the first is divided into the one before it.</summary>
    Divide,
}

/// <summary>
///     Which part of a date or time is taken out of a value.
/// </summary>
public enum DatePartKind
{
    Year,
    Month,
    Day,
    Hour,
    Minute,
    Second,

    /// <summary>The day of the week as a number, counting Sunday as zero.</summary>
    DayOfWeek,

    /// <summary>The day of the year, counting from one.</summary>
    DayOfYear,

    /// <summary>The quarter of the year, counting from one.</summary>
    Quarter,

    /// <summary>The name of the month, in the reader's own language.</summary>
    MonthName,

    /// <summary>The name of the day of the week, in the reader's own language.</summary>
    WeekdayName,
}

/// <summary>
///     The per-value arithmetic a derived column is built from.
/// </summary>
/// <remarks>
///     <para>
///         Each of these is a pure function of the values it is handed, so what a derived column would hold
///         can be worked out, and checked, without a store behind it. The store is only needed to carry the
///         answer to the rows, which is the engine's job rather than the arithmetic's.
///     </para>
///     <para>
///         A value that cannot be read as what the operation needs - a number for the arithmetic, a moment
///         for a date part - reads as nothing rather than as a fault, so one value of the wrong shape only
///         costs the cell it sits in rather than the whole column.
///     </para>
/// </remarks>
public static class ComputedValueOperations
{
    /// <summary>
    ///     Whether a value counts as unset, which is how an empty cell and a cell of nothing but space are
    ///     both read.
    /// </summary>
    public static bool IsBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    ///     Joins a row's values into one, with <paramref name="separator" /> between them.
    /// </summary>
    /// <param name="values">The values to join, in the order they are to appear.</param>
    /// <param name="separator">What to put between the values. Empty when they are to be run together.</param>
    /// <param name="skipBlank">Whether a value that is unset is left out rather than joined as nothing.</param>
    public static string Concatenate(IReadOnlyList<string?> values, string? separator, bool skipBlank)
    {
        ArgumentNullException.ThrowIfNull(values);

        var parts = skipBlank
            ? values.Where(value => !IsBlank(value)).Select(value => value!.Trim())
            : values.Select(value => value ?? string.Empty);

        return string.Join(separator ?? string.Empty, parts);
    }

    /// <summary>
    ///     Takes one part out of a value that holds several, split on a separator.
    /// </summary>
    /// <param name="value">The value to split.</param>
    /// <param name="separator">What the value is split on.</param>
    /// <param name="index">Which part is wanted, counting from zero.</param>
    /// <returns>
    ///     The part, or <see langword="null" /> when the value has no part there, which is how a row that is
    ///     missing a section reads rather than as a fault.
    /// </returns>
    public static string? SplitPart(string? value, string separator, int index)
    {
        if (value is null || string.IsNullOrEmpty(separator) || index < 0)
        {
            return null;
        }

        var parts = value.Split(separator, StringSplitOptions.None);

        return index < parts.Length ? parts[index] : null;
    }

    /// <summary>
    ///     Takes a run of characters out of a value.
    /// </summary>
    /// <param name="value">The value to take from.</param>
    /// <param name="start">Where the run starts, counting from zero.</param>
    /// <param name="length">
    ///     How many characters the run holds, or a negative number to run to the end of the value.
    /// </param>
    /// <returns>The run, or <see langword="null" /> when the value has nothing there.</returns>
    public static string? ExtractSubstring(string? value, int start, int length)
    {
        if (value is null || start < 0 || start >= value.Length)
        {
            return null;
        }

        if (length < 0 || start + length > value.Length)
        {
            return value[start..];
        }

        return value.Substring(start, length);
    }

    /// <summary>
    ///     Takes a value out of a value by matching it against a regular expression.
    /// </summary>
    /// <param name="value">The value to match.</param>
    /// <param name="pattern">The regular expression to match.</param>
    /// <param name="group">
    ///     Which capture is wanted, counting from zero. A pattern without a capture of its own holds the
    ///     whole of what it matched in group zero.
    /// </param>
    /// <returns>The capture, or <see langword="null" /> when the value does not match.</returns>
    public static string? ExtractMatch(string? value, string pattern, int group)
    {
        if (value is null || string.IsNullOrEmpty(pattern) || group < 0)
        {
            return null;
        }

        Match match;

        try
        {
            match = Regex.Match(value, pattern);
        }
        catch (ArgumentException)
        {
            // A pattern that cannot be read matches nothing rather than being allowed to take the operation
            // down part way through a table.
            return null;
        }

        if (!match.Success || group >= match.Groups.Count)
        {
            return null;
        }

        return match.Groups[group].Value;
    }

    /// <summary>
    ///     Works out a value from the numbers of a row.
    /// </summary>
    /// <param name="values">The values to work out, in the order they are applied.</param>
    /// <param name="isDescending">
    ///     Which way the work is applied: <see langword="false" /> left to right, <see langword="true" />
    ///     from the right back to the left, which is what makes subtraction and division read the way they
    ///     are written.
    /// </param>
    /// <param name="mathOperator">Which arithmetic to apply.</param>
    /// <param name="decimalPlaces">How many decimal places the answer is rounded to.</param>
    /// <returns>
    ///     The answer as it is to be stored, or <see langword="null" /> when a value does not read as a
    ///     number or the arithmetic cannot be done.
    /// </returns>
    public static string? Calculate(
        IReadOnlyList<string?> values,
        MathOperator mathOperator,
        int decimalPlaces)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
        {
            return null;
        }

        var numbers = new decimal[values.Count];

        for (var index = 0; index < values.Count; index++)
        {
            if (values[index] is null
                || !decimal.TryParse(values[index], NumberStyles.Number, CultureInfo.InvariantCulture,
                    out var number))
            {
                return null;
            }

            numbers[index] = number;
        }

        var result = numbers[0];

        for (var index = 1; index < numbers.Length; index++)
        {
            switch (mathOperator)
            {
                case MathOperator.Add:
                    result += numbers[index];
                    break;

                case MathOperator.Subtract:
                    result -= numbers[index];
                    break;

                case MathOperator.Multiply:
                    result *= numbers[index];
                    break;

                case MathOperator.Divide:
                    if (numbers[index] == 0m)
                    {
                        return null;
                    }

                    result /= numbers[index];
                    break;

                default:
                    return null;
            }
        }

        var rounded = Math.Round(result, Math.Clamp(decimalPlaces, 0, 15), MidpointRounding.AwayFromZero);

        return rounded.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Takes one part out of a value that reads as a date or a time.
    /// </summary>
    /// <param name="value">The value to read.</param>
    /// <param name="kind">Which part is wanted.</param>
    /// <returns>
    ///     The part, or <see langword="null" /> when the value does not read as a moment. A part that is a
    ///     number is written without thousands separators so it can be read back as an integer.
    /// </returns>
    public static string? DatePart(string? value, DatePartKind kind)
    {
        if (IsBlank(value)
            || !DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var moment))
        {
            return null;
        }

        return kind switch
        {
            DatePartKind.Year => moment.Year.ToString(CultureInfo.InvariantCulture),
            DatePartKind.Month => moment.Month.ToString(CultureInfo.InvariantCulture),
            DatePartKind.Day => moment.Day.ToString(CultureInfo.InvariantCulture),
            DatePartKind.Hour => moment.Hour.ToString(CultureInfo.InvariantCulture),
            DatePartKind.Minute => moment.Minute.ToString(CultureInfo.InvariantCulture),
            DatePartKind.Second => moment.Second.ToString(CultureInfo.InvariantCulture),
            DatePartKind.DayOfWeek => ((int)moment.DayOfWeek).ToString(CultureInfo.InvariantCulture),
            DatePartKind.DayOfYear => moment.DayOfYear.ToString(CultureInfo.InvariantCulture),
            DatePartKind.Quarter => (((moment.Month - 1) / 3) + 1).ToString(CultureInfo.InvariantCulture),
            DatePartKind.MonthName => moment.ToString("MMMM", CultureInfo.InvariantCulture),
            DatePartKind.WeekdayName => moment.ToString("dddd", CultureInfo.InvariantCulture),
            _ => null,
        };
    }

    /// <summary>
    ///     Whether a part of a date reads as a number rather than as a name, which is what decides the type
    ///     of the column it is written into.
    /// </summary>
    public static bool IsNumericPart(DatePartKind kind)
    {
        return kind is not (DatePartKind.MonthName or DatePartKind.WeekdayName);
    }
}

