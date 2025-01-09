namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerSQLInputOptions : ConverterHandlerBaseOptions
{
    public readonly Dictionary<string, string> QuoteTypes = new()
    {
        { "No Quotes", "" },
        { "Double Quotes (\")", "\"" },
        { "MySQL Quotes (`)", "`" },
        { "SQL Server Quotes ([])", "[" }
    };

    public string SelectedQuoteType { get; set; } = "No Quotes";

    public bool HasColumnNames { get; set; } = true;
}