using System.Collections.Generic;

namespace TableConverter.Interfaces;

/// <summary>
/// Owns the store files that hold table data documents, so a store can be deleted the way SQLite
/// needs and stale ones can be cleaned up.
/// </summary>
/// <remarks>
/// This is where the assumption that a table is stored as a SQLite file lives. A document type which
/// stores its data differently would use its own service instead of this one.
/// </remarks>
public interface ITableStoreFiles
{
    /// <summary>
    /// Deletes a table store together with its SQLite side car files.
    /// </summary>
    void Delete(string path);

    /// <summary>
    /// Deletes the stores the application created on previous runs that no open document references
    /// any more. Only files with an application generated name are ever removed.
    /// </summary>
    void PruneOrphaned(IEnumerable<string> keepPaths);
}

