using System.Threading.Tasks;

namespace TableConverter.Interfaces;

/// <summary>
/// Defines the contract for managing file conversions,
/// including adding, removing, loading, and saving file metadata.
/// </summary>
public interface IConvertFilesManagerService
{
    /// <summary>
    /// Adds a file to the application storage directory (Synchronous).
    /// Copies the file and stores its metadata.
    /// </summary>
    /// <param name="sourceFilePath">The full path of the file to be added.</param>
    /// <param name="converterName">The converter type which was selected.</param>
    void AddFile(string sourceFilePath, string converterName);

    /// <summary>
    /// Adds a file asynchronously to the application storage directory.
    /// Copies the file and stores its metadata.
    /// </summary>
    /// <param name="sourceFilePath">The full path of the file to be added.</param>
    /// <param name="converterName">The converter type which was selected.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddFileAsync(string sourceFilePath, string converterName);

    /// <summary>
    /// Removes a file from the application storage directory (Synchronous).
    /// Deletes the file from disk and updates the metadata.
    /// </summary>
    /// <param name="id">The file id representing the file to be removed.</param>
    void RemoveFile(string id);

    /// <summary>
    /// Removes a file asynchronously from the application storage directory.
    /// Deletes the file from disk and updates the metadata.
    /// </summary>
    /// <param name="id">The file id representing the file to be removed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveFileAsync(string id);
    
    /// <summary>
    /// Loads the files from the application storage directory (Synchronous).
    /// </summary>
    /// <param name="id">The file id representing the file to be loaded.</param>
    /// <returns>The text content of the file.</returns>
    string LoadAllText(string id);
    
    /// <summary>
    /// Loads the files from the application storage directory (Asynchronous).
    /// </summary>
    /// <param name="id">The file id representing the file to be loaded.</param>
    /// <returns>The text content of the file.</returns>
    Task<string> LoadAllTextAsync(string id);
}