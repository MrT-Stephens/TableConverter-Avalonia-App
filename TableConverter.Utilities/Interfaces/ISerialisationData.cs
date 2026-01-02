namespace TableConverter.Utilities.Interfaces;

public interface ISerialisationData
{
    public void ExportState(IDictionary<string, object> data);
    
    public void ImportState(IDictionary<string, object> data);
}