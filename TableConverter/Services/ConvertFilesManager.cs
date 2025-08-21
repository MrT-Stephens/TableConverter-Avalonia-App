using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public class ConvertFilesManager(IConverterTypes types) : IConvertFilesManager
{
    private static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Application.Current?.Name ??
            throw new ArgumentNullException(nameof(Application.Current.Name), "Name of the application is null"));

    private static readonly string AppFilesPath = Path.Combine(AppDataPath, "Files");

    private readonly IConverterTypes _ConverterTypes = types;

    public void AddFile(string sourceFilePath, string converterName)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFilePath));

        if (string.IsNullOrWhiteSpace(converterName))
            throw new ArgumentException("Converter name cannot be null or empty.", nameof(converterName));

        var converter = _ConverterTypes.GetInputConverter(converterName);

        if (converter is null)
            throw new InvalidOperationException($"No converter found for name: {converterName}");

        var document = new ConvertDocumentViewModel
        {
            InputConverter = converter,
        };

        if (!Directory.Exists(AppFilesPath))
        {
            Directory.CreateDirectory(AppFilesPath);
        }
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