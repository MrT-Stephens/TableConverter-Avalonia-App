using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace TableConverter.Interfaces;

public interface IFilesDialogManager : ITopLevelAware
{
    public Task<IEnumerable<IStorageFile>?> OpenFileAsync(FilePickerOpenOptions options);
    
    public Task<IStorageFile?> SaveFileAsync(FilePickerSaveOptions options);
}