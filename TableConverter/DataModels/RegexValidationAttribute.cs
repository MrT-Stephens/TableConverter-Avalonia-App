using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TableConverter.DataModels;

/// <summary>
///     Specifies that a string value must match a specified regular expression.
/// </summary>
public class RegexValidationAttribute : ValidationAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RegexValidationAttribute" /> class.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match.</param>
    public RegexValidationAttribute(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or whitespace.", nameof(pattern));

        Pattern = pattern;
    }

    /// <summary>
    ///     Gets the regular expression pattern used for validation.
    /// </summary>
    public string Pattern { get; }

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true; // Null values are considered valid to allow for optional fields.

        if (value is string stringValue) return Regex.IsMatch(stringValue, Pattern);

        return false; // Non-string values are invalid.
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name)
    {
        return $"{name} must match the pattern \"{Pattern}\".";
    }
}