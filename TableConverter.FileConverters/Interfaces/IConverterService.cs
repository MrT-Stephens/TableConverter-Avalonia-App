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

    public TableData InputFile(string name, string path);
    
    public Task<TableData> InputFileAsync(string name, string path);
    
    public void OutputFile(string name, string path, TableData tableData);
    
    public Task OutputFileAsync(string name, string path, TableData tableData);

    public TOptions? GetInputOptionsByName<TOptions>(string name);
    
    public TOptions? GetOutputOptionsByName<TOptions>(string name);
}