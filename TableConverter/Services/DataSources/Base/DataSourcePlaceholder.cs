namespace TableConverter.Services.DataSources.Base;

/// <summary>
/// The text a data source shows in a row or column whose real values have not been read from the store
/// yet.
/// </summary>
/// <remarks>
/// It is shared rather than spelled out at each call site so the UI can tell a placeholder apart from
/// a value the user typed.
/// </remarks>
public static class DataSourcePlaceholder
{
    public const string Text = "...";
}

