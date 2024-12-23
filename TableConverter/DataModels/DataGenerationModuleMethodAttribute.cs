using System;

namespace TableConverter.DataModels;

[AttributeUsage(AttributeTargets.Method)]
public class DataGenerationModuleMethodAttribute(string name, string description, string helpText = "") : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string HelpText { get; } = helpText;
}