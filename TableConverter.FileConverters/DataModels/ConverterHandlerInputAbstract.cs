using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.DataModels;

/// <summary>
///     An abstract base class for handling input conversion, with generic support for options of type
///     <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type of the options that extend <see cref="ConverterHandlerBaseOptions" />.</typeparam>
/// <remarks>
///     A handler implements a single method, <see cref="ReadStreamAsync" />, and every entry point funnels
///     into it: an import hands it the file and a destination, and the rows are parsed straight into that
///     destination rather than the whole file being held as a string or as a whole table in memory.
/// </remarks>
public abstract class ConverterHandlerInputAbstract<T> : IConverterHandlerInput
    where T : ConverterHandlerBaseOptions, new()
{
    /// <summary>
    ///     Gets or sets the options used for input conversion.
    /// </summary>
    public T? Options { get; set; } = typeof(T) == typeof(ConverterHandlerBaseOptions) ? null : new T();

    /// <summary>
    ///     Gets or sets the dynamic options for input conversion. Implements <see cref="IConverterHandlerInput.Options" />.
    /// </summary>
    dynamic? IConverterHandlerInput.Options
    {
        get => Options;
        set => Options = value;
    }

    /// <inheritdoc />
    public abstract Task<Result> ReadStreamAsync(
        Stream? stream,
        ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}