using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Contracts;
using TableConverter.Utilities;
using TableConverter.ViewModels;
using DataGenerationFieldViewModel = TableConverter.ViewModels.Models.DataGenerationFieldViewModel;

namespace TableConverter.Interfaces;

public interface IDataGenerationTypes
{
    /// <summary>
    /// Gets the list of available data generation types.
    /// </summary>
    public IReadOnlyList<DataGenerationType> Types { get; }

    /// <summary>
    /// Gets the list of available locales for data generation.
    /// </summary>
    public IReadOnlyList<string> AvailableLocales { get; }

    /// <summary>
    /// Sets the locale for data generation.
    /// </summary>
    /// <param name="locale">
    /// The locale to set for data generation.
    /// </param>
    public void SetLocale(string locale);

    /// <summary>
    /// Sets the seed for data generation. A seed allows for reproducible data generation.
    /// </summary>
    /// <param name="seed">
    /// The seed value to use for data generation.
    /// </param>
    public void SetSeed(int seed);

    /// <summary>
    /// Generates data based on the provided fields and row count.
    /// </summary>
    /// <param name="fields">
    /// An array of <see cref="DataGenerationFieldViewModel"/> that defines the fields to generate data for.
    /// </param>
    /// <param name="rowCount">
    /// The number of rows to generate. If set to 0, the default row count will be used.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation. The task result contains a <see cref="Result{TableData}"/>
    /// </returns>
    public Task<Result<TableData>> GenerateData(DataGenerationFieldViewModel[] fields, int rowCount = 0);
}