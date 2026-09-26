namespace TableConverter.Utilities.Database;

/// <summary>
///     The direction a table is turned in when the whole of it is rotated.
/// </summary>
public enum TableRotation
{
    /// <summary>
    ///     A quarter turn to the right, which brings the left of the table to the top and its top to the
    ///     right.
    /// </summary>
    Clockwise,

    /// <summary>
    ///     A quarter turn to the left, which brings the right of the table to the top and its top to the
    ///     left.
    /// </summary>
    CounterClockwise,
}

