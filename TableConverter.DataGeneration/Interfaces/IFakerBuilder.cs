using TableConverter.DataGeneration.DataModels;
using TableConverter.Utilities;
using TableConverter.Utilities.Models;

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
    ///     Adds a rule for generating data for a specific column, declaring the kind of value it holds.
    /// </summary>
    /// <param name="columnName">The name of the column for which the rule applies.</param>
    /// <param name="dataType">The kind of value the generator produces, which the column is declared with.</param>
    /// <param name="valueGenerator">
    ///     A function or rule that takes a <typeparamref name="TFaker" /> instance and generates the column's value.
    /// </param>
    /// <param name="blanksPercentage">
    ///     Specifies the percentage (0-100) of rows in which this column will have a blank value. Defaults to 0.
    /// </param>
    /// <returns>
    ///     Returns the current builder instance for method chaining, allowing additional column rules to be added.
    /// </returns>
    /// <remarks>
    ///     Declaring the type is what keeps a generated table from being nothing but text. The column
    ///     carries this type from the first row, rather than having one read off its values the way an
    ///     imported column does, because the builder knows what its generators produce.
    /// </remarks>
    IFakerBuilder<TFaker> Add(string columnName, ColumnDataType dataType, Func<TFaker, string> valueGenerator, int blanksPercentage = 0);

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
    /// <param name="dataType">
    ///     The kind of value the generator produces, which the column is declared with. Defaults to text.
    /// </param>
    /// <returns>
    ///     Returns the current builder instance for method chaining, allowing further configuration.
    /// </returns>
    IFakerBuilder<TFaker> AddConditional(string columnName, Func<TFaker, bool> condition,
        Func<TFaker, string> valueGenerator, int blankValuePercentage = 0,
        ColumnDataType dataType = ColumnDataType.Text);

    /// <summary>
    ///     Generates the configured rows and writes them to <paramref name="sink" />, one row at a time.
    /// </summary>
    /// <param name="sink">The destination the generated rows are written to.</param>
    /// <param name="cancellationToken">Token used to cancel the generation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     The builder owns the call order on <paramref name="sink" />: it declares the headers, writes
    ///     each row and completes. A generator that throws part way through leaves the sink uncompleted,
    ///     so the destination discards what was written rather than keeping half a table.
    /// </remarks>
    Task BuildAsync(ITableRowSink sink, CancellationToken cancellationToken = default);
}