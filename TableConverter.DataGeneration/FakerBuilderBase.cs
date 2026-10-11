using TableConverter.DataGeneration.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Models;

namespace TableConverter.DataGeneration;

/// <summary>
///     Abstract class to build a table-like structure with rules and customizations using a
///     <typeparamref name="TFaker" /> instance.
/// </summary>
/// <typeparam name="TFaker">The type of the Faker instance used to generate column values.</typeparam>
public abstract class FakerBuilderBase<TFaker>(TFaker fakerInstance) : IFakerBuilder<TFaker> where TFaker : FakerBase
{
    /// <summary>
    ///     A column's value generator together with the kind of value it produces.
    /// </summary>
    /// <param name="DataType">The kind of value the generator produces, which the column is declared with.</param>
    /// <param name="Generate">The function that produces one value for the column.</param>
    protected readonly record struct ColumnGenerator(ColumnDataType DataType, Func<TFaker, string> Generate);

    /// <summary>
    ///     A dictionary that holds the column names and their corresponding value generation functions.
    ///     Each column name maps to a list of value generators to allow multiple columns with the same name.
    /// </summary>
    protected readonly Dictionary<string, List<ColumnGenerator>> _actions = new();

    /// <summary>
    ///     Number of rows to generate during the build process. Default is 1.
    /// </summary>
    private int _RowCount = 1;

    /// <summary>
    ///     Gets the Faker instance used for generating data.
    /// </summary>
    public TFaker FakerInstance { get; } = fakerInstance;

    /// <inheritdoc />
    public IFakerBuilder<TFaker> WithRowCount(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Row count must be greater than zero.");

        _RowCount = count;
        return this;
    }

    /// <inheritdoc />
    public IFakerBuilder<TFaker> Add(string columnName, Func<TFaker, string> valueGenerator, int blanksPercentage = 0)
    {
        return Add(columnName, ColumnDataType.Text, valueGenerator, blanksPercentage);
    }

    /// <inheritdoc />
    public IFakerBuilder<TFaker> Add(string columnName, ColumnDataType dataType, Func<TFaker, string> valueGenerator,
        int blanksPercentage = 0)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            columnName = $"Column_{_actions.SelectMany(kvp => kvp.Value).Count() + 1}";

        if (valueGenerator is null)
            throw new ArgumentNullException(nameof(valueGenerator));
        if (blanksPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(blanksPercentage), blanksPercentage,
                "Blanks percentage must be between 0 and 100.");

        // A name can be given to more than one column, so each name collects its generators in a list.
        if (!_actions.TryGetValue(columnName, out var generators))
            _actions[columnName] = generators = [];

        // Blanks are worked out here rather than by the generator, so that every way of adding a column
        // blanks its values the same way.
        generators.Add(new ColumnGenerator(dataType, faker =>
            faker.Randomizer.Number(0, 100) < blanksPercentage ? string.Empty : valueGenerator(faker)));

        return this;
    }

    /// <inheritdoc />
    public IFakerBuilder<TFaker> AddConditional(string columnName, Func<TFaker, bool> condition,
        Func<TFaker, string> valueGenerator, int blankValuePercentage = 0,
        ColumnDataType dataType = ColumnDataType.Text)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            columnName = $"Column-{_actions.SelectMany(kvp => kvp.Value).Count() + 1}";

        if (condition is null)
            throw new ArgumentNullException(nameof(condition));
        if (valueGenerator is null)
            throw new ArgumentNullException(nameof(valueGenerator));
        if (blankValuePercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(blankValuePercentage), blankValuePercentage,
                "Blank value percentage must be between 0 and 100.");

        // The blanks are left to the column, so a conditional column blanks its values the same way any
        // other column does.
        return Add(columnName, dataType, faker => condition(faker) ? valueGenerator(faker) : string.Empty,
            blankValuePercentage);
    }

    /// <inheritdoc />
    public async Task BuildAsync(ITableRowSink sink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);

        // Flattened once, so the headings and every row are built from the same order.
        var generators = _actions
            .SelectMany(pair => pair.Value.Select(generator => (Name: pair.Key, Generator: generator)))
            .ToList();

        // Every column names the kind of value it holds, because the builder knows what its generators
        // produce. A generated table is therefore not one whose types have to be read off its values the
        // way an imported one is.
        var columns = generators.Select(entry => new TableColumn(entry.Name, entry.Generator.DataType)).ToList();

        await sink.BeginAsync(columns, cancellationToken).ConfigureAwait(false);

        // Rows are generated and handed over one at a time rather than gathered into a whole table
        // first, so the memory a build needs does not grow with the row count.
        for (var i = 0; i < _RowCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = generators.Select(entry => entry.Generator.Generate(FakerInstance)).ToList();

            await sink.WriteRowAsync(row, cancellationToken).ConfigureAwait(false);
        }

        await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);
    }
}