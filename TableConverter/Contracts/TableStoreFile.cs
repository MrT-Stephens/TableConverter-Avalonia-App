using System.Text.RegularExpressions;

namespace TableConverter.Contracts;

/// <summary>
/// Naming conventions for the store files the table editor works with. A store is a SQLite database
/// holding the columns, rows and cells of a single table data document.
/// </summary>
public static partial class TableStoreFile
{
    /// <summary>
    /// The extension of a table store.
    /// </summary>
    public const string Extension = ".tcstore";

    /// <summary>
    /// The glob matching every table store.
    /// </summary>
    public const string SearchPattern = "*" + Extension;

    /// <summary>
    /// The name the application gives a store it created, which is a guid with the store extension.
    /// </summary>
    [GeneratedRegex("^[0-9a-f]{32}\\" + Extension + "$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ApplicationGeneratedName();

    /// <summary>
    /// Whether <paramref name="fileName" /> is a name the application generated for a store it owns.
    /// Cleanup is limited to these names so a store the user copied into the documents directory by
    /// hand is never deleted.
    /// </summary>
    /// <param name="fileName">The file name, without a directory.</param>
    public static bool IsApplicationGeneratedName(string fileName)
    {
        return ApplicationGeneratedName().IsMatch(fileName);
    }
}
