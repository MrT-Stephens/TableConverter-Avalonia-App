namespace TableConverter.FileConverters.DataModels
{
    public record TableData(
        List<string> headers,
        List<string[]> rows
    );
}
