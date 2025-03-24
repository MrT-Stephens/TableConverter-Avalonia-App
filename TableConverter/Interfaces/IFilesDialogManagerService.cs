using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using TableConverter.DataModels;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.Interfaces;

public interface IFilesDialogManagerService
{
    public Task<Result<FileDialogManagerFile>?> OpenFileAsync(FilePickerOpenOptions options);
    
    public Task<Result<FileDialogManagerFile>?> SaveFileAsync(FilePickerSaveOptions options);
}