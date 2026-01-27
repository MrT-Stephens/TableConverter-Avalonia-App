using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace TableConverter.Views.Controls.Spreadsheet;

public class RowSplitter : Border
{
    public int RowId { get; }

    public RowSplitter(int rowId,double rowHeaderWidth)
    {
        Width = rowHeaderWidth;
        Height = 5;
        Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
        RowId = rowId;
        
#if DEBUG
        Background = Brushes.Transparent;
        ToolTip.SetTip(this, $"rowID={rowId}");
#else
        Background = Brushes.Transparent;
#endif
    }
}