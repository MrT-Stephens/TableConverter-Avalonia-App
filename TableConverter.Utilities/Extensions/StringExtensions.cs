namespace TableConverter.Utilities.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Checks if the input string contains only alphanumeric characters (letters and digits).
    /// </summary>
    /// <param name="input">
    /// The string to check.
    /// </param>
    /// <returns>
    /// True if the string contains only alphanumeric characters; otherwise, false.
    /// </returns>
    public static bool IsOnlyAlphaNumeric(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return input.All(char.IsLetterOrDigit);
    }
    
    /// <summary>
    /// Formats the input string with the provided arguments if the input is not null or empty and arguments are provided.
    /// </summary>
    /// <param name="input">
    /// The string to format.
    /// </param>
    /// <param name="args">
    /// The arguments to format the string with.
    /// </param>
    /// <returns>
    /// The formatted string if input is not null or empty and arguments are provided; otherwise, the original input string.
    /// </returns>
    public static string Format(this string input, params object[]? args)
    {
        if (string.IsNullOrEmpty(input) || args is null || args.Length == 0)
            return input;

        return string.Format(input, args);
    }
    
    /// <summary>
    /// Converts a camelCase or PascalCase string into a space-separated string.
    /// </summary>
    /// <param name="input">
    /// The camelCase or PascalCase string to convert.
    /// </param>
    /// <returns>
    /// A space-separated string with words separated by spaces.
    /// </returns>
    public static string UnCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        var words = input
            .Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString())
            .ToArray();
        
        return string.Concat(words);
    }
}