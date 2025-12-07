using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.DataModels;

public class ConverterMetadata(
    string name,
    IReadOnlyList<string> extensions,
    IReadOnlyList<string> mimeTypes,
    IReadOnlyList<string> appleUTIs,
    string description,
    ConverterSupport support) 
    : IConverterMetadata
{
    public string Name { get; } = name;

    public IReadOnlyList<string> Extensions { get; } = extensions;

    public IReadOnlyList<string> MimeTypes { get; } = mimeTypes;

    public IReadOnlyList<string> AppleUTIs { get; } = appleUTIs;

    public string Description { get; } = description;

    public ConverterSupport Support { get; } = support;
}

