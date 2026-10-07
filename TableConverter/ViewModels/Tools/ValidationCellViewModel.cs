using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace TableConverter.ViewModels.Tools;

/// <summary>
///     One cell that does not read as its column's type, as the validation panel lists it.
/// </summary>
/// <remarks>
///     The cell is addressed by the place its row holds in the table rather than by the row itself, because
///     that place is what the grid is driven by when the cell is jumped to. Jumping is handed to the panel
///     rather than done here, so a cell knows nothing of the table it came from.
/// </remarks>
public sealed class ValidationCellViewModel
{
    public ValidationCellViewModel(int rowPosition, string value, Func<ValidationCellViewModel, Task> jumpToCell)
    {
        RowPosition = rowPosition;
        Value = value;
        JumpCommand = new AsyncRelayCommand(() => jumpToCell(this));
    }

    /// <summary>
    ///     The place the row holds in the table, counting from one, which is what the row is labelled with.
    /// </summary>
    public int RowPosition { get; }

    /// <summary>
    ///     The value as it was stored, which is the text that does not read as its column's type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     The label the row is shown under, which is the place the row holds in the table.
    /// </summary>
    public string RowLabel => $"Row {RowPosition:N0}";

    /// <summary>
    ///     Puts the grid on the row that holds this value.
    /// </summary>
    public IAsyncRelayCommand JumpCommand { get; }
}

