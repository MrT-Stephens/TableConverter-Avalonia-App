using System.Collections.Concurrent;
using TableConverter.FileConverters.Exceptions;
using TableConverter.FileConverters.Interfaces;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Services;

public class ConverterService : IConverterService
{
    #region Properties

    private readonly IEnumerable<IConverterProvider> _converterProviders;
    
    // This service is a singleton, so the caches must be safe for concurrent use.
    private readonly ConcurrentDictionary<string, Lazy<IConverterHandlerInput>> _inputHandlersCache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Lazy<IConverterHandlerOutput>> _outputHandlersCache = new(StringComparer.Ordinal);

    public IEnumerable<IConverterMetadata> InputMetadata => _converterProviders
        .Where(x => x.Metadata.Support.HasFlag(ConverterSupport.Input))
        .Select(x => x.Metadata);
    
    public IEnumerable<IConverterMetadata> OutputMetadata => _converterProviders
        .Where(x => x.Metadata.Support.HasFlag(ConverterSupport.Output))
        .Select(x => x.Metadata);

    public IEnumerable<string> InputNames => InputMetadata.Select(x => x.Name);
    
    public IEnumerable<string> OutputNames => OutputMetadata.Select(x => x.Name);
    
    #endregion

    #region Constructors

    public ConverterService(IEnumerable<IConverterProvider> providers)
    {
        _converterProviders = providers;
    }

    #endregion

    #region IConverterService Methods

    public IConverterMetadata GetInputMetadataByName(string name)
    {
        var inputMetadata = InputMetadata.FirstOrDefault(x => x.Name == name);

        if (inputMetadata is null)
        {
            throw new InvalidOperationException($"Input {name} is not found.");
        }

        return inputMetadata;
    }

    public IConverterMetadata GetOutputMetadataByName(string name)
    {
        var outputMetadata = OutputMetadata.FirstOrDefault(x => x.Name == name);

        if (outputMetadata is null)
        {
            throw new InvalidOperationException($"Output {name} is not found.");
        }
        
        return outputMetadata;
    }

    public IConverterHandlerInput GetInputByName(string name)
    {
        // GetOrAdd + Lazy guarantees the handler is created exactly once, even under concurrency.
        return _inputHandlersCache.GetOrAdd(name, CreateInputHandlerLazy).Value;
    }

    public IConverterHandlerOutput GetOutputByName(string name)
    {
        return _outputHandlersCache.GetOrAdd(name, CreateOutputHandlerLazy).Value;
    }

    private Lazy<IConverterHandlerInput> CreateInputHandlerLazy(string name)
    {
        return new Lazy<IConverterHandlerInput>(
            () => CreateInputHandler(name),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private Lazy<IConverterHandlerOutput> CreateOutputHandlerLazy(string name)
    {
        return new Lazy<IConverterHandlerOutput>(
            () => CreateOutputHandler(name),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private IConverterHandlerInput CreateInputHandler(string name)
    {
        var provider = _converterProviders.FirstOrDefault(x => x.Metadata.Name == name);

        if (provider is null || !provider.Metadata.Support.HasFlag(ConverterSupport.Input))
        {
            throw new InvalidOperationException($"Input {name} is not supported");
        }

        return provider.InputHandler()
            ?? throw new InvalidOperationException($"Input {name} is not supported");
    }

    private IConverterHandlerOutput CreateOutputHandler(string name)
    {
        var provider = _converterProviders.FirstOrDefault(x => x.Metadata.Name == name);

        if (provider is null || !provider.Metadata.Support.HasFlag(ConverterSupport.Output))
        {
            throw new InvalidOperationException($"Output {name} is not supported");
        }

        return provider.OutputHandler()
            ?? throw new InvalidOperationException($"Output {name} is not supported");
    }

    public async Task ImportFileAsync(
        string name,
        string path,
        ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);

        var converter = GetInputByName(name);

        await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var result = await converter.ReadStreamAsync(stream, sink, progress, cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess is false)
        {
            throw new FileConverterException(name, result.Error ?? string.Empty);
        }

        // A converter that reported success but never completed the sink would leave the destination
        // silently empty. Catching it here turns a quiet data loss into a reported failure.
        if (sink.IsCompleted is false)
        {
            throw new FileConverterException(
                name, "The file was read but the table was not finished, so nothing was stored.");
        }
    }

    public async Task ExportFileAsync(
        string name,
        string path,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var converter = GetOutputByName(name);

        await using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);

        var result = await converter.ConvertToStreamAsync(stream, source, progress, cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess is false)
        {
            throw new FileConverterException(name, result.Error ?? string.Empty);
        }
    }

    public TOptions? GetInputOptionsByName<TOptions>(string name)
    {
        var converter = GetInputByName(name);
        return (TOptions?)converter.Options;
    }

    public TOptions? GetOutputOptionsByName<TOptions>(string name)
    {
        var converter = GetOutputByName(name);
        return (TOptions?)converter.Options;
    }

    #endregion
}