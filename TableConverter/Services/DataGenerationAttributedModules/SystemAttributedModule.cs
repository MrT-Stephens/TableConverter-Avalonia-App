using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("System",
    "Module for generating system-related data such as file paths, file types, and version numbers.",
    "DataGenerationSystemIcon")]
public class SystemAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : SystemModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("File Extension",
        "Generates a random file extension, optionally based on a provided mime type.")]
    public override string FileExtension(string mimeType = "")
    {
        return base.FileExtension(mimeType);
    }

    [DataGenerationModuleMethod("File Type", "Generates a random file type, such as 'text', 'image', 'audio', etc.")]
    public override string FileType()
    {
        return base.FileType();
    }

    [DataGenerationModuleMethod("Directory Path",
        "Generates a random directory path, typically consisting of several directory names.")]
    public override string DirectoryPath()
    {
        return base.DirectoryPath();
    }

    [DataGenerationModuleMethod("Common File Extension",
        "Generates a common file extension (e.g., '.txt', '.jpg', etc.).")]
    public override string CommonFileExtension()
    {
        return base.CommonFileExtension();
    }

    [DataGenerationModuleMethod("Common File Type",
        "Generates a common file type (e.g., 'text', 'image', 'audio', etc.).")]
    public override string CommonFileType()
    {
        return base.CommonFileType();
    }

    [DataGenerationModuleMethod("Common File Mime Type",
        "Generates a common MIME type (e.g., 'text/plain', 'image/jpeg', etc.).")]
    public override string CommonFileMimeType()
    {
        return base.CommonFileMimeType();
    }

    [DataGenerationModuleMethod("File Name", "Generates a random file name, optionally including an extension.")]
    public override string FileName(string extension = "")
    {
        return base.FileName(extension);
    }

    [DataGenerationModuleMethod("Common File Name",
        "Generates a common file name (e.g., 'document', 'image', 'audio', etc.), optionally with an extension.")]
    public override string CommonFileName(string extension = "")
    {
        return base.CommonFileName(extension);
    }

    [DataGenerationModuleMethod("File Path", "Generates a random file path, including directories and a file name.")]
    public override string FilePath()
    {
        return base.FilePath();
    }

    [DataGenerationModuleMethod("Semver", "Generates a random semantic versioning (semver) string (e.g., '1.0.0').")]
    public override string Semver()
    {
        return base.Semver();
    }
}