using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.Commands.DataModels;

public record CommandMetadata(string Name, string? Title = null, string? Description = null, string? IconName = null)
    : ICommandMetadata
{
    /// <inheritdoc />
    public string Name { get; } = Name;

    /// <inheritdoc />
    public string? Title { get; } = Title;

    /// <inheritdoc />
    public string? Description { get; } = Description;

    /// <inheritdoc />
    public string? IconName { get; } = IconName;
}