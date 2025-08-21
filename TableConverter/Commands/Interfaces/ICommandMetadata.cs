namespace TableConverter.Commands.Interfaces;

public interface ICommandMetadata
{
    /// <summary>
    /// Gets the name of the command, which is used to identify it.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the title of the command.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The name of the icon to be used for the command.
    /// </summary>
    public string? IconName { get; }
}