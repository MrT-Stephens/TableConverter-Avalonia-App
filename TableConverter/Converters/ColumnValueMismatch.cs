using Avalonia;
using Avalonia.Controls;

namespace TableConverter.Converters;

/// <summary>
/// Marks a control whose text shows a value that does not read as its column's type.
/// </summary>
/// <remarks>
/// The mark is carried on the control rather than turned into a brush where the cell is built, so the
/// colour standing for it can be chosen by a style, where it is a theme resource. A cell styled that way
/// is drawn in whatever the theme in force says, and follows the theme when it changes, which a brush
/// picked while the cell was being built could not.
/// The mark reaches the styles as the <c>:mismatch</c> class, since a class is what a style can match on.
/// </remarks>
public static class ColumnValueMismatch
{
    /// <summary>
    /// Whether the text the marked control shows reads as its column's type.
    /// </summary>
    public static readonly AttachedProperty<bool> IsMismatchedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsMismatched", typeof(ColumnValueMismatch));

    static ColumnValueMismatch()
    {
        IsMismatchedProperty.Changed.AddClassHandler<Control>(OnIsMismatchedChanged);
    }

    /// <summary>
    /// Reads whether the text <paramref name="control" /> shows reads as its column's type.
    /// </summary>
    /// <param name="control">The control showing the value.</param>
    /// <returns><see langword="true" /> when the value does not read as the column's type.</returns>
    public static bool GetIsMismatched(Control control)
    {
        return control.GetValue(IsMismatchedProperty);
    }

    /// <summary>
    /// Marks the text <paramref name="control" /> shows as not reading as its column's type.
    /// </summary>
    /// <param name="control">The control showing the value.</param>
    /// <param name="value"><see langword="true" /> when the value does not read as the column's type.</param>
    public static void SetIsMismatched(Control control, bool value)
    {
        control.SetValue(IsMismatchedProperty, value);
    }

    /// <summary>
    /// Hands the mark on to the control's class list, which is what the styles match against.
    /// </summary>
    private static void OnIsMismatchedChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        var classes = (IPseudoClasses)control.Classes;

        if (args.GetNewValue<bool>())
        {
            classes.Add(":mismatch");
        }
        else
        {
            classes.Remove(":mismatch");
        }
    }
}

