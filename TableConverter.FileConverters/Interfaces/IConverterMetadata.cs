namespace TableConverter.FileConverters.Interfaces;

[Flags]
public enum ConverterSupport
{
    None = 0,
    Input = 1 << 0,
    Output = 1 << 1,
}

public interface IConverterMetadata
{
    public string Name { get; }

    public IReadOnlyList<string> Extensions { get; }

    public IReadOnlyList<string> MimeTypes { get; }

    public IReadOnlyList<string> AppleUTIs { get; }

    public string Description { get; }

    public ConverterSupport Support { get; } 
}

