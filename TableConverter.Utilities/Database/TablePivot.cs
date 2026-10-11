using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Database;

/// <summary>
///     How the rows of a group are boiled down to the one value the group is summarised by.
/// </summary>
public enum AggregateKind
{
    /// <summary>How many rows fall into the group, or how many of them hold a value.</summary>
    Count,

    /// <summary>The values of the group added together.</summary>
    Sum,

    /// <summary>The values of the group added together and shared out over how many there are.</summary>
    Average,

    /// <summary>The smallest of the values of the group.</summary>
    Minimum,

    /// <summary>The largest of the values of the group.</summary>
    Maximum,
}

/// <summary>
///     Boils a table down to one row per group, each holding the values the group is keyed by and the one
///     value its rows were summarised into.
/// </summary>
/// <remarks>
///     <para>
///         The rows are read one at a time and only the running total for each group is kept, so the memory
///         a summarising read needs is set by how many groups there are rather than by how many rows. A
///         table of a million rows that falls into a handful of groups costs a handful of totals.
///     </para>
///     <para>
///         A value that does not read as a number is left out of the arithmetic rather than taking the whole
///         group down with it, so one badly written cell only costs the row it sits in. A group that holds no
///         readable number at all summarises as nothing, which is how an unset cell reads everywhere else.
///     </para>
/// </remarks>
public static class TablePivot
{
    /// <summary>
    ///     How many decimal places an answer is rounded to before it is written, so a shared-out average
    ///     does not run on for ever.
    /// </summary>
    public const int DefaultDecimalPlaces = 6;

    /// <summary>
    ///     Separates the key values of a row into the string a group is looked up by. It is the ASCII unit
    ///     separator, which does not occur in text a person types.
    /// </summary>
    private const char KeySeparator = '\u001f';

    /// <summary>
    ///     Reads the source a row at a time, groups the rows by the key columns and writes one row per group
    ///     to the destination.
    /// </summary>
    /// <param name="source">The table to summarise.</param>
    /// <param name="destination">The table to write the groups to.</param>
    /// <param name="keyColumnNames">
    ///     The columns a row is grouped by, in the order they are to head the result. One column groups by
    ///     its own values; more than one groups by the combination.
    /// </param>
    /// <param name="valueColumnName">
    ///     The column whose values are summarised, or <see langword="null" /> to count the rows of each group
    ///     rather than read one of their columns.
    /// </param>
    /// <param name="aggregate">How the rows of a group are boiled down to the one value it is written with.</param>
    /// <param name="decimalPlaces">How many decimal places an arithmetic answer is rounded to.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>How many groups were written.</returns>
    public static async Task<int> PivotAsync(
        ITableRowSource source,
        ITableRowSink destination,
        IReadOnlyList<string> keyColumnNames,
        string? valueColumnName,
        AggregateKind aggregate,
        int decimalPlaces = DefaultDecimalPlaces,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(keyColumnNames);
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);

        if (keyColumnNames.Count == 0)
        {
            throw new ArgumentException("At least one column has to be grouped by.", nameof(keyColumnNames));
        }

        if (aggregate is not AggregateKind.Count && string.IsNullOrWhiteSpace(valueColumnName))
        {
            throw new ArgumentException(
                "A column has to be summarised unless the rows are only being counted.", nameof(valueColumnName));
        }

        var sourceColumns = await source.GetColumnsAsync(cancellationToken).ConfigureAwait(false);

        var keyIndices = keyColumnNames.Select(name => IndexOfColumn(sourceColumns, name)).ToArray();
        var valueIndex = string.IsNullOrWhiteSpace(valueColumnName)
            ? -1
            : IndexOfColumn(sourceColumns, valueColumnName!);

        var resultName = Describe(aggregate, valueColumnName);
        var resultType = ResultType(aggregate, valueIndex >= 0 ? sourceColumns[valueIndex] : null);

        var resultColumns = new List<TableColumn>(keyColumnNames.Count + 1);

        for (var index = 0; index < keyColumnNames.Count; index++)
        {
            resultColumns.Add(new TableColumn(keyColumnNames[index], sourceColumns[keyIndices[index]].EffectiveDataType));
        }

        resultColumns.Add(new TableColumn(resultName, resultType));

        // The order the groups are first met in is the order they are written in, so a summarising read
        // keeps the shape of the table it read rather than imposing an order of its own.
        var groups = new Dictionary<string, GroupAccumulator>(StringComparer.Ordinal);
        var order = new List<string>();

        await foreach (var row in source.ReadRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            var key = BuildKey(row, keyIndices);

            if (!groups.TryGetValue(key, out var group))
            {
                group = new GroupAccumulator([.. keyIndices.Select(index => ValueAt(row, index))]);

                groups[key] = group;
                order.Add(key);
            }

            group.Add(valueIndex >= 0 ? ValueAt(row, valueIndex) : null, aggregate);
        }

        await destination.BeginAsync(resultColumns, cancellationToken).ConfigureAwait(false);

        foreach (var key in order)
        {
            var group = groups[key];

            var written = new string?[keyColumnNames.Count + 1];
            group.KeyValues.CopyTo(written, 0);
            written[^1] = group.Result(aggregate, decimalPlaces, valueIndex >= 0);

            await destination.WriteRowAsync(written, cancellationToken).ConfigureAwait(false);
        }

        await destination.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return order.Count;
    }

    /// <summary>
    ///     Names the column a group's summarised value is written under.
    /// </summary>
    /// <param name="aggregate">How the rows of a group are summarised.</param>
    /// <param name="valueColumnName">The column that was summarised, when there was one.</param>
    public static string Describe(AggregateKind aggregate, string? valueColumnName)
    {
        var subject = string.IsNullOrWhiteSpace(valueColumnName) ? null : valueColumnName!.Trim();

        return aggregate switch
        {
            AggregateKind.Count => subject is null ? "Count" : $"Count of {subject}",
            AggregateKind.Sum => $"Sum of {subject}",
            AggregateKind.Average => $"Average of {subject}",
            AggregateKind.Minimum => $"Minimum of {subject}",
            AggregateKind.Maximum => $"Maximum of {subject}",
            _ => "Value",
        };
    }

    private static string BuildKey(IReadOnlyList<string?> row, IReadOnlyList<int> keyIndices)
    {
        return string.Join(KeySeparator, keyIndices.Select(index => ValueAt(row, index) ?? string.Empty));
    }

    private static string? ValueAt(IReadOnlyList<string?> row, int index)
    {
        return index >= 0 && index < row.Count ? row[index] : null;
    }

    private static int IndexOfColumn(IReadOnlyList<TableColumn> columns, string name)
    {
        for (var index = 0; index < columns.Count; index++)
        {
            if (string.Equals(columns[index].Name, name, StringComparison.Ordinal))
            {
                return index;
            }
        }

        throw new ArgumentException($"The table has no column called '{name}'.", nameof(columns));
    }

    private static ColumnDataType ResultType(AggregateKind aggregate, TableColumn? valueColumn)
    {
        return aggregate switch
        {
            AggregateKind.Count => ColumnDataType.Integer,

            // An average of whole numbers is rarely a whole number, so it is always written as a decimal.
            AggregateKind.Average => ColumnDataType.Decimal,

            // The rest keep the shape of what they read: whole numbers stay whole, everything else - a
            // column of text that happens to hold numbers included - is written as a decimal.
            _ => valueColumn is { } column && column.EffectiveDataType is ColumnDataType.Integer or ColumnDataType.Decimal
                ? column.EffectiveDataType
                : ColumnDataType.Decimal,
        };
    }

    /// <summary>
    ///     The running total for one group, so only the answer rather than the rows it came from is held.
    /// </summary>
    private sealed class GroupAccumulator(string?[] keyValues)
    {
        private int _rows;
        private int _numbers;
        private decimal _sum;
        private decimal _minimum;
        private decimal _maximum;

        public string?[] KeyValues { get; } = keyValues;

        public void Add(string? value, AggregateKind aggregate)
        {
            _rows++;

            // Counting every value of a column is not the same as counting the rows that hold one, so a
            // count over a named column leaves the empty cells out of it.
            if (aggregate is AggregateKind.Count && string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (aggregate is AggregateKind.Count)
            {
                _numbers++;
                return;
            }

            if (value is null
                || !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            {
                return;
            }

            if (_numbers == 0)
            {
                _minimum = number;
                _maximum = number;
            }
            else
            {
                _minimum = Math.Min(_minimum, number);
                _maximum = Math.Max(_maximum, number);
            }

            _sum += number;
            _numbers++;
        }

        public string? Result(AggregateKind aggregate, int decimalPlaces, bool hasValueColumn)
        {
            if (aggregate is AggregateKind.Count)
            {
                // A count over a column counts the values it holds; a count over no column counts the rows.
                var count = hasValueColumn ? _numbers : _rows;

                return count.ToString(CultureInfo.InvariantCulture);
            }

            if (_numbers == 0)
            {
                return null;
            }

            var answer = aggregate switch
            {
                AggregateKind.Sum => _sum,
                AggregateKind.Average => _sum / _numbers,
                AggregateKind.Minimum => _minimum,
                AggregateKind.Maximum => _maximum,
                _ => _sum,
            };

            return Math.Round(answer, decimalPlaces, MidpointRounding.AwayFromZero)
                .ToString(CultureInfo.InvariantCulture);
        }
    }
}

