using System;
using System.Threading.Tasks;
using TableConverter.DataModels.Dtos;
using TableConverter.Utilities;

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
    /// <param name="converterName">The converter type which was selected.</param>
    /// <returns>
    /// A Result indicating success or failure of the operation.
    /// </returns>
    public Result AddFile(string sourceFilePath, string converterName);

    /// <summary>
    /// Adds a file asynchronously to the application storage directory.
    /// Copies the file and stores its metadata.
    /// </summary>
    /// <param name="sourceFilePath">The full path of the file to be added.</param>
    /// <param name="converterName">The converter type which was selected.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a Result indicating success or failure of the operation.
    /// </returns>
    public Task<Result> AddFileAsync(string sourceFilePath, string converterName);

    /// <summary>
    /// Removes a file from the application storage directory (Synchronous).
    /// Deletes the file from disk and updates the metadata.
    /// </summary>
    /// <param name="fileInfo">
    /// The FileInfoDto representing the file to be removed.
    /// </param>
    /// <returns>
    /// A Result indicating success or failure of the operation.
    /// </returns>
    public Result RemoveFile(FileInfoDto fileInfo);

    /// <summary>
    /// Removes a file asynchronously from the application storage directory.
    /// </summary>
    /// <param name="fileInfo">
    /// The FileInfoDto representing the file to be removed.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a Result indicating success or failure of the operation.
    /// </returns>
    public Task<Result> RemoveFileAsync(FileInfoDto fileInfo);

    /// <summary>
    /// Loads the table data from the specified file.
    /// </summary>
    /// <param name="fileInfo">
    /// The FileInfoDto representing the file to load data from.
    /// </param>
    /// <returns>
    /// A Result containing the TableData object with the loaded data, or an error if the operation fails.
    /// </returns>
    public Result<TableData> LoadTableData(FileInfoDto fileInfo);

    /// <summary>
    /// Loads the table data asynchronously from the specified file.
    /// </summary>
    /// <param name="fileInfo">
    /// The FileInfoDto representing the file to load data from.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a Result with the TableData object with the loaded data, or an error if the operation fails.
    /// </returns>
    public Task<Result<TableData>> LoadTableDataAsync(FileInfoDto fileInfo);

    /// <summary>
    /// Saves the table data to the specified file.
    /// </summary>
    /// <param name="fileInfo">
    /// The FileInfoDto representing the file to save data to.
    /// </param>
    /// <param name="path">
    /// The full path where the file should be saved.
    /// </param>
    /// <param name="converterName">
    /// The converter type which was selected.
    /// </param>
    /// <returns>
    /// A Result indicating success or failure of the operation.
    /// </returns>
    public Result SaveTableData(FileInfoDto fileInfo, string path, string converterName);

    /// <summary>
    /// Saves the table data asynchronously to the specified file.
    /// </summary>
    /// <param name="fileInfo">
    /// The FileInfoDto representing the file to save data to.
    /// </param>
    /// <param name="path">
    /// The full path where the file should be saved.
    /// </param>
    /// <param name="converterName">
    /// The converter type which was selected.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a Result indicating success or failure of the operation.
    /// </returns>
    public Task<Result> SaveTableDataAsync(FileInfoDto fileInfo, string path, string converterName);
}