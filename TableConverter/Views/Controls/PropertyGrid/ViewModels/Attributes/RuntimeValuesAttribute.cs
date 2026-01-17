using System;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RuntimeValuesAttribute(string providerFunction) : Attribute
{
    public string ValuesPath { get; } = providerFunction;
}