namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerSQLOutputOptions : ConverterHandlerBaseOptions
{
    public readonly Dictionary<QuoteStyles, string> QuoteTypes = new()
    {
        { QuoteStyles.None, "" },
        { QuoteStyles.DoubleQuotes, "\"" },
        { QuoteStyles.MySqlQuotes, "`" },
        { QuoteStyles.SqlServerQuotes, "[" }
    };

    public enum QuoteStyles
    {
        None,
        DoubleQuotes,
        MySqlQuotes,
        SqlServerQuotes,
    }

    public string TableName { get; set; } = "table_name";

    public QuoteStyles SelectedQuoteType { get; set; } = QuoteStyles.None;

    public bool InsertMultiRowsAtOnce { get; set; } = false;
}