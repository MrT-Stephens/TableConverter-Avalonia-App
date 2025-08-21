using System;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.Commands.DataModels;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute(string name, string title, string description, string? iconName = null)
    : Attribute, ICommandMetadata
{
    public string Name { get; } = name;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public string? IconName { get; } = iconName;
}