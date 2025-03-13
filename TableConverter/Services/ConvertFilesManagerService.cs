using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public partial class ConvertFilesManagerService(ConverterTypesService types)
    : ObservableObject, IConvertFilesManagerService
{
    private static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Application.Current?.Name ??
            throw new ArgumentNullException(nameof(Application.Current.Name), "Name of the application is null"));

    private static readonly string AppFilesPath = Path.Combine(AppDataPath, "Files");
    
    protected readonly ConverterTypesService ConverterTypesService = types;

    [ObservableProperty] private ObservableCollection<ConvertDocumentViewModel> _Files = [];

    public void AddFile(string sourceFilePath, string converterName)
    {
        
    }

    public Task AddFileAsync(string sourceFilePath, string converterName)
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

    public string LoadAllText(string id)
    {
        throw new NotImplementedException();
    }

    public Task<string> LoadAllTextAsync(string id)
    {
        throw new NotImplementedException();
    }
}