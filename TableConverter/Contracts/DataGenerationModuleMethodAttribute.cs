using System;
using TableConverter.Utilities.Models;

namespace TableConverter.Contracts;

[AttributeUsage(AttributeTargets.Method)]
public class DataGenerationModuleMethodAttribute(
    string name,
    string description,
    string helpText = "",
    ColumnDataType dataType = ColumnDataType.Text) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string HelpText { get; } = helpText;

    /// <summary>
    ///     The kind of value the method produces, which is what a column generated from it is declared
    ///     with. Text by default, since a name, a city or a phone number is text however it is written
    ///     and only a handful of methods produce something a reader would call a number or a date.
    /// </summary>
    /// <remarks>
    ///     A method that wraps its number in a symbol still names the number's type, which is why
    ///     <c>Commerce.Price</c> names a decimal while <c>Number.Percent</c>, whose sign is there by
    ///     default, names text.
    /// </remarks>
    public ColumnDataType DataType { get; } = dataType;
}