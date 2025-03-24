using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using TableConverter.DataModels;
using TableConverter.FileConverters.DataModels;
using TableConverter.Interfaces;

namespace TableConverter.Services;

public class FilesDialogManagerService : IFilesDialogManagerService, ITopLevelAware
{
    public async Task<Result<FileDialogManagerFile>?> OpenFileAsync(FilePickerOpenOptions options)
    {
        var window = ITopLevelAware.GetTopLevel();
        
        if (window is null)
        {
            return Result<FileDialogManagerFile>.Failure("Main window not found");
        }

        try
        {
            var result = await window.StorageProvider.OpenFilePickerAsync(options);

            if (result.Count > 0)
            {
                return Result<FileDialogManagerFile>.Success(new FileDialogManagerFile(
                    result[0].Name,
                    result[0].Path,
                    await result[0].OpenReadAsync()
                ));
            }
        }
        catch (Exception ex)
        {
            return Result<FileDialogManagerFile>.Failure($"Error occurred while opening file. (Exception: {ex.Message})");
        }

        return null;
    }

    public async Task<Result<FileDialogManagerFile>?> SaveFileAsync(FilePickerSaveOptions options)
    {
        var window = ITopLevelAware.GetTopLevel();
        
        if (window is null)
        {
            return Result<FileDialogManagerFile>.Failure("Main window not found");
        }

        try
        {
            var result = await window.StorageProvider.SaveFilePickerAsync(options);

            if (result is not null)
            {
                return Result<FileDialogManagerFile>.Success(new FileDialogManagerFile(
                    result.Name,
                    result.Path,
                    await result.OpenWriteAsync()
                ));
            }
        }
        catch (Exception ex)
        {
            return Result<FileDialogManagerFile>.Failure($"Error occurred while saving file. (Exception: {ex.Message})");
        }

        return null;
    }
}