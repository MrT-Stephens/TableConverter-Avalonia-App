using System;
using Avalonia;
using Avalonia.Controls.Shapes;
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
        if (!name.IsOnlyAlphaNumeric())
            throw new ArgumentException("Command name must be only alphanumeric.", nameof(name));

        Name = name;
        Title = null;
        Description = null;
        IconName = null;
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
    public CommandMetadata(string name, string title, string description, string iconName, string category = "") 
        : this(name)
    {
        Title = title;
        Description = description;
        IconName = iconName;
        Category = category;
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
    public Path? IconPath
    {
        get
        {
            if (IconName is null)
                return null;

            return Application.Current?.Resources[IconName] as Path
                   ?? throw new InvalidOperationException(
                       $"Icon with name '{IconName}' not found in application resources.");
        }
    }
}