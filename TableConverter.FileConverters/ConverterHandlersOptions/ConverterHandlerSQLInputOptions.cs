namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerSQLInputOptions : ConverterHandlerBaseOptions
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

    public QuoteStyles SelectedQuoteType { get; set; } = QuoteStyles.None;

    public bool HasColumnNames { get; set; } = true;
}