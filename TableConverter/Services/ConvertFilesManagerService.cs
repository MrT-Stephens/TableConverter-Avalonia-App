using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public partial class ConvertFilesManagerService : ObservableObject, IConvertFilesManager
{
    private static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Application.Current?.Name ??
            throw new ArgumentNullException(nameof(Application.Current.Name), "Name of the application is null"));

    private static readonly string AppFilesPath = Path.Combine(AppDataPath, "Files");

    [ObservableProperty] private ObservableCollection<ConvertDocumentViewModel> _Files = [];

    public void AddFile(string sourceFilePath)
    {
        
    }

    public Task AddFileAsync(string sourceFilePath)
    {
        throw new NotImplementedException();
    }

    public void RemoveFile(string id)
    {
        throw new NotImplementedException();
    }

    public Task RemoveFileAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task LoadFilesAsync()
    {
        throw new NotImplementedException();
    }

    public Task SaveMetadataAsync()
    {
        throw new NotImplementedException();
    }
}