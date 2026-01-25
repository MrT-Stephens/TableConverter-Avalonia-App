using Avalonia.Controls.Shapes;
using Avalonia.Media;

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
    
    /// <summary>
    /// Gets the icon path based on the IconName from the application resources.
    /// </summary>
    public StreamGeometry? IconPath { get; }
    
    /// <summary>
    /// Gets the category of the command.
    /// </summary>
    public string? Category { get; }
    
    /// <summary>
    /// Index which can be used to identify sub categories.
    /// </summary>
    public int? SubCategoryIndex { get; }
    
    /// <summary>
    /// Gets the key gestures which can be used to execute the command.
    /// </summary>
    public string[] KeyGestures { get; }
    
    /// <summary>
    /// Indicates whether the command can set the loading state of the application.
    /// </summary>
    public bool CanSetLoadingState { get;  }
    
    /// <summary>
    /// Indicates whether the command can set the loading state of the workspace.
    /// Only works if the parent is a workspace view model.
    /// </summary>
    public bool CanSetLoadingOnWorkspace { get; }
}