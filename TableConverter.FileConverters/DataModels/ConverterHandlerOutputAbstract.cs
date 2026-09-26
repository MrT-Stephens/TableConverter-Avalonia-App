using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.DataModels;

/// <summary>
///     An abstract base class for handling output conversion, with generic support for options of type
///     <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type of the options that extend <see cref="ConverterHandlerBaseOptions" />.</typeparam>
/// <remarks>
///     A handler implements a single method, <see cref="ConvertToStreamAsync" />, which pulls the table from
///     the source and writes it straight to the stream, so a large table is never held in memory as a whole
///     and never turned into one giant string first.
/// </remarks>
public abstract class ConverterHandlerOutputAbstract<T> : IConverterHandlerOutput
    where T : ConverterHandlerBaseOptions, new()
{
    /// <summary>
    ///     Gets or sets the options used for output conversion.
    /// </summary>
    public T? Options { get; set; } = typeof(T) == typeof(ConverterHandlerBaseOptions) ? null : new T();

    /// <summary>
    ///     Gets or sets the dynamic options for output conversion. Implements <see cref="IConverterHandlerOutput.Options" />.
    /// </summary>
    dynamic? IConverterHandlerOutput.Options
    {
        get => Options;
        set => Options = value;
    }

    /// <inheritdoc />
    public abstract Task<Result> ConvertToStreamAsync(
        Stream? stream,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}