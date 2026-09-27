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
    ///     with.
    /// </summary>
    /// <remarks>
    ///     Text by default, because most of what is generated is text: a name, a city or a phone number
    ///     is text however it is written, and only a handful of methods produce something a reader would
    ///     call a number or a date.
    ///     <para>
    ///         A method that can be told to wrap its number in a symbol still names the type of that
    ///         number, because the number is the value. Where the symbol is there by default the value is
    ///         text instead, which is why <c>Number.Percent</c> names text while <c>Commerce.Price</c>
    ///         names a decimal.
    ///     </para>
    /// </remarks>
    public ColumnDataType DataType { get; } = dataType;
}