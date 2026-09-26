namespace TableConverter.Contracts;

/// <summary>
/// What a table data document needs in order to reopen itself: the store that holds its table, the
/// title it was shown with, and whether the store is scratch data the application may delete.
/// </summary>
/// <param name="Path">The absolute path of the document's table store.</param>
/// <param name="Title">The title the document was shown with.</param>
/// <param name="IsTemporaryStore">
/// <see langword="true" /> when the store was created by the application (and can therefore be
/// deleted by it), <see langword="false" /> when the user opened an existing file.
/// </param>
public sealed record TableDataDocumentState(string Path, string Title, bool IsTemporaryStore);

