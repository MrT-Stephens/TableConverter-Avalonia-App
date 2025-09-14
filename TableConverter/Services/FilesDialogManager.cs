using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using TableConverter.Interfaces;

namespace TableConverter.Services;

public class FilesDialogManager : IFilesDialogManager
{
    public async Task<IStorageFile?> OpenFileAsync(FilePickerOpenOptions options)
    {
        var window = ((ITopLevelAware)this).GetTopLevel();

        if (window is null)
        {
            throw new NullReferenceException("Main window not found");
        }
        
        var result = await window.StorageProvider.OpenFilePickerAsync(options);

        return result.Count > 0 ? result[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync(FilePickerSaveOptions options)
    {
        var window = ((ITopLevelAware)this).GetTopLevel();

        if (window is null)
        {
            throw new NullReferenceException("Main window not found");
        }
        
        var result = await window.StorageProvider.SaveFilePickerAsync(options);
        
        return result;
    }
}