namespace TableConverter.DataGeneration.Interfaces;

/// <summary>
/// Represents a module with a name, typically used to define components or sections of a larger system.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    public string ModuleName { get; }
}
