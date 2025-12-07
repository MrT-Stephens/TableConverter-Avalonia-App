namespace TableConverter.FileConverters.Interfaces;

/// <summary>
///     Provides metadata and handlers for a specific converter type.
/// </summary>
public interface IConverterProvider
{
    /// <summary>
    ///     Gets metadata describing the converter.
    /// </summary>
    public IConverterMetadata Metadata { get; }

    /// <summary>
    ///     Gets the input converter handler.
    /// </summary>
    public IConverterHandlerInput? InputHandler();

    /// <summary>
    ///     Gets the output converter handler.
    /// </summary>
    public IConverterHandlerOutput? OutputHandler();

    /// <summary>
    ///     Creates a new input converter handler.
    /// </summary>
    protected IConverterHandlerInput CreateInputHander();

    /// <summary>
    ///     Creates a new output converter handler.
    /// </summary>
    protected IConverterHandlerOutput CreateOutputHander();
}
