﻿namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerSQLOutputOptions : ConverterHandlerBaseOptions
{
    public readonly Dictionary<string, string> QuoteTypes = new()
    {
        { "No Quotes", "" },
        { "Double Quotes (\")", "\"" },
        { "MySQL Quotes (`)", "`" },
        { "SQL Server Quotes ([])", "[" }
    };

    public string TableName { get; set; } = "table_name";

    public string SelectedQuoteType { get; set; } = "No Quotes";

    public bool InsertMultiRowsAtOnce { get; set; } = false;
}