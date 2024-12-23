using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration;

/// <summary>
///     Abstract class to build a table-like structure with rules and customizations using a
///     <typeparamref name="TFaker" /> instance.
/// </summary>
/// <typeparam name="TFaker">The type of the Faker instance used to generate column values.</typeparam>
public abstract class FakerBuilderBase<TFaker>
{
    /// <summary>
    ///     A dictionary that holds the column names and their corresponding value generation functions.
    /// </summary>
    private readonly Dictionary<string, Func<TFaker, string>> _actions = new();

    private int _rowCount = 1;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FakerBuilderBase{TFaker}" /> class.
    /// </summary>
    /// <param name="fakerInstance">The Faker instance used for generating data.</param>
    protected FakerBuilderBase(TFaker fakerInstance)
    {
        FakerInstance = fakerInstance;
    }

    /// <summary>
    ///     Gets the Faker instance used for generating data.
    /// </summary>
    public TFaker FakerInstance { get; }

    /// <summary>
    ///     Sets the number of rows to generate during the build process.
    /// </summary>
    /// <param name="count">The number of rows to generate.</param>
    /// <returns>Returns the current <see cref="FakerBuilderBase{TFaker}" /> instance for method chaining.</returns>
    public FakerBuilderBase<TFaker> WithRowCount(int count)
    {
        _rowCount = count;
        return this;
    }

    /// <summary>
    ///     Adds a rule for generating data for a specific column.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="value">
    ///     A function that takes the <typeparamref name="TFaker" /> instance and returns a string value for the column.
    /// </param>
    /// <returns>Returns the current <see cref="FakerBuilderBase{TFaker}" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if a rule for the specified column name is already defined.
    /// </exception>
    public FakerBuilderBase<TFaker> Add(string columnName, Func<TFaker, string> value)
    {
        if (!_actions.TryAdd(columnName, value))
            throw new InvalidOperationException($"Rule already defined for property '{columnName}'.");

        return this;
    }

    /// <summary>
    ///     Adds a conditional rule for generating data for a specific column.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="condition">
    ///     A function that takes the current Faker instance and returns whether the rule should be applied.
    /// </param>
    /// <param name="value">
    ///     A function that generates the value if the condition is met.
    /// </param>
    /// <returns>Returns the current <see cref="FakerBuilderBase{TFaker}" /> instance for method chaining.</returns>
    public FakerBuilderBase<TFaker> AddConditional(string columnName, Func<TFaker, bool> condition,
        Func<TFaker, string> value)
    {
        return Add(columnName, faker => condition(faker) ? value(faker) : string.Empty);
    }


    /// <summary>
    ///     Builds and returns a <see cref="TableData" /> object with the generated columns and rows based on the defined
    ///     rules.
    /// </summary>
    /// <returns>
    ///     A <see cref="TableData" /> object containing the column names and generated rows.
    /// </returns>
    public TableData Build()
    {
        var rows = new List<string[]>();
        
        for (var i = 0; i < _rowCount; i++)
        {
            var row = new List<string>();

            foreach (var (_, action) in _actions)
            {
                row.Add(action(FakerInstance));
            }

            rows.Add(row.ToArray());
        }

        return new TableData(_actions.Keys.ToList(), rows);
    }
}