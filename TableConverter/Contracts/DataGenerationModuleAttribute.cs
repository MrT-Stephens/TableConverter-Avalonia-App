using System;

namespace TableConverter.Contracts;

[AttributeUsage(AttributeTargets.Class)]
public class DataGenerationModuleAttribute(string name, string description, string iconResourceName) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string IconResourceName { get; } = iconResourceName;
}