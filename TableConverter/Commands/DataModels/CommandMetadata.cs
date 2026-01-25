using System;
using Avalonia;
using Avalonia.Media;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Commands.DataModels;

public record CommandMetadata : ICommandMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandMetadata"/> class with the specified name.
    /// </summary>
    /// <param name="name">
    /// The name of the command. Must be alphanumeric.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided name is not alphanumeric.
    /// </exception>
    public CommandMetadata(string name)
    {
        if (!name.IsOnlyAlphaNumeric('.'))
            throw new ArgumentException("Command name must be only alphanumeric.", nameof(name));

        Name = name;
        Title = null;
        Description = null;
        IconName = null;
        CanSetLoadingState = false;
        CanSetLoadingOnWorkspace = false;
        KeyGestures = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandMetadata"/> class with the specified name, title, description, and icon name.
    /// </summary>
    /// <param name="name">
    /// The name of the command. Must be alphanumeric.
    /// </param>
    /// <param name="title">
    /// The title of the command.
    /// </param>
    /// <param name="description">
    /// The description of the command.
    /// </param>
    /// <param name="iconName">
    /// The name of the icon resource for the command.
    /// </param>
    /// <param name="category">
    /// The category of the command.
    /// </param>
    /// <param name="subCategoryIndex">
    /// Index which can be used to identify sub categories.
    /// </param>
    /// <param name="keyGestures">
    /// The key gestures which can be used to execute the command.
    /// </param>
    /// <param name="canSetLoadingState">
    /// Indicates whether the command can set the loading state of the application.
    /// </param>
    /// <param name="canSetLoadingOnWorkspace">
    /// Indicates whether the command can set the loading state of the workspace.
    /// Only works if the parent is a workspace view model.
    /// </param>
    public CommandMetadata(
        string name, 
        string title, 
        string description, 
        string iconName, 
        string category = "", 
        int? subCategoryIndex = null, 
        string[]? keyGestures = null, 
        bool canSetLoadingState = false,
        bool canSetLoadingOnWorkspace = false)
        : this(name)
    {
        Title = title;
        Description = description;
        IconName = iconName;
        Category = category;
        SubCategoryIndex = subCategoryIndex;
        KeyGestures = keyGestures ?? [];
        CanSetLoadingState = canSetLoadingState;
        CanSetLoadingOnWorkspace = canSetLoadingOnWorkspace;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string? Title { get; }

    /// <inheritdoc />
    public string? Description { get; }

    /// <inheritdoc />
    public string? IconName { get; }
    
    /// <inheritdoc />
    public string? Category { get; }

    /// <inheritdoc />
    public int? SubCategoryIndex { get; }

    /// <inheritdoc />
    public string[] KeyGestures { get; }
    
    /// <inheritdoc />
    public bool CanSetLoadingState { get; }
    
    /// <inheritdoc />
    public bool CanSetLoadingOnWorkspace { get; }

    /// <inheritdoc />
    public StreamGeometry? IconPath
    {
        get
        {
            if (IconName is null)
                return null;

            return Application.Current?.Resources[IconName] as StreamGeometry
                   ?? throw new InvalidOperationException(
                       $"Icon with name '{IconName}' not found in application resources.");
        }
    }
}