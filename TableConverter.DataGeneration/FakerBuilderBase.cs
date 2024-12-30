using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration;

/// <summary>
///     Abstract class to build a table-like structure with rules and customizations using a
///     <typeparamref name="TFaker" /> instance.
/// </summary>
/// <typeparam name="TFaker">The type of the Faker instance used to generate column values.</typeparam>
public abstract class FakerBuilderBase<TFaker>(TFaker fakerInstance) : IFakerBuilder<TFaker> where TFaker : FakerBase
{
    /// <summary>
    ///     A dictionary that holds the column names and their corresponding value generation functions.
    ///     Each column name maps to a list of value generators to allow multiple columns with the same name.
    /// </summary>
    protected readonly Dictionary<string, List<Func<TFaker, string>>> _actions = new();

    /// <summary>
    ///     Number of rows to generate during the build process. Default is 1.
    /// </summary>
    private int _rowCount = 1;

    /// <summary>
    ///     Gets the Faker instance used for generating data.
    /// </summary>
    public TFaker FakerInstance { get; } = fakerInstance;

    /// <inheritdoc />
    public IFakerBuilder<TFaker> WithRowCount(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Row count must be greater than zero.");

        _rowCount = count;
        return this;
    }

    /// <inheritdoc />
    public IFakerBuilder<TFaker> Add(string columnName, Func<TFaker, string> valueGenerator, int blanksPercentage = 0)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            columnName = $"Column-{_actions.SelectMany(kvp => kvp.Value).Count() + 1}";
        
        if (valueGenerator is null)
            throw new ArgumentNullException(nameof(valueGenerator));
        if (blanksPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(blanksPercentage), blanksPercentage,
                "Blanks percentage must be between 0 and 100.");

        // Add the value generator to the list for the column name
        if (!_actions.TryGetValue(columnName, out var generators))
        {
            generators = [];
            _actions[columnName] = generators;
        }

        generators.Add(AdjustedGenerator);

        return this;

        // Adjusted generator to handle blanks percentage
        string AdjustedGenerator(TFaker faker)
        {
            return faker.Randomizer.Number(0, 100) < blanksPercentage ? string.Empty : valueGenerator(faker);
        }
    }

    /// <inheritdoc />
    public IFakerBuilder<TFaker> AddConditional(string columnName, Func<TFaker, bool> condition,
        Func<TFaker, string> valueGenerator, int blankValuePercentage = 0)
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

        return Add(columnName, faker =>
        {
            if (faker.Randomizer.Number(0, 100) < blankValuePercentage || !condition(faker))
                return string.Empty;

            return valueGenerator(faker);
        });
    }

    /// <inheritdoc />
    public TableData Build()
    {
        var rows = new List<string[]>();

        for (var i = 0; i < _rowCount; i++)
        {
            var row = new List<string>();

            foreach (var (columnName, actions) in _actions)
                row.AddRange(actions.Select(action => action(FakerInstance)));

            rows.Add(row.ToArray());
        }

        // Flatten the column names to match the number of generators for each column
        var columnHeaders = _actions.SelectMany(pair => pair.Value.Select(_ => pair.Key)).ToList();

        return new TableData(columnHeaders, rows);
    }

    /// <inheritdoc />
    public async Task<TableData> BuildAsync()
    {
        var rows = await Task.WhenAll(Enumerable.Range(0, _rowCount).Select(_ =>
        {
            var row = new List<string>();

            foreach (var (columnName, actions) in _actions)
                row.AddRange(actions.Select(action => action(FakerInstance)));

            return Task.FromResult(row.ToArray());
        }));

        // Flatten the column names to match the number of generators for each column
        var columnHeaders = _actions.SelectMany(pair => pair.Value.Select(_ => pair.Key)).ToList();

        return new TableData(columnHeaders, rows.ToList());
    }
}