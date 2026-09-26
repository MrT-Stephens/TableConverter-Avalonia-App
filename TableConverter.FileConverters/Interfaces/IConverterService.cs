using TableConverter.Utilities;

namespace TableConverter.FileConverters.Interfaces;

public interface IConverterService
{
    public IEnumerable<IConverterMetadata> InputMetadata { get; } 
    
    public IEnumerable<IConverterMetadata> OutputMetadata { get; }
    
    public IEnumerable<string> InputNames { get; }
    
    public IEnumerable<string> OutputNames { get; }
    
    public IConverterMetadata GetInputMetadataByName(string name);
    
    public IConverterMetadata GetOutputMetadataByName(string name);
    
    public IConverterHandlerInput GetInputByName(string name);
    
    public IConverterHandlerOutput GetOutputByName(string name);

    /// <summary>
    ///     Reads the file at <paramref name="path" /> straight into <paramref name="sink" />, one row at
    ///     a time, so a large file is never held in memory as a whole table before it can be stored.
    /// </summary>
    /// <param name="name">The name of the input converter to use.</param>
    /// <param name="path">The file to read.</param>
    /// <param name="sink">The destination the parsed rows are written to.</param>
    /// <param name="progress">
    ///     Receives how far the import has got, or <see langword="null" /> if the caller does not want
    ///     progress. What it is told is up to the converter: a row count with a known total for a file
    ///     that can be parsed in one pass, or nothing at all for one that cannot be measured.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the import.</param>
    /// <exception cref="FileConverterException">The file could not be read or parsed.</exception>
    /// <remarks>
    ///     The converter owns the transaction on <paramref name="sink" />: it declares the headers, writes
    ///     each row and completes. A converter that fails part way through leaves the sink uncompleted,
    ///     so the destination discards what was written instead of keeping half a table.
    /// </remarks>
    public Task ImportFileAsync(string name, string path, ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Writes the table behind <paramref name="source" /> out to the file at <paramref name="path" />,
    ///     one row at a time, so a large table is never held in memory as a whole string before it can be
    ///     written.
    /// </summary>
    /// <param name="name">The name of the output converter to use.</param>
    /// <param name="path">The file to write.</param>
    /// <param name="source">The table to convert, supplied one row at a time.</param>
    /// <param name="progress">
    ///     Receives how far the export has got, or <see langword="null" /> if the caller does not want
    ///     progress. What it is told is up to the converter, most often the rows pulled from
    ///     <paramref name="source" />.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the export.</param>
    /// <exception cref="FileConverterException">The table could not be converted or written.</exception>
    public Task ExportFileAsync(string name, string path, ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);

    public TOptions? GetInputOptionsByName<TOptions>(string name);
    
    public TOptions? GetOutputOptionsByName<TOptions>(string name);
}