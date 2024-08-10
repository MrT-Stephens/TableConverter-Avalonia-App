namespace TableConverter.DataGeneration.DataModels
{
    public record TableData(
        List<string> headers,
        List<string[]> rows
    );
}
