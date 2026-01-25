using System;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RuntimeValuesAttribute(string valuesPath) : Attribute
{
    public string ValuesPath { get; } = valuesPath;
}