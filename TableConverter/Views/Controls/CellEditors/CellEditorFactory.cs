using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using TableConverter.Converters;
using TableConverter.Utilities.Models;

namespace TableConverter.Views.Controls.CellEditors;

/// <summary>
/// Builds the controls a table's cells are shown and entered with.
/// </summary>
/// <remarks>
/// A column's type decides both jobs, but reading a value and entering one are different jobs, so the
/// two are built apart. Every cell is read as text laid out the way its type reads, so a column of
/// numbers lines up the way it would in a spreadsheet; entering a value is done with whatever control
/// the type calls for, so a day is picked and a number is stepped rather than typed.
/// </remarks>
public static class CellEditorFactory
{
    /// <summary>
    /// Builds the control a cell's value is shown with.
    /// </summary>
    /// <param name="dataType">The type the column was given, which decides how the value reads.</param>
    /// <param name="valuePath">The path, from a row, of the cell's value.</param>
    /// <returns>The control a cell of this column is read with.</returns>
    public static Control CreateDisplay(ColumnDataType dataType, string valuePath)
    {
        return new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = dataType.IsNumeric() ? TextAlignment.Right : TextAlignment.Left,

            // What is shown is the value laid out the way the type reads, while what is stored stays
            // exactly what was entered: a number is grouped for reading without the grouping being
            // written back over it.
            [!TextBlock.TextProperty] = new Binding
            {
                Path = valuePath,
                Mode = BindingMode.TwoWay,
                Converter = new ColumnValueFormatConverter(dataType),
            },

            // A value that does not read as its column's type is marked rather than rejected, so the
            // styles can draw it in the theme's error colour while it stays editable.
            [!ColumnValueMismatch.IsMismatchedProperty] = new Binding
            {
                Path = valuePath,
                Converter = new ColumnValueMismatchConverter(dataType),
            },
        };
    }

    /// <summary>
    /// Builds the control a cell's value is entered with.
    /// </summary>
    /// <param name="dataType">The type the column was given, which decides the control to enter it with.</param>
    /// <param name="valuePath">The path, from a row, of the cell's value.</param>
    /// <returns>The control a cell of this column is entered with.</returns>
    /// <remarks>
    /// A typed editor hands its value over as soon as it changes rather than waiting for the cell to be
    /// left. Picking a day or stepping a number is one deliberate act, and waiting would risk the act
    /// being thrown away with the control: the grid tears the editor down as soon as the cell is left,
    /// so a value still on its way out at that moment would never arrive.
    /// </remarks>
    public static Control CreateEditor(ColumnDataType dataType, string valuePath)
    {
        if (dataType is ColumnDataType.Text)
        {
            // Text is what a keyboard is for, so a text column keeps the plain box it has always had.
            // What is typed counts once the box is left, so a half written value is never committed
            // to the store while it is still being written.
            return new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Left,
                [!TextBox.TextProperty] = new Binding
                {
                    Path = valuePath,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
                },
            };
        }

        // Every other type is entered with a control that can only name a value of that type, so a
        // typed column cannot be filled in with something that does not read as its type.
        return new CellValueEditor(dataType)
        {
            [!CellValueEditor.ValueProperty] = new Binding
            {
                Path = valuePath,
                Mode = BindingMode.TwoWay,
            },
        };
    }
}
