using System;

namespace TableConverter.Views.Controls.PropertyGrid;

[AttributeUsage(AttributeTargets.Property)]
public sealed class Ignore : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RuntimeValuesAttribute(string providerFunction)
    : Attribute
{
    public string ValuesPath { get; } = providerFunction;
}
