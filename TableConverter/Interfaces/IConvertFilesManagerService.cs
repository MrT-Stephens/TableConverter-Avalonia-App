using System.Threading.Tasks;

namespace TableConverter.Interfaces;

/// <summary>
/// Defines the contract for managing file conversions,
/// including adding, removing, loading, and saving file metadata.
/// </summary>
public interface IConvertFilesManager
{
    /// <summary>
    /// Adds a file to the application storage directory (Synchronous).
    /// Copies the file and stores its metadata.
    /// </summary>
    /// <param name="sourceFilePath">The full path of the file to be added.</param>
    void AddFile(string sourceFilePath);

    /// <summary>
    /// Adds a file asynchronously to the application storage directory.
    /// Copies the file and stores its metadata.
    /// </summary>
    /// <param name="sourceFilePath">The full path of the file to be added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddFileAsync(string sourceFilePath);

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
    /// Loads the file metadata from persistent storage.
    /// Typically called at application startup.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadFilesAsync();

    /// <summary>
    /// Saves the current file metadata to persistent storage.
    /// Ensures that the stored file list remains updated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveMetadataAsync();
}