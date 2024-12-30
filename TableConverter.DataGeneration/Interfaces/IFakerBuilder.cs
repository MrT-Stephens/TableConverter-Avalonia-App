using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.Interfaces;

/// <summary>
///     Interface for building a table-like structure with rules and customizations.
/// </summary>
/// <typeparam name="TFaker">The type of the Faker instance used for generating column values.</typeparam>
public interface IFakerBuilder<out TFaker> where TFaker : IFaker
{
    /// <summary>
    ///     Sets the number of rows to generate during the build process.
    /// </summary>
    /// <param name="count">The number of rows to generate.</param>
    /// <returns>
    ///     Returns the current builder instance for method chaining, allowing further configuration.
    /// </returns>
    IFakerBuilder<TFaker> WithRowCount(int count);

    /// <summary>
    ///     Adds a rule for generating data for a specific column.
    /// </summary>
    /// <param name="columnName">The name of the column for which the rule applies.</param>
    /// <param name="valueGenerator">
    ///     A function or rule that takes a <typeparamref name="TFaker" /> instance and generates the column's value.
    /// </param>
    /// <param name="blacksPercentage">
    ///     Specifies the percentage (0-100) of rows in which this column will have a blank value. Defaults to 0.
    /// </param>
    /// <returns>
    ///     Returns the current builder instance for method chaining, allowing additional column rules to be added.
    /// </returns>
    IFakerBuilder<TFaker> Add(string columnName, Func<TFaker, string> valueGenerator, int blacksPercentage = 0);

    /// <summary>
    ///     Adds a conditional rule for generating data for a specific column.
    /// </summary>
    /// <param name="columnName">The name of the column for which the rule applies.</param>
    /// <param name="condition">
    ///     A function that takes a <typeparamref name="TFaker" /> instance and returns a boolean indicating whether
    ///     the rule should be applied.
    /// </param>
    /// <param name="valueGenerator">
    ///     A function or rule that generates the column's value if the <paramref name="condition" /> is met.
    /// </param>
    /// <param name="blankValuePercentage">
    ///     Specifies the percentage (0-100) of rows in which this column will have a blank value. Defaults to 0.
    /// </param>
    /// <returns>
    ///     Returns the current builder instance for method chaining, allowing further configuration.
    /// </returns>
    IFakerBuilder<TFaker> AddConditional(string columnName, Func<TFaker, bool> condition,
        Func<TFaker, string> valueGenerator, int blankValuePercentage = 0);

    /// <summary>
    ///     Builds and returns a <see cref="TableData" /> object with the generated columns and rows based on the defined
    ///     rules.
    /// </summary>
    /// <returns>
    ///     A <see cref="TableData" /> object containing the column names and generated rows.
    /// </returns>
    TableData Build();

    /// <summary>
    ///     Asynchronously builds and returns a <see cref="TableData" /> object with the generated columns and rows
    ///     based on the defined rules.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the generated <see cref="TableData" />.
    /// </returns>
    Task<TableData> BuildAsync();
}