using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TableConverter.DataModels;

/// <summary>
///     Specifies that a value must be within a specified numeric range.
/// </summary>
public class RangeValidationAttribute<TNumeric> : ValidationAttribute where TNumeric : struct, IComparable<TNumeric>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RangeValidationAttribute{TNumeric}" /> class.
    /// </summary>
    /// <param name="minimum">The minimum allowable value.</param>
    /// <param name="maximum">The maximum allowable value.</param>
    public RangeValidationAttribute(TNumeric minimum, TNumeric maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    ///     Gets the minimum allowable value.
    /// </summary>
    public TNumeric Minimum { get; }

    /// <summary>
    ///     Gets the maximum allowable value.
    /// </summary>
    public TNumeric Maximum { get; }

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true; // Validation frameworks typically treat null as "not applicable"

        if (value is TNumeric numericValue)
            return Comparer<TNumeric>.Default.Compare(numericValue, Minimum) >= 0 &&
                   Comparer<TNumeric>.Default.Compare(numericValue, Maximum) <= 0;

        // Attempt conversion if the value is not already of type TNumeric
        try
        {
            var convertedValue = (TNumeric)Convert.ChangeType(value, typeof(TNumeric));
            return Comparer<TNumeric>.Default.Compare(convertedValue, Minimum) >= 0 &&
                   Comparer<TNumeric>.Default.Compare(convertedValue, Maximum) <= 0;
        }
        catch
        {
            return false; // If conversion fails, validation fails
        }
    }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be between {Minimum} and {Maximum}.";
    }
}