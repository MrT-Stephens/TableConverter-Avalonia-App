using System.Text;
using TableConverter.FileConverters.Exceptions;
using TableConverter.FileConverters.Interfaces;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Services;

public class ConverterService : IConverterService
{
    #region Properties

    private readonly IEnumerable<IConverterProvider> _converterProviders;
    
    private readonly Dictionary<string, IConverterHandlerInput> _inputHandlersCache;
    private readonly Dictionary<string, IConverterHandlerOutput> _outputHandlersCache;

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
        
        _inputHandlersCache = new Dictionary<string, IConverterHandlerInput>();
        _outputHandlersCache = new Dictionary<string, IConverterHandlerOutput>();
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
        if (_inputHandlersCache.TryGetValue(name, out var inputHandler))
        {
            return inputHandler;
        }
        
        var provider = _converterProviders.FirstOrDefault(x => x.Metadata.Name == name);

        if (provider is null || !provider.Metadata.Support.HasFlag(ConverterSupport.Input))
        {
            throw new InvalidOperationException($"Input {name} is not supported");
        }

        var converter = provider.InputHandler()
            ?? throw new NullReferenceException($"Input {name} is not supported");
        
        _inputHandlersCache[name] = converter;
        return converter;
    }

    public IConverterHandlerOutput GetOutputByName(string name)
    {
        if (_outputHandlersCache.TryGetValue(name, out var outputHandler))
        {
            return outputHandler;
        }
        
        var provider = _converterProviders.FirstOrDefault(x => x.Metadata.Name == name);

        if (provider is null || !provider.Metadata.Support.HasFlag(ConverterSupport.Output))
        {
            throw new InvalidOperationException($"Output {name} is not supported");
        }

        var converter = provider.OutputHandler()
            ?? throw new NullReferenceException($"Output {name} is not supported");
        
        _outputHandlersCache[name] = converter;
        return converter;
    }

    public TableData InputFile(string name, string path)
    {
        var converter = GetInputByName(name);
        
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var readResult = converter.ReadFile(stream);

        if (readResult.IsSuccess is false)
        {
            throw new FileConverterException(name, readResult.Error ?? string.Empty);
        }

        var parseResult = converter.ReadText(readResult.Value);

        if (parseResult.IsSuccess is false)
        {
            throw new FileConverterException(name, parseResult.Error ?? string.Empty);
        }
        
        return parseResult.Value;
    }

    public async Task<TableData> InputFileAsync(string name, string path)
    {
        var converter = GetInputByName(name);

        await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var readResult = await converter.ReadFileAsync(stream);

        if (readResult.IsSuccess is false)
        {
            throw new FileConverterException(name, readResult.Error ?? string.Empty);
        }

        var parseResult = await converter.ReadTextAsync(readResult.Value);

        if (parseResult.IsSuccess is false)
        {
            throw new FileConverterException(name, parseResult.Error ?? string.Empty);
        }
        
        return parseResult.Value;
    }

    public void OutputFile(string name, string path, TableData tableData)
    {
        var converter = GetOutputByName(name);
        
        var converterResult = converter.Convert(tableData.Headers.ToArray(), tableData.Rows.ToArray());

        if (converterResult.IsSuccess is false)
        {
            throw new FileConverterException(name, converterResult.Error ?? string.Empty);
        }
        
        using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        var text = Encoding.UTF8.GetBytes(converterResult.Value);
        
        var writeResult = converter.SaveFile(stream, text);

        if (writeResult.IsSuccess is false)
        {
            throw new FileConverterException(name, writeResult.Error ?? string.Empty);
        }
    }

    public async Task OutputFileAsync(string name, string path, TableData tableData)
    {
        var converter = GetOutputByName(name);
        
        var converterResult = await converter.ConvertAsync(tableData.Headers.ToArray(), tableData.Rows.ToArray());

        if (converterResult.IsSuccess is false)
        {
            throw new FileConverterException(name, converterResult.Error ?? string.Empty);
        }

        await using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        var text = Encoding.UTF8.GetBytes(converterResult.Value);
        
        var writeResult = await converter.SaveFileAsync(stream, text);

        if (writeResult.IsSuccess is false)
        {
            throw new FileConverterException(name, writeResult.Error ?? string.Empty);
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